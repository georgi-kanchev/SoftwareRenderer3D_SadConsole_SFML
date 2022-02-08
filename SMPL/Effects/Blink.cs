using System;

namespace SMPL.Effects
{
	public class Blink : Effect
	{
		public float Speed { get; set; } = 1;
		public float TargetOpacity { get; set; }
		public float TimingOffset { get; set; }

		public override Data PerGlyph(Data input)
		{
			var c = input.BackgroundColorResult;
			var a = MathF.Sin((Time.GameClock + TimingOffset) * Speed).Map(-1, 1, TargetOpacity, 1);
			input.BackgroundColorResult = new(c.R / 255f, c.G / 255f, c.B / 255f, a);

			return input;
		}
	}
}
