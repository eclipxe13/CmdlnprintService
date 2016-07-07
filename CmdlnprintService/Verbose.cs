using System;
using System.IO;

namespace CmdlnprintService
{
	public class Verbose
	{
		readonly StreamWriter verboseStream = null;

		public string Header;
		public bool Muted;

		public Verbose (Stream outputStream): this(outputStream, "") {}

		public Verbose (Stream outputStream, string header): this(outputStream, header, true) {}

		public Verbose (Stream outputStream, string header, bool muted)
		{
			if (outputStream != null && outputStream.CanWrite) {
				verboseStream = new StreamWriter(outputStream);
			}
			Header = header;
			Muted = muted;
		}

		public void WriteLine (string message)
		{
			Write (message + Environment.NewLine);
		}

		public void Write (string message)
		{
			if (null == verboseStream || Muted) {
				return;
			}
			if (!string.IsNullOrEmpty (Header)) {
				message = "[" + Header + "] " + message;
			}
			message = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + "] " + message;
			verboseStream.Write (message);
			verboseStream.Flush ();
		}

	}
}

