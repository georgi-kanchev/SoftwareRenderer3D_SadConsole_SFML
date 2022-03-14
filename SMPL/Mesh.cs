using SadConsole;
using SFML.Graphics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace SMPL
{
	public class MeshDrawDetails
	{
		public ICellSurface Surface { get; set; }
		public Camera Camera { get; set; }
		public bool BackSideIsVisible { get; set; }
		public bool WireframeIsVisible { get; set; }
		public bool DepthIsIgnored { get; set; }
		public SadRogue.Primitives.Color WireframeColor { get; set; }
		public List<Effect> Effects { get; set; }
		public float[,] DepthBuffer { get; set; }
	}
	public class Mesh
	{
		public enum Shape { Quad, Cube, Pyramid, Sphere }

		private static readonly Dictionary<Shape, string> shapes = new()
		{
			{ Shape.Quad, @"
v -0.500000 -0.500000 0.000000
v 0.500000 -0.500000 0.000000
v 0.500000 0.500000 0.000000
v -0.500000 0.500000 0.000000

vt 1.000000 0.000000
vt 0.000000 0.000000
vt 0.000000 1.000000
vt 1.000000 1.000000

f 1/1 2/2 3/3
f 1/1 3/3 4/4

f 1/1 4/4 3/3
f 3/3 2/2 1/1" },
			{ Shape.Cube, @"
v 1.000000 -1.000000 -1.000000
v 1.000000 -1.000000 1.000000
v -1.000000 -1.000000 1.000000
v -1.000000 -1.000000 -1.000000
v 1.000000 1.000000 -1.000000
v 1.000000 1.000000 1.000000
v -1.000000 1.000000 1.000000
v -1.000000 1.000000 -1.000000

vt 1.000000 0.000000
vt 0.000000 1.000000
vt 0.000000 0.000000
vt 1.000000 1.000000

# front
f 1/1/6 4/3/6 8/2/6
f 1/1/6 8/2/6 5/4/6

# back
f 6/2/4 3/1/4 2/3/4
f 6/2/4 7/4/4 3/1/4

# top
f 8/2/2 6/1/2 5/4/2
f 8/2/2 7/3/2 6/1/2

# bottom
f 2/1/1 4/2/1 1/4/1
f 2/1/1 3/3/1 4/2/1

# right
f 5/2/3 2/1/3 1/3/3
f 5/2/3 6/4/3 2/1/3

# left
f 3/3/5 8/4/5 4/1/5
f 3/3/5 7/2/5 8/4/5" },
			{ Shape.Sphere, @"
v 0.000000 -1.000000 0.000000
v 0.723607 -0.447220 0.525725
v -0.276388 -0.447220 0.850649
v -0.894426 -0.447216 0.000000
v -0.276388 -0.447220 -0.850649
v 0.723607 -0.447220 -0.525725
v 0.276388 0.447220 0.850649
v -0.723607 0.447220 0.525725
v -0.723607 0.447220 -0.525725
v 0.276388 0.447220 -0.850649
v 0.894426 0.447216 0.000000
v 0.000000 1.000000 0.000000
v 0.425323 -0.850654 0.309011
v 0.262869 -0.525738 0.809012
v -0.162456 -0.850654 0.499995
v 0.425323 -0.850654 -0.309011
v 0.850648 -0.525736 0.000000
v -0.688189 -0.525736 0.499997
v -0.525730 -0.850652 0.000000
v -0.688189 -0.525736 -0.499997
v -0.162456 -0.850654 -0.499995
v 0.262869 -0.525738 -0.809012
v 0.951058 0.000000 -0.309013
v 0.951058 0.000000 0.309013
v 0.587786 0.000000 0.809017
v 0.000000 0.000000 1.000000
v -0.587786 0.000000 0.809017
v -0.951058 0.000000 0.309013
v -0.951058 0.000000 -0.309013
v -0.587786 0.000000 -0.809017
v 0.000000 0.000000 -1.000000
v 0.587786 0.000000 -0.809017
v 0.688189 0.525736 0.499997
v -0.262869 0.525738 0.809012
v -0.850648 0.525736 0.000000
v -0.262869 0.525738 -0.809012
v 0.688189 0.525736 -0.499997
v 0.525730 0.850652 0.000000
v 0.162456 0.850654 0.499995
v -0.425323 0.850654 0.309011
v -0.425323 0.850654 -0.309011
v 0.162456 0.850654 -0.499995

vt 0.205825 0.796643
vt 0.294076 0.836727
vt 0.194767 0.892391
vt 0.392861 0.882659
vt 0.392861 0.758866
vt 0.110211 0.815955
vt 0.158472 0.712693
vt 0.271600 0.725450
vt 0.230581 0.341262
vt 0.310841 0.381155
vt 0.221892 0.435870
vt 0.208124 0.214326
vt 0.203356 0.272468
vt 0.094549 0.208673
vt 0.375357 0.202247
vt 0.289955 0.200480
vt 0.333914 0.112837
vt 0.493319 0.318885
vt 0.442032 0.257470
vt 0.548340 0.256209
vt 0.397986 0.402164
vt 0.463376 0.367384
vt 0.481008 0.457571
vt 0.130940 0.330232
vt 0.208014 0.131625
vt 0.464517 0.160839
vt 0.586007 0.387930
vt 0.343728 0.480146
vt 0.602829 0.610867
vt 0.707137 0.674836
vt 0.590954 0.718476
vt 0.392861 0.728208
vt 0.488005 0.651345
vt 0.491646 0.774141
vt 0.440907 0.966089
vt 0.392861 0.852001
vt 0.514121 0.885417
vt 0.679959 0.992738
vt 0.558426 1.000000
vt 0.627249 0.898174
vt 0.785721 0.772652
vt 0.751262 0.893301
vt 0.675511 0.794912
vt 0.297716 0.959523
vt 0.182892 1.000000
vt 0.344814 0.644778
vt 0.078584 0.936032
vt 0.000000 0.838215
vt 0.034460 0.717567
vt 0.105763 0.618129
vt 0.227296 0.610867
vt 0.238979 0.578634
vt 0.000000 0.272627
vt 0.245559 0.000000
vt 0.604562 0.131369
vt 0.617757 0.497238
vt 0.061534 0.472487
vt 0.058506 0.081050
vt 0.449224 0.000000
vt 0.695428 0.313278
vt 0.442967 0.610867
vt 0.579896 0.814224

f 1/1/1 13/2/1 15/3/1
f 2/4/2 13/2/2 17/5/2
f 1/1/3 15/3/3 19/6/3
f 1/1/4 19/6/4 21/7/4
f 1/1/5 21/7/5 16/8/5
f 2/9/6 17/10/6 24/11/6
f 3/12/7 14/13/7 26/14/7
f 4/15/8 18/16/8 28/17/8
f 5/18/9 20/19/9 30/20/9
f 6/21/10 22/22/10 32/23/10
f 2/9/11 24/11/11 25/24/11
f 3/12/12 26/14/12 27/25/12
f 4/15/13 28/17/13 29/26/13
f 5/18/14 30/20/14 31/27/14
f 6/21/15 32/23/15 23/28/15
f 7/29/16 33/30/16 39/31/16
f 8/32/17 34/33/17 40/34/17
f 9/35/18 35/36/18 41/37/18
f 10/38/19 36/39/19 42/40/19
f 11/41/20 37/42/20 38/43/20
f 15/3/21 14/44/21 3/45/21
f 15/3/22 13/2/22 14/44/22
f 13/2/23 2/4/23 14/44/23
f 17/5/24 16/8/24 6/46/24
f 17/5/25 13/2/25 16/8/25
f 13/2/26 1/1/26 16/8/26
f 19/6/27 18/47/27 4/48/27
f 19/6/28 15/3/28 18/47/28
f 15/3/29 3/45/29 18/47/29
f 21/7/30 20/49/30 5/50/30
f 21/7/31 19/6/31 20/49/31
f 19/6/32 4/48/32 20/49/32
f 16/8/33 22/51/33 6/46/33
f 16/8/34 21/7/34 22/51/34
f 21/7/35 5/50/35 22/51/35
f 24/11/36 23/28/36 11/52/36
f 24/11/37 17/10/37 23/28/37
f 17/10/38 6/21/38 23/28/38
f 26/14/39 25/24/39 7/53/39
f 26/14/40 14/13/40 25/24/40
f 14/13/41 2/9/41 25/24/41
f 28/17/42 27/25/42 8/54/42
f 28/17/43 18/16/43 27/25/43
f 18/16/44 3/12/44 27/25/44
f 30/20/45 29/26/45 9/55/45
f 30/20/46 20/19/46 29/26/46
f 20/19/47 4/15/47 29/26/47
f 32/23/48 31/27/48 10/56/48
f 32/23/49 22/22/49 31/27/49
f 22/22/50 5/18/50 31/27/50
f 25/24/51 33/57/51 7/53/51
f 25/24/52 24/11/52 33/57/52
f 24/11/53 11/52/53 33/57/53
f 27/25/54 34/58/54 8/54/54
f 27/25/55 26/14/55 34/58/55
f 26/14/56 7/53/56 34/58/56
f 29/26/57 35/59/57 9/55/57
f 29/26/58 28/17/58 35/59/58
f 28/17/59 8/54/59 35/59/59
f 31/27/60 36/60/60 10/56/60
f 31/27/61 30/20/61 36/60/61
f 30/20/62 9/55/62 36/60/62
f 23/28/63 37/61/63 11/52/63
f 23/28/64 32/23/64 37/61/64
f 32/23/65 10/56/65 37/61/65
f 39/31/66 38/43/66 12/62/66
f 39/31/67 33/30/67 38/43/67
f 33/30/68 11/41/68 38/43/68
f 40/34/69 39/31/69 12/62/69
f 40/34/70 34/33/70 39/31/70
f 34/33/71 7/29/71 39/31/71
f 41/37/72 40/34/72 12/62/72
f 41/37/73 35/36/73 40/34/73
f 35/36/74 8/32/74 40/34/74
f 42/40/75 41/37/75 12/62/75
f 42/40/76 36/39/76 41/37/76
f 36/39/77 9/35/77 41/37/77
f 38/43/78 42/40/78 12/62/78
f 38/43/79 37/42/79 42/40/79
f 37/42/80 10/38/80 42/40/80" },
			{ Shape.Pyramid, @"
v -0.500000 -0.500000 0.500000
v 0.500000 -0.500000 0.500000
v 0.000000 0.500000 0.000000
v -0.500000 -0.500000 -0.500000
v 0.500000 -0.500000 -0.500000

vt 0.000000 0.000000
vt 1.000000 0.000000
vt 0.000000 1.000000
vt 1.000000 1.000000

f 1/1/1 2/2/1 3/3/1

f 4/2/3 3/3/3 5/1/3

f 4/1/4 5/2/4 1/3/4
f 1/3/4 5/2/4 2/4/4

f 2/1/5 5/2/5 3/3/5

f 4/1/6 1/2/6 3/3/6" }
		};
		internal Triangle[] triangles;

		public Area Area { get; } = new();
		public Image Image
		{
			get => triangles[0].Image;
			set
			{
				for (int i = 0; i < triangles.Length; i++)
					triangles[i].Image = value;
			}
		}
		public MeshDrawDetails DrawDetails { get; set; } = new();

		public void Draw()
		{
			var camera = DrawDetails.Camera ?? Camera.Main;
			var surface = DrawDetails.Surface ?? Simple.Console;
			var depthBuffer = DrawDetails.DepthBuffer ?? Triangle.zBuffer;
			var backSide = DrawDetails.BackSideIsVisible;
			var wireFrame = DrawDetails.WireframeIsVisible;
			var ignoreZBuffer = DrawDetails.DepthIsIgnored;
			var wireColor = DrawDetails.WireframeColor == default ? SadRogue.Primitives.Color.White : DrawDetails.WireframeColor;
			var effects = DrawDetails.Effects;

			//var zClippedTrigs = new List<Triangle>();
			//for (int i = 0; i < triangles.Length; i++)
			//{
			//	triangles[i].lightAmount = SadRogue.Primitives.Color.White;
			//	triangles[i].UpdatePoints(Area);
			//	triangles[i].CalculateNormal();
			//	triangles[i].ApplyLight(Light.Sun.ColorShadow, Light.Type.Ambient);
			//	triangles[i].ApplyLight(Light.Sun.ColorLight, Light.Type.Directional);
			//	triangles[i].AccountCamera(camera);
			//
			//	zClippedTrigs.AddRange(triangles[i].GetZClippedTriangles());
			//}
			//
			//for (int i = 0; i < zClippedTrigs.Count; i++)
			//{
			//	zClippedTrigs[i].ApplyPerspective(surface, camera);
			//	zClippedTrigs[i].CalculateNormalZ();
			//	zClippedTrigs[i].FixAffineCoordinates();
			//
			//	var clippedTrigs = zClippedTrigs[i].GetClippedTriangles(surface);
			//	for (int j = 0; j < clippedTrigs.Length; j++)
			//		clippedTrigs[j].Draw(surface, clippedTrigs[j].Image, backSide == false, effects, depthBuffer, wireFrame, wireColor, ignoreZBuffer);
			//}

			var zclip = new ConcurrentDictionary<int, Triangle[]>();
			Parallel.For(0, triangles.Length, delegate (int i)
			//for (int i = 0; i < triangles.Length; i++)
			{
				var trig = triangles[i];
				trig.lightAmount = SadRogue.Primitives.Color.White;
				trig.UpdatePoints(Area);
				trig.CalculateNormal();
				trig.ApplyLight(Light.Sun.ColorShadow, Light.Type.Ambient);
				trig.ApplyLight(Light.Sun.ColorLight, Light.Type.Directional);
				trig.AccountCamera(camera);
			
				var zClip = trig.GetZClippedTriangles();
				for (int k = 0; k < zClip.Length; k++)
				{
					var ztrig = zClip[k];
					ztrig.ApplyPerspective(surface, camera);
					ztrig.CalculateNormalZ();
					ztrig.FixAffineCoordinates();
			
					//zclip[i + k] = ztrig.GetClippedTriangles(surface);
			
					var clip = ztrig.GetClippedTriangles(surface);
					for (int j = 0; j < clip.Length; j++)
						clip[j].Draw(surface, clip[j].Image, backSide == false, effects, depthBuffer, wireFrame, wireColor, ignoreZBuffer);
				}
			});

			//foreach (var kvp in zclip)
			//	for (int i = 0; i < kvp.Value.Length; i++)
			//		kvp.Value[i].Draw(surface, kvp.Value[i].Image, backSide == false, effects, depthBuffer, wireFrame, wireColor, ignoreZBuffer);
		}

		public static Mesh Load(Shape shape, Image image = null)
		{
			return LoadFromText(shapes[shape], image);
		}
		public static Mesh Load(string fileName, Image image = null)
		{
			return fileName == null || File.Exists(fileName) == false ? default : LoadFromText(File.ReadAllText(fileName), image);
		}
		internal static Mesh LoadFromText(string fileContent, Image image)
		{
			if (fileContent == null)
				return default;

			var mesh = new Mesh();
			var lines = fileContent.Replace('\r', ' ').Split('\n');
			var indexTexCoords = new List<int>();
			var indexVert = new List<int>();
			var texCoords = new List<Vector3>();
			var verts = new List<Vector3>();
			for (int i = 0; i < lines.Length; i++)
			{
				var split = lines[i].Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
				if (split.Length == 0)
					continue;

				switch (split[0])
				{
					case "v": verts.Add(new(N(1), N(2), N(3))); break;
					case "vt": texCoords.Add(new(N(1), 1 - N(2), 1)); break;
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
					default,
					default,
					SadRogue.Primitives.Color.White);
				mesh.triangles[i / 3] = trig;
			}
			return mesh;
		}

		internal static List<Vector2> Outline(List<Vector2> points)
		{
			var result = new List<Vector2>();
			foreach (var p in points)
			{
				if (result.Count == 0)
					result.Add(p);
				else
				{
					if (result[0].X > p.X)
						result[0] = p;
					else if (result[0].X == p.X)
						if (result[0].Y > p.Y)
							result[0] = p;
				}
			}
			var counter = 0;
			while (counter < result.Count)
			{
				var q = Next(points, result[counter]);
				result.Add(q);
				if (q == result[0] || result.Count > points.Count)
					break;
				counter++;

				Vector2 Next(List<Vector2> points, Vector2 p)
				{
					Vector2 q = p;
					int t;
					foreach (Vector2 r in points)
					{
						t = ((q.X - p.X) * (r.Y - p.Y) - (r.X - p.X) * (q.Y - p.Y)).CompareTo(0);
						if (t == -1 || t == 0 && Vector2.Distance(p, r) > Vector2.Distance(p, q))
							q = r;
					}
					return q;
				}
			}
			result.Add(result[0]);
			return result;
		}
	}
}
