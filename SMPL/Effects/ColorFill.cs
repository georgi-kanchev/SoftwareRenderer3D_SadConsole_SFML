using SadRogue.Primitives;

namespace SMPL.Effects
{
	public class ColorFill : Effect
	{
		public Color Color { get; set; } = Color.White;

		public override Data PerGlyph(Data input)
		{
			input.Color = Color;
			return input;
		}
	}
}
