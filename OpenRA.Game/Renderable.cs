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
		public readonly string Palette;
		public readonly int? PalettePlayer;
		public readonly int Z;
		public readonly int ZOffset;
	    public float Scale;

        public Renderable(Sprite sprite, float2 pos, string palette, int? palettePlayer, int z, int zOffset, float scale)
        {
            Sprite = sprite;
            Pos = pos;
            Palette = palette;
			PalettePlayer = palettePlayer;
            Z = z;
            ZOffset = zOffset;
            Scale = scale;
        }

        public Renderable(Sprite sprite, float2 pos, string palette, int? palettePlayer, int z)
            : this(sprite, pos, palette, palettePlayer, z, 0, 1f) { }


        public Renderable(Sprite sprite, float2 pos, string palette, int? palettePlayer, int z, float scale)
            : this(sprite, pos, palette, palettePlayer, z, 0, scale) { }

        public Renderable WithPalette(string newPalette) { return new Renderable(Sprite, Pos, newPalette, PalettePlayer, Z, ZOffset, Scale); }
        public Renderable WithPalette(string newPalette, int? newPalettePlayer) { return new Renderable(Sprite, Pos, newPalette, newPalettePlayer, Z, ZOffset, Scale); }
        public Renderable WithZOffset(int newOffset) { return new Renderable(Sprite, Pos, Palette, PalettePlayer, Z, newOffset, Scale); }
		public Renderable WithPos(float2 newPos) { return new Renderable(Sprite, newPos, Palette, PalettePlayer, Z, ZOffset, Scale); }

		public static Renderable Centered( Sprite sprite, float2 pos, string palette, int? palettePlayer, int z )
		{
			return new Renderable( sprite, pos - 0.5f * sprite.size, palette, palettePlayer, z, 0, 1f );
		}
	}
}
