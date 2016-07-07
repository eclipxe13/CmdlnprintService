using System;
using System.Net;
using System.IO;
using System.Threading;
using System.Web;

namespace CmdlnprintService
{
	class RequestProcessor
	{
		readonly HttpListenerContext context;
		readonly Verbose verbose = null;
		string desktop;
		int timeout;

		public RequestProcessor (HttpListenerContext context, Verbose verbose, string desktop, int timeout)
		{
			this.context = context;
			this.verbose = verbose;
			this.desktop = desktop;
			this.timeout = timeout;
		}

		public void ProcessRequest ()
		{
			verbose.WriteLine ("ProcessRequest Start");

			// check that the request method id POST
			if ("POST" != context.Request.HttpMethod) {
				DoResponseError ("Invalid request http method");
				return;
			}
			// check that the request has some content
			if (!context.Request.HasEntityBody) {
				DoResponseError ("No client data was sent with the request");
				return;
			}

			// get the input
			string input;
			using (var reader = new StreamReader (context.Request.InputStream)) {
				input = reader.ReadToEnd();
			}
			verbose.WriteLine ("Input: " + input);

			// get the temp file name
			// string tempfile = Path.GetTempFileName ();
			string tempfile = CreateTempFileName();
			verbose.WriteLine ("Temporary file: " + tempfile);
			// create the Url2Pdf command object, if any Exception then return the message and exit
			try {
				var cmd = new Url2Pdf (verbose);
				cmd.SetupProperties (HttpUtility.ParseQueryString (input));
				cmd.SetOutputFile(tempfile);
				cmd.SetDesktop(desktop);
				cmd.Execute ();
			} catch (Exception e) {
				DoResponseError (e.Message);
				return;
			}

			// wait until OS detect the file exists for 5 seconds (500 ms x 10)
			if (!VerifyFileExists (tempfile, 500, timeout * 2)) {
				DoResponseError ("The file was not created");
				return;
			}

			// response with content of tempfile
			DoTransferFile (tempfile);

			// remove the file after the transfer
			verbose.WriteLine ("Removing file");
			File.Delete(tempfile);
		}

		bool VerifyFileExists (string filename, int msWait, int maxAttemps)
		{
			int numFileNotFound = 0;
			while (!File.Exists (filename)) {
				if (numFileNotFound++ >= maxAttemps) {
					return false;
				}
				verbose.WriteLine ("File " + filename + " not found, waiting for " + msWait + " ms [" + numFileNotFound + "/" + maxAttemps + "]");
				Thread.Sleep(msWait);
			}
			return true;
		}

		void DoTransferFile (string filename)
		{
			verbose.WriteLine ("Transferting file: " + filename);
			using (FileStream fs = File.OpenRead (filename)) {
				HttpListenerResponse response = context.Response;
				response.ContentLength64 = fs.Length;
				response.SendChunked = false;
				response.ContentType = System.Net.Mime.MediaTypeNames.Application.Octet;
				response.AddHeader ("Content-disposition", "attachment; filename=" + Path.GetFileName (filename));
				var buffer = new byte[ 64 * 1024 ];
				int totalbytes = 0;
				int read;
				using (var bw = new BinaryWriter (response.OutputStream)) {
					while ((read = fs.Read (buffer, 0, buffer.Length)) > 0) {
						totalbytes += read;
						Thread.Sleep (200); //take this out and it will not run
						bw.Write (buffer, 0, read);
						bw.Flush (); //seems to have no effect
					}
					bw.Close ();
					verbose.WriteLine ("Writen: " + totalbytes + " bytes");
				}
				response.OutputStream.Close ();
				verbose.WriteLine ("Response closed OK");
				response.Close ();
			}
		}


		void DoResponseError (string message)
		{
			context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			context.Response.StatusDescription = message;
			using (var sw = new StreamWriter (context.Response.OutputStream)) {
				sw.Write (message);
				sw.Flush ();
			}
			context.Response.Close ();
			verbose.WriteLine ("Response closed with error message: " + message);
		}

		string CreateTempFileName()
		{
			string tempdir = Path.Combine (Path.GetTempPath (), "CmdlnprintService");
			if (!Directory.Exists (tempdir)) {
				verbose.WriteLine ("Create temporary directory " + tempdir);
				try {
					Directory.CreateDirectory (tempdir);
				} catch (Exception ex) {
					verbose.WriteLine ("ERROR: Cannot create temporary directory " + tempdir);
					throw ex;
				}
			}
			return CreateTempFileName (tempdir);
		}

		string CreateTempFileName(string directory)
		{
			string filename;
			for (int attempt = 1; attempt <= 10; attempt = attempt + 1) {
				filename = Path.Combine (directory, Guid.NewGuid () + ".pdf");
				if (! File.Exists(filename)) {
					return filename;
				}
			}
			throw new Exception ("Unable to create a temporary file name in " + directory);
		}
	}
}

