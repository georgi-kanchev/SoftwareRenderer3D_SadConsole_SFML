using SadRogue.Primitives;
using System;

namespace SMPL.Effects
{
	public class Fog : Effect
	{
		public float Density { get; set; } = 0.1f;
		public Color Color { get; set; } = Color.SlateGray;
		public float MaxDistance { get; set; } = 120;

		public override Data PerGlyph(Data input)
		{
			var depth = input.Depth.Map(0, MaxDistance, 0, 1);
			input.Color = MixColors(Color, input.Color, Math.Clamp(depth * Density, 0, 1));
			return input;
		}
	}
}