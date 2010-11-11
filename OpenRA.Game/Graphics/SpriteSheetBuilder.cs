#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System.Linq;
using OpenRA.FileFormats;
using System.Collections.Generic;

namespace OpenRA.Graphics
{
	public static class SpriteSheetBuilder
	{
		public static void Initialize( TileSet tileset )
		{
			exts = tileset.Extensions;
			sprites = new Cache<string, Sprite[]>( LoadSprites );

			shpPalettes = new Dictionary<string, PaletteRef>();
			if( FileSystem.Exists( "palettes.yaml" ) )
				using( var shpPalettesFile = FileSystem.Open( "palettes.yaml" ) )
					foreach( var y in MiniYaml.FromStream( shpPalettesFile ) )
						shpPalettes.Add( y.Key, PaletteRef.Get( y.Value.Value ) );
		}

		static Cache<string, Sprite[]> sprites;
		static string[] exts;

		static Sprite[] LoadSprites(string filename)
		{
			var shp = new ShpReader(FileSystem.OpenWithExts(filename, exts));
			var palette = PaletteForShp( filename );
			return shp.Select(a => Game.modData.SheetBuilder.Add(a.Image, shp.Size, palette)).ToArray();
		}

		public static Sprite[] LoadAllSprites(string filename) { return sprites[filename]; }

		static Dictionary<string, PaletteRef> shpPalettes;
		static PaletteRef PaletteForShp( string filename )
		{
			PaletteRef ret;
			if( shpPalettes.TryGetValue( filename, out ret ) )
				return ret;

			if( filename.EndsWith( "icon" ) || filename.EndsWith( "ichn" ) )
				return PaletteRef.Get( "chrome" );
			return PaletteRef.Get( "player" );
		}
	}
}
