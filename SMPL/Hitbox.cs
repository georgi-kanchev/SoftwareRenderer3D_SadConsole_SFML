using System.Collections.Generic;
using System;
using SFML.Graphics;
using Color = SFML.Graphics.Color;
using System.Numerics;

namespace SMPL.Parts
{
	public class Hitbox
	{
		internal abstract class Shape
		{
			internal Texture defTex = new(new Image(new Color[,] { { Color.Green } }));
			internal Vector3 Position { get; set; }
			internal abstract bool Contains(Vector3 point);
			internal abstract bool Overlaps(Shape shape);
			internal abstract bool CrossesRay(Vector3 point, Vector3 direction, float distance, ref Vector3 hit);
			internal virtual void Draw(Camera camera = null) { }
		}
		internal class Box : Shape
		{
			internal Vector3 Size { get; set; }
			internal Vector3 Min => Position - Size * 0.5f;
			internal Vector3 Max => Position + Size * 0.5f;
			internal Vector3 A => new(Max.X, Max.Y, Max.Z);
			internal Vector3 B => new(Min.X, Max.Y, Max.Z);
			internal Vector3 C => new(Min.X, Min.Y, Max.Z);
			internal Vector3 D => new(Max.X, Min.Y, Max.Z);
			internal Vector3 E => new(Max.X, Max.Y, Min.Z);
			internal Vector3 F => new(Min.X, Max.Y, Min.Z);
			internal Vector3 G => new(Min.X, Min.Y, Min.Z);
			internal Vector3 H => new(Max.X, Min.Y, Min.Z);

			internal override bool Contains(Vector3 point) => point.X.IsBetween(Min.X, Max.X, true, true) && point.Y.IsBetween(Min.Y, Max.Y, true, true);
			internal bool Contains(Box box)
			{
				return Contains(box.A) && Contains(box.B) && Contains(box.C) && Contains(box.D) &&
					Contains(box.E) && Contains(box.F) && Contains(box.G) && Contains(box.H);
			}
			internal override bool Overlaps(Shape shape)
			{
				if (shape is Box box) return Overlaps(box);
				else if (shape is Sphere sphere) return Overlaps(sphere);
				return false;
			}
			internal bool Overlaps(Box box)
			{
				return Contains(box.A) || Contains(box.B) || Contains(box.C) || Contains(box.D) ||
					Contains(box.E) || Contains(box.F) || Contains(box.G) || Contains(box.H);
			}
			internal bool Overlaps(Sphere sphere)
			{
				if (sphere.Position.IsNaN())
					return false;
				float sqDist = 0f;
				for (int i = 0; i < 3; i++)
				{
					var v = sphere.Position.IndexToAxis(i);
					if (v < Min.IndexToAxis(i)) sqDist += (Min.IndexToAxis(i) - v) * (Min.IndexToAxis(i) - v);
					if (v > Max.IndexToAxis(i)) sqDist += (v - Max.IndexToAxis(i)) * (v - Max.IndexToAxis(i));
				}
				return sqDist <= sphere.Radius * sphere.Radius;
			}
			internal override bool CrossesRay(Vector3 point, Vector3 direction, float distance, ref Vector3 hit)
			{
				float t1 = (Min.X - point.X) / direction.X;
				float t2 = (Max.X - point.X) / direction.X;
				float t3 = (Min.Y - point.Y) / direction.Y;
				float t4 = (Max.Y - point.Y) / direction.Y;
				float t5 = (Min.Z - point.Z) / direction.Z;
				float t6 = (Max.Z - point.Z) / direction.Z;

				float tmin = MathF.Max(MathF.Max(MathF.Min(t1, t2), MathF.Min(t3, t4)), MathF.Min(t5, t6));
				float tmax = MathF.Min(MathF.Min(MathF.Max(t1, t2), MathF.Max(t3, t4)), MathF.Max(t5, t6));

				hit = new Vector3().NaN();
				// if tmax < 0, ray (line) is intersecting AABB, but whole AABB is behing us
				// if tmin > tmax, ray doesn't intersect AABB
				if (tmax < 0 || tmin > tmax)
					return false;

				var t = tmin;
				if (tmin < 0f)
					t = tmax;

				if (t > distance)
					return false;

				hit = point + direction * t;
				return true;
			}
		}
		internal class Sphere : Shape
		{
			internal float Radius { get; set; }

			internal override bool Contains(Vector3 point) => Vector3.Distance(point, Position) <= Radius;
			internal bool Contains(Sphere sphere) => Vector3.Distance(Position, sphere.Position) + sphere.Radius <= Radius;
			internal override bool Overlaps(Shape shape)
			{
				if (shape is Box box) return Overlaps(box);
				else if (shape is Sphere sphere) return Overlaps(sphere);
				return false;
			}
			internal bool Overlaps(Sphere sphere) => Vector3.Distance(Position, sphere.Position) - sphere.Radius <= Radius;
			internal bool Overlaps(Box box) => box.Overlaps(this);
			internal override bool CrossesRay(Vector3 point, Vector3 direction, float distance, ref Vector3 hit)
			{
				var m = point - Position;
				var b = Vector3.Dot(m, direction);
				var c = Vector3.Dot(m, m) - Radius * Radius;
				hit = new Vector3().NaN();

				// Exit if r’s origin outside s (c > 0) and r pointing away from s (b > 0) 
				if (c > 0.0f && b > 0.0f) return false;
				var discr = b * b - c;

				// A negative discriminant corresponds to ray missing sphere 
				if (discr < 0.0f) return false;

				// Ray now found to intersect sphere, compute smallest t value of intersection
				var t = -b - MathF.Sqrt(discr);

				// If t is negative, ray started inside sphere so clamp t to zero 
				if (t < 0.0f) t = 0.0f;

				if (t > distance)
					return false;

				hit = point + direction * t;
				return true;
			}
		}

		internal readonly Dictionary<string, Shape> shapes = new();

		public static Hitbox Raycast(Vector3 point, Vector3 targetPoint, List<Hitbox> hitboxes)
		{
			return Raycast(point, targetPoint - point, hitboxes);
		}
		public static Hitbox Raycast(Vector3 point, Vector3 direction, float distance, List<Hitbox> hitboxes)
		{
			if (hitboxes == default || hitboxes.Count == 0)
				return default;

			var bestDist = distance;
			var bestResult = default(Hitbox);
			for (int i = 0; i < hitboxes.Count; i++)
			{
				var hit = hitboxes[i].CrossesRay(point, direction, distance);
				var dist = Vector3.Distance(point, hit);
				if (hit != point && dist < bestDist)
				{
					bestDist = dist;
					bestResult = hitboxes[i];
				}
			}
			return bestResult;
		}

		public void SetShape(string name, Vector3 position, float radius) => shapes[name] = new Sphere() { Position = position, Radius = radius };
		public void SetShape(string name, Vector3 position, Vector3 size) => shapes[name] = new Box() { Position = position, Size = size };
		public void RemoveShape(string name) { if (name != null && shapes.ContainsKey(name)) shapes.Remove(name); }
		public bool Overlaps(List<Hitbox> hitboxes)
		{
			if (hitboxes == null || hitboxes.Count == 0)
				return false;

			for (int i = 0; i < hitboxes.Count; i++)
				foreach (var kvp in hitboxes[i].shapes)
					foreach (var kvp2 in shapes)
						if (kvp.Value.Overlaps(kvp2.Value))
							return true;
			return false;
		}

		public Vector3 CrossesRay(Vector3 point, Vector3 targetPoint)
		{
			return CrossesRay(point, targetPoint - point, Vector3.Distance(point, targetPoint));
		}
		public Vector3 CrossesRay(Vector3 point, Vector3 direction, float distance)
		{
			var bestDist = distance;
			var bestResult = point;
			foreach (var kvp in shapes)
			{
				var hit = new Vector3().NaN();
				kvp.Value.CrossesRay(point, Vector3.Normalize(direction), distance, ref hit);

				var dist = Vector3.Distance(point, hit);
				if (dist < bestDist)
				{
					bestDist = dist;
					bestResult = hit;
				}
			}
			return bestResult == point ? new Vector3(float.NaN, float.NaN, float.NaN) : bestResult;
		}
	}
}
