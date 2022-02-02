using System.Numerics;

namespace SMPL
{
	internal struct Vertex
	{
		public Vector3 Position { get; set; }
		public Vector3 TexCoords { get; set; }

		public Vertex(Vector3 pos, Vector3 texCoords)
		{
			Position = pos;
			TexCoords = texCoords;
		}
	}
}
