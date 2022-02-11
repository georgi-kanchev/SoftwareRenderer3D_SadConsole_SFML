using SadConsole;
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
			public ICellSurface Surface { get; set; }
			public Image Image { get; set; }
			public bool IsVisible { get; set; }

			public Vector2 LineScreenStart { get; set; }
			public Vector2 LineScreenEnd { get; set; }

			public Vector3 TrianglePointScreenA { get; internal set; }
			public Vector3 TrianglePointScreenB { get; internal set; }
			public Vector3 TrianglePointScreenC { get; internal set; }

			public Vector2 CurrentPosition { get; set; }
			public Vector2 CurrentImagePosition { get; set; }
			public int CurrentGlyphCount { get; set; }

			public float Depth { get; set; }
			public Color Color { get; set; }
			public int Glyph { get; set; }
			public Color GlyphColor { get; set; }
		}
		
		public abstract Data PerGlyph(Data input);

		protected static Color ColorFromImage(uint x, uint y, Image image)
		{
			var w = image == null ? 1 : image.Size.X;
			var h = image == null ? 1 : image.Size.Y;
			var u = Math.Clamp(x, 0, w - 1);
			var v = Math.Clamp(y, 0, h - 1);
			var c = image == null ? SFML.Graphics.Color.White : image.GetPixel(u, v);
			return new(c.R, c.G, c.B, c.A);
		}
		protected static Color MixColors(Color colorA, Color colorB, float amount)
		{
			var r = (int)(colorA.R * amount + colorB.R * (1 - amount));
			var g = (int)(colorA.G * amount + colorB.G * (1 - amount));
			var b = (int)(colorA.B * amount + colorB.B * (1 - amount));
			return new(r, g, b);
		}
	}
}
