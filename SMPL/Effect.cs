using SFML.Graphics;
using System;
using System.Numerics;
using Color = SadRogue.Primitives.Color;

namespace SMPL
{
	public abstract class Effect
	{
		public class Data
		{
			public Vector2 LineScreenStart { get; set; }
			public Vector2 LineScreenEnd { get; set; }

			public Vector2 CurrentPosition { get; set; }
			public Vector2 CurrentTexturePosition { get; set; }

			public float DepthPrevious { get; set; }
			public float DepthResult { get; set; }

			public Color BackgroundColorPrevious { get; set; }
			public Color BackgroundColorResult { get; set; }

			public int GlyphPrevious { get; set; }
			public int GlyphResult { get; set; }

			public Color GlyphColorPrevious { get; set; }
			public Color GlyphColorResult { get; set; }
		}
		
		public abstract Data PerGlyph(Data input);

		protected static Color ColorFromImage(int x, int y, Image image)
		{
			var w = image == null ? 1 : image.Size.X;
			var h = image == null ? 1 : image.Size.Y;
			var u = Math.Clamp(x, 0, w - 1);
			var v = Math.Clamp(y, 0, h - 1);
			var c = image == null ? SFML.Graphics.Color.White : image.GetPixel((uint)u, (uint)v);
			return new(c.R, c.G, c.B, c.A);
		}
		protected static Color Mix(Color colorA, Color colorB, float amount)
		{
			var r = (int)(colorA.R * amount + colorB.R * (1 - amount));
			var g = (int)(colorA.G * amount + colorB.G * (1 - amount));
			var b = (int)(colorA.B * amount + colorB.B * (1 - amount));
			return new(r, g, b);
		}
	}
}
