using SadRogue.Primitives;

namespace SMPL.Effects
{
	public class Wireframe : Effect
	{
		public Color Color { get; set; } = Color.White;

		public override Data PerGlyph(Data input)
		{
			var x = input.CurrentPosition.X;
			input.BackgroundColor = x == input.LineScreenStart.X || x == input.LineScreenEnd.X ? Color : Color.Transparent;
			return input;
		}
	}
}
