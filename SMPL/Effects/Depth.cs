namespace SMPL.Effects
{
	public class Depth : Effect
	{
		public float Value { get; set; } = float.NaN;

		public override Data PerGlyph(Data input)
		{
			input.Depth += float.IsNaN(Value) ? 0 : Value;
			return input;
		}
	}
}