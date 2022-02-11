using SadRogue.Primitives;
using System;
using System.Numerics;

namespace SMPL.Effects
{
	public class ColorAdjust : Effect
	{
		public float Gamma { get; set; }
		public float Desaturation { get; set; }
		public float Inversion { get; set; }
		public float Contrast { get; set; }
		public float Brightness { get; set; }

		public override Data PerGlyph(Data input)
		{
			var c = new Vector3(input.Color.R / 255f, input.Color.G / 255f, input.Color.B / 255f);
			var gamma = 1f / (1f - Gamma);
			c = new(MathF.Pow(c.X, gamma), MathF.Pow(c.Y, gamma), MathF.Pow(c.Z, gamma));
			var d = 0.2126f * c.X + 0.7152f * c.Y + 0.0722f * c.Z;
			var desat = MixColors(new(c.X, c.Y, c.Z), new(d, d, d), Math.Clamp(1 - Desaturation, 0, 1));
			var inv = MixColors(desat, new Color(255 - desat.R, 255 - desat.G, 255 - desat.B), Math.Clamp(1 - Inversion, 0, 1));
			c = new(inv.R / 255f, inv.G / 255f, inv.B / 255f);
			var half = new Vector3(0.5f, 0.5f, 0.5f);
			c = (c - half) * ((Contrast + 1f) / (1f - Contrast)) + half;
			c += new Vector3(Brightness, Brightness, Brightness);

			input.Color = new(c.X, c.Y, c.Z);
			return input;
		}
	}
}
