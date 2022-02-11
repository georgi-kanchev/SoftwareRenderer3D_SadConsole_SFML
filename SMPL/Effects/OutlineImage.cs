using SadRogue.Primitives;
using System;
using System.Numerics;

namespace SMPL.Effects
{
	public class OutlineImage : Effect
	{
		public Color Color { get; set; } = Color.White;
		public uint Thickness { get; set; } = 1;

		public override Data PerGlyph(Data input)
		{
			if (input.Color.A == 255)
				for (float i = 1; i <= Thickness; i++)
					if (HasNoNeighbourPixel(-i, 0) || HasNoNeighbourPixel(i, 0) || HasNoNeighbourPixel(0, i) || HasNoNeighbourPixel(0, -i))
						input.Color = Color;
				
			return input;

			bool HasNoNeighbourPixel(float offsetX, float offsetY)
			{
				var p = input.CurrentImagePosition + new Vector2(offsetX, offsetY);
				return ColorFromImage((uint)p.X, (uint)p.Y, input.Image).A < 255 ||
					p.X < 0 || p.Y < 0 || p.X > input.Image.Size.X || p.Y > input.Image.Size.Y;
			}
		}
	}
}