using SadRogue.Primitives;
using System.Numerics;

namespace SMPL.Effects
{
	public class ColorTint : Effect
	{
		public Color Color { get; set; } = Color.White;

		public override Data PerGlyph(Data input)
		{
			var c = input.BackgroundColorResult;
			var target = new Vector4(Color.R / 255f, Color.G / 255f, Color.B / 255f, Color.A / 255f);
			var result = new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
			result *= target;
			input.BackgroundColorResult = new Color(result.X, result.Y, result.Z, result.W);
			return input;
		}
	}
}
