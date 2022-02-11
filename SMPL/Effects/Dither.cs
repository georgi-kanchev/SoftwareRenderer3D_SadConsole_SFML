using System;
using System.Numerics;

namespace SMPL.Effects
{
	public class Dither : Effect
	{
		public bool Flip { get; set; }
		public uint Pattern1 { get; set; }
		public uint Pattern2 { get; set; }
		public uint Pattern3 { get; set; }
		public uint Pattern4 { get; set; }

		public override Data PerGlyph(Data input)
		{
			if (input.Color.A < 255)
				return input;

			var p1 = input.CurrentPosition.X % (Pattern1 + 1);
			var p2 = input.CurrentPosition.Y % (Pattern2 + 1);
			var p3 = input.CurrentPosition.X % (Pattern3 + 1);
			var p4 = input.CurrentPosition.Y % (Pattern4 + 1);
			
			if (Flip && ((p1 != 0 && p2 != 1) || (p3 != 0 && p4 != 1)))
					input.IsVisible = false;
			else if (Flip == false && ((p1 == 0 && p2 == 1) || (p3 == 0 && p4 == 1)))
					input.IsVisible = false;

			return input;
		}
	}
}
