#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;

namespace OpenRA.FileFormats
{
	public class Mod
	{
		public string Title;
		public string Description;
		public string Version;
		public string Author;
		public string Requires;
		public bool Standalone = false;

        public static readonly Dictionary<string, Mod> AllMods = ValidateMods(
            P.E("mods").GetDirs().Select(x => x.BaseName()).ToArray());

		public static Dictionary<string, Mod> ValidateMods(PathElement[] mods)
		{
			var ret = new Dictionary<string, Mod>();
			foreach (var m in mods)
			{
                var manifestFile = P.E("mods") / m / "mod.yaml";
				if (!manifestFile.Exists())
					continue;

				var yaml = new MiniYaml(null, MiniYaml.FromFile(manifestFile));
				if (!yaml.NodesDict.ContainsKey("Metadata"))
					continue;

				ret.Add(m.ToString(), FieldLoader.Load<Mod>(yaml.NodesDict["Metadata"]));
			}
			return ret;
		}
	}
}
