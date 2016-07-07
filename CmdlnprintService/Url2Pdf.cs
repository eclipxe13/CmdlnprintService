using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace CmdlnprintService
{

	class Url2Pdf
	{
		// private properties
		readonly Verbose verbose;
		string outputfile;
		string desktop;
		string url = "";
		string orientation = "";
		string bgColors = "";
		string bgImages = "";
		string shrinkToFit = "";
		string paperCustom = "";
		string paperUnits = "";
		float paperWidth = -1;
		float paperHeight = -1;
		float marginTop = -1;
		float marginRight = -1;
		float marginBottom = -1;
		float marginLeft = -1;

		// Constructor
		public Url2Pdf (Verbose verbose)
		{
			this.verbose = verbose;
		}

		public string Url {
			get { return url; }
			set {
				if (null == value) {
					throw new ArgumentException ("Invalid property URL");
				}
				string urltest = value;
				Uri urivar;
				if (!Uri.TryCreate (urltest, UriKind.Absolute, out urivar)) {
					throw new ArgumentException ("Invalid property URL");
				}
				url = urltest;
			}
		}

		public string Orientation
		{
			get { return orientation; }
			set {
				if (value == null || ("" != value && "default" != value && "landscape" != value && "portaint" != value)) {
					throw new ArgumentException("Invalid property Orientation");
				}
				orientation = value;
			}
		}

		public string BgColors
		{
			get { return bgColors; }
			set {
				if (value == null || ("" != value && "default" != value && "yes" != value && "no" != value)) {
					throw new ArgumentException("Invalid property BGColors");
				}
				bgColors = value;
			}
		}

		public string BgImages
		{
			get { return bgImages; }
			set {
				if (value == null || ("" != value && "default" != value && "yes" != value && "no" != value)) {
					throw new ArgumentException("Invalid property BGImages");
				}
				bgImages = value;
			}
		}

		public string ShrinkToFit
		{
			get { return shrinkToFit; }
			set {
				if (value == null || ("" != value && "default" != value && "yes" != value && "no" != value)) {
					throw new ArgumentException("Invalid property ShrinkToFit");
				}
				shrinkToFit = value;
			}
		}

		public string PaperCustom
		{
			get { return paperCustom; }
			set {
				if (value == null || ("" != value && "yes" != value && "no" != value)) {
					throw new ArgumentException("Invalid property CustomPaper");
				}
				paperCustom = value;
			}
		}

		public string PaperUnits
		{
			get { return paperUnits; }
			set {
				if (value == null || ("" != value && "in" != value && "mm" != value)) {
					throw new ArgumentException("Invalid property CustomPaperUnits");
				}
				paperUnits = value;
			}
		}

		// property set from NameValueCollection
		public void SetupProperties (NameValueCollection props)
		{
			foreach (string key in props.Keys) {
				if ("url" == key) {
					Url = props[key];
					continue;
				}
				if ("orientation" == key) {
					Orientation = props[key];
					continue;
				}
				if ("bgcolors" == key) {
					BgColors = props[key];
					continue;
				}
				if ("bgimages" == key) {
					BgImages = props[key];
					continue;
				}
				if ("shrinktofit" == key) {
					ShrinkToFit = props[key];
					continue;
				}
				if ("custompaper" == key) {
					PaperCustom = props[key];
					continue;
				}
				if ("custompaperunits" == key) {
					PaperUnits = props[key];
					continue;
				}
				if ("custompaperwidth" == key) {
					paperWidth = float.Parse(props[key]);
					continue;
				}
				if ("custompaperheight" == key) {
					paperHeight = float.Parse(props[key]);
					continue;
				}
				if ("margintop" == key) {
					marginTop = float.Parse(props[key]);
					continue;
				}
				if ("marginright" == key) {
					marginRight = float.Parse(props[key]);
					continue;
				}
				if ("marginbottom" == key) {
					marginBottom = float.Parse(props[key]);
					continue;
				}
				if ("marginleft" == key) {
					marginLeft = float.Parse(props[key]);
					continue;
				}
			}
		}

		public void SetOutputFile (string filename)
		{
			outputfile = filename;
		}

		public void SetDesktop (string desktop)
		{
			this.desktop = desktop;
		}

		static string EscapeArgument (string argument)
		{
			return argument.Replace("\"", "\\\"");
		}


		string BuildArguments ()
		{
			if ("" == Url) {
				throw new Exception ("URL has not been set");
			}
			if ("" == outputfile) {
				throw new Exception ("Output File has not been set");
			}
			var sb = new StringBuilder ();
			sb.AppendFormat (" --display=\":{0}\"", desktop);
			sb.AppendFormat (" -print \"{0}\"", EscapeArgument (Url));
			sb.AppendFormat (" -print-file \"{0}\"", EscapeArgument (outputfile));
			if ("" != Orientation) {
				sb.AppendFormat (" -print-orientation \"{0}\"", Orientation);
			}
			if ("" != BgColors) {
				sb.AppendFormat (" -print-bgcolors \"{0}\"", BgColors);
			}
			if ("" != BgImages) {
				sb.AppendFormat (" -print-bgimages \"{0}\"", BgImages);
			}
			if ("" != ShrinkToFit) {
				sb.AppendFormat (" -print-shrinktofit \"{0}\"", ShrinkToFit);
			}
			if ("" != PaperCustom && (paperWidth > 0 || paperHeight > 0)) {
				sb.AppendFormat (" -print-paper-custom \"{0}\"", PaperCustom);
				if ("" != PaperUnits) {
					sb.AppendFormat (" -print-paper-units \"{0}\"", PaperUnits);
				}
				if (paperWidth > 0) {
					sb.AppendFormat (" -print-paper-width \"{0}\"", paperWidth.ToString("0.0###"));
				}
				if (paperHeight > 0) {
					sb.AppendFormat (" -print-paper-height \"{0}\"", paperHeight.ToString ("0.0###"));
				}
			}
			if (marginTop > 0) {
				sb.AppendFormat (" -print-margin-top \"{0}\"", marginTop.ToString ("0.0###"));
			}
			if (marginRight > 0) {
				sb.AppendFormat (" -print-margin-right \"{0}\"", marginRight.ToString ("0.0###"));
			}
			if (marginBottom > 0) {
				sb.AppendFormat (" -print-margin-bottom \"{0}\"", marginBottom.ToString ("0.0###"));
			}
			if (marginLeft > 0) {
				sb.AppendFormat (" -print-margin-left \"{0}\"", marginLeft.ToString ("0.0###"));
			}
			// set fixed header and footer to yes but without definition
			sb.Append (" -print-mode \"pdf\"  -print-header \"yes\" -print-footer \"yes\" "); // fixed values
			return sb.ToString();
		}

		public string Execute ()
		{
			const string program = "/usr/bin/firefox";
			Process process = new Process ();
			process.StartInfo.FileName = program;
			process.StartInfo.Arguments = BuildArguments ();
			if (process.StartInfo.EnvironmentVariables.ContainsKey ("DISPLAY")) {
				process.StartInfo.EnvironmentVariables["DISPLAY"] = ":" + desktop;
			} else {
				process.StartInfo.EnvironmentVariables.Add("DISPLAY", ":" + desktop);
			}
			verbose.WriteLine("Run: " + program + " " + process.StartInfo.Arguments);
    		process.StartInfo.UseShellExecute = false;
    		process.StartInfo.RedirectStandardError = true;
    		process.StartInfo.RedirectStandardOutput = true;
    		var stdOutput = new StringBuilder();
    		process.OutputDataReceived += (sender, args) => stdOutput.Append(args.Data);
    		string stdError;
    		try {
        		process.Start();
        		process.BeginOutputReadLine();
        		stdError = process.StandardError.ReadToEnd();
        		process.WaitForExit();
    		} catch (Exception e) {
        		throw new Exception("Error while running " + program + ":" + Environment.NewLine + e.Message, e);
			}
    		if (process.ExitCode != 0) {
        		var message = new StringBuilder();
				message.AppendFormat("{0}: finished with exit code [{1}]", program, process.ExitCode);
				message.AppendLine();
				if (!string.IsNullOrEmpty(stdError)) {
            		message.AppendLine(stdError);
        		}
        		if (stdOutput.Length != 0) {
            		message.AppendLine(stdOutput.ToString());
        		}
        		throw new Exception(message.ToString());
    		}
    		return stdOutput.ToString();
		}
	}
}

