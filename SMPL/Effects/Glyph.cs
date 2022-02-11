using System;
using System.Numerics;

namespace SMPL.Effects
{
	public class Glyph : Effect
	{
		public override Data PerGlyph(Data input)
		{
			var c = new Vector3(input.Color.R / 255f, input.Color.G / 255f, input.Color.B / 255f);
			var d = 0.2126f * c.X + 0.7152f * c.Y + 0.0722f * c.Z;
			return input;
		}
	}
}
