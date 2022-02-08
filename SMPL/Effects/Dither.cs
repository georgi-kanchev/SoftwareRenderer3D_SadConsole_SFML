using System.Numerics;

namespace SMPL.Effects
{
	public class Dither : Effect
	{
		static readonly float[] hardCodes =
		{
				1f / 17f,  9f / 17f,  3f / 17f, 11f / 17f,
				13f / 17f,  5f / 17f, 15f / 17f,  7f / 17f,
				4f / 17f, 12f / 17f,  2f / 17f, 10f / 17f,
				16f / 17f,  8f / 17f, 14f / 17f,  6f / 17f
		};

		public override Data PerGlyph(Data input)
		{
			//float2 uv = ScreenPosition.xy * _ScreenParams.xy;
			//
			//uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
			//Out = In - DITHER_THRESHOLDS[index];
			//input.BackgroundColorResult = new(result.X, result.Y, result.Z, dest.W);
			return input;
		}
	}
}
