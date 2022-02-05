using SadConsole;
using SFML.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SMPL
{
	public class Mesh
	{
		public Area Area { get; } = new();
		internal Triangle[] triangles;

		public void Draw(Console console, Camera camera, bool showBackSide = false)
		{
			var zClippedTrigs = new List<Triangle>();
			for (int i = 0; i < triangles.Length; i++)
			{
				triangles[i].lightAmount = SadRogue.Primitives.Color.White;
				triangles[i].UpdatePoints(Area);
				triangles[i].CalculateNormal();
				triangles[i].ApplyLight(Light.Sun.ColorShadow, Light.Type.Ambient);
				triangles[i].ApplyLight(Light.Sun.ColorLight, Light.Type.Directional);
				triangles[i].AccountCamera(camera);

				zClippedTrigs.AddRange(triangles[i].GetZClippedTriangles());
			}

			for (int i = 0; i < zClippedTrigs.Count; i++)
			{
				zClippedTrigs[i].ApplyPerspective(console);
				zClippedTrigs[i].CalculateNormalZ();
				zClippedTrigs[i].FixAffineCoordinates();

				var clippedTrigs = zClippedTrigs[i].GetClippedTriangles(console);
				for (int j = 0; j < clippedTrigs.Length; j++)
					clippedTrigs[j].Draw(console, clippedTrigs[j].Image, showBackSide == false);
			}
		}
		public static Mesh Load(string filePath, Image image)
		{
			if (filePath == null || File.Exists(filePath) == false)
				return default;

			var mesh = new Mesh();
			var lines = File.ReadAllLines(filePath);
			var indexTexCoords = new List<int>();
			var indexVert = new List<int>();
			var indexNorm = new List<int>();
			var texCoords = new List<Vector3>();
			var verts = new List<Vector3>();
			var norms = new List<Vector3>();
			for (int i = 0; i < lines.Length; i++)
			{
				var split = lines[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
				if (split.Length == 0)
					continue;

				switch (split[0])
				{
					case "v": verts.Add(new(N(1), N(2), N(3))); break;
					case "vt": texCoords.Add(new(N(1), 1 - N(2), 1)); break;
					case "vn": norms.Add(new Vector3(N(1), N(2), N(3))); break;
					case "f":
						{
							// if a polygon has more than 3 vertices, split it into triangles
							if (split.Length > 4)
							{
								var newSplit = new List<string>() { split[0] };
								for (int j = 2; j < split.Length - 1; j++)
								{
									newSplit.Add(split[1]);
									newSplit.Add(split[j]);
									newSplit.Add(split[j + 1]);
								}
								split = newSplit.ToArray();
							}
							for (int j = 1; j < split.Length; j++)
							{
								var face = split[j].Split('/');
								indexVert.Add(int.Parse(face[0]) - 1);

								if (face.Length > 1 && face[1].Length != 0)
									indexTexCoords.Add(int.Parse(face[1]) - 1);

								if (face.Length > 2)
									indexNorm.Add(int.Parse(face[2]) - 1);
							}
							break;
						}
				}
				float N(int i) => float.Parse(split[i]);
			}

			mesh.triangles = new Triangle[indexVert.Count / 3];
			for (int i = 0; i < indexVert.Count; i += 3)
			{
				var trig = new Triangle(
					new(verts[indexVert[i]], i < indexTexCoords.Count ? texCoords[indexTexCoords[i]] : default),
					new(verts[indexVert[i + 1]], i + 1 < indexTexCoords.Count ? texCoords[indexTexCoords[i + 1]] : default),
					new(verts[indexVert[i + 2]], i + 2 < indexTexCoords.Count ? texCoords[indexTexCoords[i + 2]] : default),
					image,
					norms[indexNorm[i]],
					0,
					SadRogue.Primitives.Color.White);
				mesh.triangles[i / 3] = trig;
			}
			return mesh;
		}
	}
}
