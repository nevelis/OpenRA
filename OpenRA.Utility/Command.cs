#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using OpenRA.GameRules;
using OpenRA.FileFormats;

namespace OpenRA.Utility
{
	static class Command
	{
		public static void ExtractZip(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Error: Invalid syntax");
				return;
			}

			var zipFile = args[1];
			var dest = P.E(args[2]);
			
			if (!File.Exists(zipFile))
			{
				Console.WriteLine("Error: Could not find {0}", zipFile);
				return;
			}
			
			List<string> extracted = new List<string>();
			try
			{
				new ZipInputStream(File.OpenRead(zipFile)).ExtractZip(dest, extracted);
			}
			catch (SharpZipBaseException)
			{
				foreach(var f in extracted)
					File.Delete(f);
				Console.WriteLine("Error: Corrupted archive");
				return;
			}
			Console.WriteLine("Status: Completed");
		}

		static void InstallPackages(PathElement fromPath, PathElement toPath,
			string[] filesToCopy, string[] filesToExtract, string packageToMount)
		{
            toPath.CreateDir();

			Util.ExtractFromPackage(fromPath.ToString(), packageToMount, filesToExtract, toPath.ToString());
			foreach (var file in filesToCopy)
			{
                var fromFilename = fromPath / file;
                if (!fromFilename.Exists())
				{
					Console.WriteLine("Error: Could not find {0}", file);
					return;
				}

				Console.WriteLine("Status: Extracting {0}", file.ToLowerInvariant());
				File.Copy(
					fromFilename.ToString(),
					(toPath / P.E(file.ToLowerInvariant()).BaseName()).ToString(), true);   // some expressions do not get clearer.
			}

			Console.WriteLine("Status: Completed");
		}

        // todo: push these file lists into the mod manifests themselves.
		
		public static void InstallRAPackages(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Error: Invalid syntax");
				return;
			}

			InstallPackages(P.E(args[1]), P.E(args[2]),
				new string[] { "INSTALL/REDALERT.MIX" },
				new string[] { "conquer.mix", "russian.mix", "allies.mix", "sounds.mix",
					"scores.mix", "snow.mix", "interior.mix", "temperat.mix" },
				"MAIN.MIX");
		}

		public static void InstallCncPackages(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Error: Invalid syntax");
				return;
			}

			InstallPackages(P.E(args[1]), P.E(args[2]),
				new string[] { "CONQUER.MIX", "DESERT.MIX", "GENERAL.MIX", "SCORES.MIX",
					"SOUNDS.MIX", "TEMPERAT.MIX", "WINTER.MIX" },
				new string[] { "cclocal.mix", "speech.mix", "tempicnh.mix", "updatec.mix" },
				"INSTALL/SETUP.Z");
		}

        public static void DisplayFilepicker(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Error: Invalid syntax");
                return;
            }

            using (var dialog = new OpenFileDialog() { Title = args[1] })
                if (dialog.ShowDialog() == DialogResult.OK)
                    Console.WriteLine(dialog.FileName);
        }

		public static void Settings(string[] args)
		{
			if (args.Length < 3)
			{
				Console.WriteLine("Error: Invalid syntax");
				return;
			}
			
			var section = args[2].Split('.')[0];
			var field = args[2].Split('.')[1];
			var expandedPath = P.E(args[1]).ExpandHomeDir();
			var settings = new Settings(expandedPath / "settings.yaml", Arguments.Empty);
			var result = settings.Sections[section].GetType().GetField(field).GetValue(settings.Sections[section]);
			Console.WriteLine(result);
		}

        static void AuthenticateAndExecute(string cmd, string[] args)
        {
            for (var i = 1; i < args.Length; i++)
                cmd += " \"{0}\"".F(args[i]);
            Util.CallWithAdmin(cmd);
        }

        public static void AuthenticateAndExtractZip(string[] args) { AuthenticateAndExecute("--extract-zip-inner", args); }
		public static void AuthenticateAndInstallRAPackages(string[] args) { AuthenticateAndExecute( "--install-ra-packages-inner", args ); }
		public static void AuthenticateAndInstallCncPackages(string[] args) { AuthenticateAndExecute( "--install-cnc-packages-inner", args ); }
	}
}
