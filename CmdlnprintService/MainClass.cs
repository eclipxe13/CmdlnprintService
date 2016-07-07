using System;
using System.Net;
using System.Threading;

namespace CmdlnprintService
{
	class MainClass
	{

		public static void ShowSyntax()
		{
			Console.WriteLine("This command start a web server to invoke firefox to create a pdf file");
			Console.WriteLine("Syntax: cmdlnprint-service.exe [--desktop desktop-number] [--ip ip-address] [--port port] [--timeout seconds] [--silent | -s]");
			Console.WriteLine("--ip ip-address defaults to 127.0.0.1");
			Console.WriteLine("--port defaults to 9085");
			Console.WriteLine("--desktop defaults to 0");
			Console.WriteLine("--timeout seconds default to 5");
			Console.WriteLine("--silent mute all verbose output");

		}

		public static int Main (string[] args)
		{
			// arguments
			var verbose = new Verbose(Console.OpenStandardOutput(), "", false);
			string ipaddress = "127.0.0.1";
			string port = "9085";
			string desktop = "0";
			string timeout = "5";

			for (int i = 0; i < args.Length; i++) {
				if ("-h" == args [i] || "--help" == args [i]) {
					MainClass.ShowSyntax ();
					return 0;
				}
				if ("-s" == args [i] || "--silent" == args [i]) {
					verbose.Muted = true;
				} else if ("--ip" == args [i] && i + 1 < args.Length) {
					ipaddress = args [++i];
				} else if ("--port" == args [i] && i + 1 < args.Length) {
					port = args [++i];
				} else if ("--desktop" == args [i] && i + 1 < args.Length) {
					desktop = args [++i];
				} else if ("--timeout" == args [i] && i + 1 < args.Length) {
					timeout = args [++i];
				} else {
					Console.WriteLine ("Invalid argument {0}", args [i]);
					MainClass.ShowSyntax ();
					return 1;
				}
			}

			verbose.WriteLine("desktop: " + desktop);
			verbose.WriteLine("ipaddress: " + ipaddress);
			verbose.WriteLine("port: " + port);
			verbose.WriteLine("timeout: " + timeout);

			// validate ip address argument
			IPAddress net_ip;
			if (ipaddress != "*") {
				if (!IPAddress.TryParse (ipaddress, out net_ip)) {
					Console.WriteLine ("Invalid IP Address {0}", ipaddress);
					return 1;
				}
				if (net_ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) {
					Console.WriteLine ("Invalid IP Address {0}", ipaddress);
					return 1;
				}
			}

			// validate port argument
			UInt16 net_port;
			if (!UInt16.TryParse (port, out net_port)) {
				Console.WriteLine ("Invalid port number {0}", port);
				return 1;
			}
			if (net_port < 1 || net_port > 65535) {
				Console.WriteLine ("Invalid port number {0}", port);
				return 1;
			}

			// validate desktop argument
			int desktopnumber;
			if (!int.TryParse(desktop, out desktopnumber)) {
				Console.WriteLine ("Invalid desktop number {0}", desktop);
				return 1;
			}
			if (desktopnumber < 0 || desktopnumber > 255) {
				Console.WriteLine ("Invalid desktop number {0}", desktop);
				return 1;
			}

			// validate timeout argument
			int timeoutnumber;
			if (!int.TryParse (timeout, out timeoutnumber)) {
				Console.WriteLine("Invalid timeout number {0}", timeout);
			} else if (timeoutnumber < 1 || timeoutnumber > 120) {
				Console.WriteLine ("Invalid timeout number {0}", timeout);
				return 1;
			}

			// create web server main process
			var listener = new HttpListener ();
			try {
				string prefix = "http://" + ipaddress + ":" + port + "/";
				verbose.WriteLine("Prefix: " + prefix);
				listener.Prefixes.Add(prefix);
				verbose.WriteLine("Starting listener...");
				listener.Start ();
				verbose.WriteLine("Listener started...");
				while (true) {
					verbose.WriteLine("Waiting for new context...");
					var ctx = listener.GetContext ();

					verbose.WriteLine("Context received, creating a Request Processor");
					var rp = new RequestProcessor (ctx, verbose, desktop, timeoutnumber);

					verbose.WriteLine("Opening a new thread for Request Processor");
					var t = new Thread (rp.ProcessRequest);

					verbose.WriteLine("Starting the thread...");
					t.Start ();
					verbose.WriteLine("Thread started");

				}
			} catch (Exception e) {
				verbose.WriteLine("Error: " + e.Message);
				Console.WriteLine("Error: " + e.Message);
				if (listener.IsListening) {
					listener.Stop();
				}
				return 2;
			}
//			listener.Stop ();
//			return 0;
		}
	}



}
