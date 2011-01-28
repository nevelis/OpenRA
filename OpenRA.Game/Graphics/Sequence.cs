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
using OpenRA.FileFormats;

namespace OpenRA.Graphics
{
	public class SequenceData
	{
		public readonly int Start, Length, Facings, Tick;
		public readonly string Name, Src;

		const int DefaultTick = 40;

		/* todo: if sequenceEditor is going to be able to handle facings, tick, etc properly, need more! */
		public SequenceData(string name, string src, int start, int length)
		{
			Name = name;
			Src = src;
			Start = start;
			Length = length;
			Facings = 1;
			Tick = DefaultTick;
		}

		public SequenceData(string unit, string name, MiniYaml info)
		{
			Name = name;
			Src = info.Value ?? unit;

			var d = info.NodesDict;
			Start = int.Parse(d["Start"].Value);
			if (!d.ContainsKey("Length"))
				Length = 1;
			else if (d["Length"].Value == "*")
				Length = -1;
			else
				Length = int.Parse(d["Length"].Value);

			if (d.ContainsKey("Facings"))
				Facings = int.Parse(d["Facings"].Value);
			else
				Facings = 1;

			if (d.ContainsKey("Tick"))
				Tick = int.Parse(d["Tick"].Value);
			else
				Tick = DefaultTick;
		}

		public int ActualLength(int shpLength) { return (Length == -1) ? shpLength - Start : Length; }

		public MiniYaml Save()
		{
			var root = new List<MiniYamlNode>();

			root.Add(new MiniYamlNode("Start", Start.ToString()));

			if (Length > 1)
				root.Add(new MiniYamlNode("Length", Length.ToString()));
			else if (Length == -1)
				root.Add(new MiniYamlNode("Length", "*"));

			if (Facings > 1)
				root.Add(new MiniYamlNode("Facings", Facings.ToString()));

			if (Tick != DefaultTick)
				root.Add(new MiniYamlNode("Tick", Tick.ToString()));

			return new MiniYaml(Src, root);
		}
	}

	public class Sequence
	{
		readonly Sprite[] sprites;
		public readonly int Start, End, Length, Facings, Tick;
		public readonly string Name;
		SequenceData data;

		public Sequence(string unit, string name, MiniYaml info)
		{
			data = new SequenceData(unit, name, info);

			Name = name;
			var d = info.NodesDict;

			sprites = Game.modData.SpriteLoader.LoadAllSprites(data.Src);

			Start = data.Start;
			Length = data.ActualLength(sprites.Length);
			End = Start + Length;
			Facings = data.Facings;
			Tick = data.Tick;
		}

		public MiniYaml Save() { return data.Save(); }
		
		public Sprite GetSprite( int frame )
		{
			return GetSprite( frame, 0 );
		}

		public Sprite GetSprite(int frame, int facing)
		{
			var f = Traits.Util.QuantizeFacing( facing, Facings );
			return sprites[ (f * Length) + ( frame % Length ) + Start ];
		}
	}
}
