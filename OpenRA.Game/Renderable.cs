using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.Graphics;

namespace OpenRA
{
	public struct Renderable
	{
		public readonly Sprite Sprite;
		public readonly float2 Pos;
		public readonly PaletteRef Palette;
		public readonly int? PalettePlayer;
		public readonly int Z;
		public readonly int ZOffset;
	    public float Scale;

        public Renderable(Sprite sprite, float2 pos, PaletteRef palette, int? palettePlayer, int z, int zOffset, float scale)
        {
            Sprite = sprite;
            Pos = pos;
            Palette = palette;
			PalettePlayer = palettePlayer;
            Z = z;
            ZOffset = zOffset;
            Scale = scale;
        }

        public Renderable(Sprite sprite, float2 pos, PaletteRef palette, int? palettePlayer, int z)
            : this(sprite, pos, palette, palettePlayer, z, 0, 1f) { }

		public Renderable(Sprite sprite, float2 pos, PaletteRef palette, int? palettePlayer, int z, float scale)
            : this(sprite, pos, palette, palettePlayer, z, 0, scale) { }

        public Renderable WithPalette(PaletteRef newPalette) { return new Renderable(Sprite, Pos, newPalette, PalettePlayer, Z, ZOffset, Scale); }
        public Renderable WithPalette(PaletteRef newPalette, int? newPalettePlayer) { return new Renderable(Sprite, Pos, newPalette, newPalettePlayer, Z, ZOffset, Scale); }
        public Renderable WithZOffset(int newOffset) { return new Renderable(Sprite, Pos, Palette, PalettePlayer, Z, newOffset, Scale); }
		public Renderable WithPos(float2 newPos) { return new Renderable(Sprite, newPos, Palette, PalettePlayer, Z, ZOffset, Scale); }

		public static Renderable Centered( Sprite sprite, float2 pos, PaletteRef palette, int? palettePlayer, int z )
		{
			return new Renderable( sprite, pos - 0.5f * sprite.size, palette, palettePlayer, z, 0, 1f );
		}
	}

	public class PaletteRef
	{
		readonly string paletteName;
		WorldRenderer cachedWR;
		int[] cachedPalettes;

		private PaletteRef( string paletteName )
		{
			this.paletteName = paletteName;
		}

		public int PaletteIndex( WorldRenderer wr, int? playerIndex )
		{
			var i = playerIndex ?? -1;
			if( i >= 16 || i < 0 )
				return wr.GetPaletteIndex( paletteName, playerIndex );
			if( cachedWR != wr )
			{
				cachedPalettes = new int[ 16 ];
				cachedWR = wr;
			}
			if( cachedPalettes[ i ] == 0 )
				cachedPalettes[ i ] = wr.GetPaletteIndex( paletteName, i );

			return cachedPalettes[ i ];
		}

		static readonly Dictionary<string, PaletteRef> paletterefs = new Dictionary<string, PaletteRef>();
		public static PaletteRef Get( string name )
		{
			if( name == null ) return null;
			return paletterefs.GetOrAdd( name, x => new PaletteRef( x ) );
		}

		public static readonly PaletteRef Player = PaletteRef.Get( "player" );
		public static readonly PaletteRef Shadow = PaletteRef.Get( "shadow" );
		public static readonly PaletteRef Effect = PaletteRef.Get( "effect" );
		public static readonly PaletteRef Invuln = PaletteRef.Get( "invuln" );
		public static readonly PaletteRef Chrome = PaletteRef.Get( "chrome" );
		public static readonly PaletteRef Terrain = PaletteRef.Get( "terrain" );
		public static readonly PaletteRef Disabled = PaletteRef.Get( "disabled" );
		public static readonly PaletteRef Highlight = PaletteRef.Get( "highlight" );
	}
}
