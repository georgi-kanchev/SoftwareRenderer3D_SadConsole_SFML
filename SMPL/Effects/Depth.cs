namespace SMPL.Effects
{
	public class Depth : Effect
	{
		public float Value { get; set; } = float.NaN;
		public bool IsVisible { get; set; }

		public override Data PerGlyph(Data input)
		{
			input.Depth += float.IsNaN(Value) ? 0 : Value;
			var d = input.Depth / 255f;
			if (IsVisible)
				input.Color = new SadRogue.Primitives.Color(d, d, d);
			return input;
		}
	}
}