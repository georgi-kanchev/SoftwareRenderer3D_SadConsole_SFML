using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SadConsole.UI;
using SadConsole.UI.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Numerics;
using System.Reflection;
using System.Text;
using static System.MathF;

namespace SMPL
{
	internal class JsonBinder : ISerializationBinder
	{
		internal static JsonBinder Instance = new();
		public List<Type> KnownTypes { get; set; } = new();
		public JsonBinder()
		{
			KnownTypes.AddRange(Assembly.GetEntryAssembly().GetTypes());
			KnownTypes.AddRange(Assembly.GetCallingAssembly().GetTypes());

			KnownTypes.Add(typeof(bool)); KnownTypes.Add(typeof(char)); KnownTypes.Add(typeof(string)); KnownTypes.Add(typeof(double));
			KnownTypes.Add(typeof(float)); KnownTypes.Add(typeof(decimal)); KnownTypes.Add(typeof(long)); KnownTypes.Add(typeof(ulong));
			KnownTypes.Add(typeof(byte)); KnownTypes.Add(typeof(sbyte)); KnownTypes.Add(typeof(short)); KnownTypes.Add(typeof(ushort));
			KnownTypes.Add(typeof(int)); KnownTypes.Add(typeof(uint));

			var types = new List<Type>(KnownTypes);
			for (int i = 0; i < types.Count; i++)
			{
				var listType = Activator.CreateInstance(typeof(List<>).MakeGenericType(types[i])).GetType();
				var arrType = types[i].MakeArrayType();
				KnownTypes.Add(listType);
				KnownTypes.Add(arrType);
			}
			KnownTypes.Add(typeof(Dictionary<string, string>)); KnownTypes.Add(typeof(Dictionary<string, int>));
			KnownTypes.Add(typeof(Dictionary<string, bool>)); KnownTypes.Add(typeof(Dictionary<string, double>));
			KnownTypes.Add(typeof(Dictionary<bool, string>)); KnownTypes.Add(typeof(Dictionary<bool, int>));
			KnownTypes.Add(typeof(Dictionary<bool, bool>)); KnownTypes.Add(typeof(Dictionary<bool, double>));
			KnownTypes.Add(typeof(Dictionary<int, string>)); KnownTypes.Add(typeof(Dictionary<int, int>));
			KnownTypes.Add(typeof(Dictionary<int, bool>)); KnownTypes.Add(typeof(Dictionary<int, double>));
			KnownTypes.Add(typeof(Dictionary<double, string>)); KnownTypes.Add(typeof(Dictionary<double, int>));
			KnownTypes.Add(typeof(Dictionary<double, bool>)); KnownTypes.Add(typeof(Dictionary<double, double>));
		}
		public Type BindToType(string assemblyName, string typeName)
		{
			for (int i = 0; i < KnownTypes.Count; i++)
				if (KnownTypes[i].ToString() == typeName)
					return KnownTypes[i];
			return default;
		}
		public void BindToName(Type serializedType, out string assemblyName, out string typeName)
		{
			assemblyName = null;
			typeName = serializedType.ToString();
		}
	}
	public static class Extensions
	{
		public enum ChoiceLimit { ClosestBound, Overflow }
		public enum ChoiceRoundWay { Closest, Up, Down }
		public enum ChoiceRoundWhen5
		{
			TowardEven, AwayFromZero, TowardZero, TowardNegativeInfinity, TowardPositiveInfinity
		}
		public enum ChoiceSizeToSize
		{
			Bit_Byte, Bit_KB,
			Byte_Bit, Byte_KB, Byte_MB,
			KB_Bit, KB_Byte, KB_MB, KB_GB,
			MB_Byte, MB_KB, MB_GB, MB_TB,
			GB_KB, GB_MB, GB_TB,
			TB_MB, TB_GB
		}
		public enum ChoiceAnimation
		{
			BendWeak, // Sine
			Bend, // Cubic
			BendStrong, // Quint
			Circle, // Circ
			Elastic, // Elastic
			Swing, // Back
			Bounce // Bounce
		}
		public enum ChoiceAnimationCurve { In, Out, InOut }

		public static Vector3 Translate(this Vector3 point, Vector3 translation)
		{
			point.X += translation.X;
			point.Y += translation.Y;
			point.Z += translation.Z;
			return point;
		}
		public static Vector3 Rotate(this Vector3 point, Vector3 rotation)
		{
			// this is the new rotation code using quaternions, helps with the first person camera problems of gimbal lock
			var v = point;
			v = Vector3.Transform(v, Quaternion.CreateFromAxisAngle(new(0, 1, 0), rotation.Y));
			v = Vector3.Transform(v, Quaternion.CreateFromAxisAngle(new(1, 0, 0), -rotation.X));
			v = Vector3.Transform(v, Quaternion.CreateFromAxisAngle(new(0, 0, 1), rotation.Z));
			return v;

			// this is the old rotation code used by the BuildSucceeded tutorial
			//var result = new Vector3()
			//{
			//	X = point.X * (Cos(rotation.Z) * Cos(rotation.Y)) +
			//	point.Y * (Cos(rotation.Z) * Sin(rotation.Y) * Sin(rotation.X) - Sin(rotation.Z) * Cos(rotation.X)) +
			//	point.Z * (Cos(rotation.Z) * Sin(rotation.Y) * Cos(rotation.X) + Sin(rotation.Z) * Sin(rotation.X)),
			//	Y = point.X * (Sin(rotation.Z) * Cos(rotation.Y)) +
			//	point.Y * (Sin(rotation.Z) * Sin(rotation.Y) * Sin(rotation.X) + Cos(rotation.Z) * Cos(rotation.X)) +
			//	point.Z * (Sin(rotation.Z) * Sin(rotation.Y) * Cos(rotation.X) - Cos(rotation.Z) * Sin(rotation.X)),
			//	Z = point.X * (-Sin(rotation.Y)) +
			//	point.Y * (Cos(rotation.Y) * Sin(rotation.X)) +
			//	point.Z * (Cos(rotation.Y) * Cos(rotation.X)),
			//};
			//return result;
		}
		public static Vector3 ApplyPerspective(this Vector3 point, float camResolutionX, float camFieldOfView)
		{
			var Z0 = (camResolutionX / 2f) / Tan((camFieldOfView / 2f) * PI / 180f);
			point.X *= Z0 / (Z0 + point.Z);
			point.Y *= Z0 / (Z0 + point.Z);
			point.Z *= Z0 / (Z0 + point.Z);
			return point;
		}
		public static Vector3 FixAffineCoordinates(this Vector3 texCoords, float pointZ, float camResolutionX, float camFieldOfView)
		{
			var Z0 = (camResolutionX / 2f) / Tan((camFieldOfView / 2f) * PI / 180f);
			texCoords.X *= Z0 / (Z0 + pointZ);
			texCoords.Y *= Z0 / (Z0 + pointZ);
			texCoords.Z *= Z0 / (Z0 + pointZ);
			return texCoords;
		}
		public static Vector3 CenterScreen(this Vector3 point, Vector2 size)
		{
			return new(point.X + size.X * 0.5f, point.Y + size.Y * 0.5f, point.Z);
		}

		public static T Choose<T>(this List<T> list)
		{
			return list[Random(0, list.Count - 1)];
		}
		public static void Shuffle<T>(this List<T> list)
		{
			var n = list.Count;
			while (n > 1)
			{
				n--;
				var k = new Random().Next(n + 1);
				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		public static T Duplicate<T>(this T obj) => ToObject<T>(ToJSON(obj));
		/// <summary>
		/// Tries to convert a <paramref name="JSON"/> <see cref="string"/> into <typeparamref name="T"/> 
		/// <paramref name="instance"/> and returns it if successful. Otherwise returns 
		/// <paramref name="default"/>(<typeparamref name="T"/>).
		/// </summary>
		public static T ToObject<T>(this string JSON)
		{
			try
			{
				var settings = new JsonSerializerSettings
				{
					TypeNameHandling = TypeNameHandling.All,
					SerializationBinder = JsonBinder.Instance,
				};
				return JsonConvert.DeserializeObject<T>(JSON, settings);
			}
			catch (Exception) { return default; }
		}
		/// <summary>
		/// Tries to convert <paramref name="instance"/> into a <paramref name="JSON"/> <see cref="string"/> 
		/// and returns it if successful. Returns <paramref name="null"/> otherwise.‪‪<br></br><br></br> When <paramref name="instance"/> inherits <see cref="Thing"/>:<br></br>
		/// - Fields and properties (both <paramref name="public"/> and <paramref name="private"/>) require the attribute
		/// [<see cref="JsonProperty"/>] in order for them to be included in the <paramref name="JSON"/> <see cref="string"/>.<br></br>
		/// When <paramref name="instance"/> is anything else:<br></br>
		/// - All <paramref name="public"/> members are included.<br></br><br></br>
		/// Only the following <see cref="Type"/>s are allowed for security reasons
		/// (known <paramref name="JSON"/> deserialization code injection vulnerability):<br></br>
		/// - <see cref="bool"/>, <see cref="char"/>, <see cref="string"/>, <see cref="sbyte"/>, <see cref="byte"/>, <see cref="short"/>,
		/// <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>, <see cref="float"/>,
		/// <see cref="double"/>, <see cref="decimal"/> (can be in a <see cref="List{T}"/> or <see cref="Array"/>)<br></br>
		/// - <see cref="Point"/>, <see cref="Color"/>, <see cref="Size"/>, <see cref="Line"/>, <see cref="Corner"/>, <see cref="Quad"/>,
		/// <see cref="Assets.DataSlot"/> (can be in a <see cref="List{T}"/> or <see cref="Array"/>)<br></br>
		/// - <see cref="Dictionary{TKey, TValue}"/> (<typeparamref name="TKey"/> = <see cref="bool"/>, <see cref="string"/>,
		/// <see cref="int"/>, <see cref="double"/>, <typeparamref name="TValue"/> = <see cref="bool"/>, <see cref="string"/>,
		/// <see cref="int"/>, <see cref="double"/>), <paramref name="JSON"/> <see cref="string"/>s can be used if more
		/// <see cref="Type"/>s are needed. <br></br>
		/// - As well as all of the <paramref name="classes"/> in <see cref="global::SMPL"/> and the <see cref="Assembly"/> calling this
		/// function (can be in a <see cref="List{T}"/> or <see cref="Array"/>).<br></br><br></br>
		/// <see cref="FromJSON{T}(string)"/> will return <paramref name="null"/> if the <paramref name="instance"/>
		/// or any of its members is a <see cref="Type"/> that is not listed above.
		/// </summary>
		public static string ToJSON(this object instance)
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All,
				SerializationBinder = JsonBinder.Instance,
			};
			return JsonConvert.SerializeObject(instance, settings);
		}

		internal static Dictionary<string, int> gateEntries = new();
		internal static Dictionary<string, bool> gates = new();
		public static bool Once(this bool condition, uint maxEntries = uint.MaxValue, string uniqueness = default)
		{
			var uniqueID = $"{Debug.CurrentFilePath(1)}-{Debug.CurrentLineNumber(1)}-{uniqueness}";
			if (gates.ContainsKey(uniqueID) == false && condition == false) return false;
			else if (gates.ContainsKey(uniqueID) == false && condition == true)
			{
				gates[uniqueID] = true;
				gateEntries[uniqueID] = 1;
				return true;
			}
			else
			{
				if (gates[uniqueID] == true && condition == true) return false;
				else if (gates[uniqueID] == false && condition == true)
				{
					gates[uniqueID] = true;
					gateEntries[uniqueID]++;
					return true;
				}
				else if (gateEntries[uniqueID] < maxEntries) gates[uniqueID] = false;
			}
			return false;
		}

		public static bool IsNumber(this string text)
		{
			return float.TryParse(text, out _);
		}
		public static bool IsLetters(this string text)
		{
			for (int i = 0; i < text.Length; i++)
			{
				var isLetter = (text[i] >= 'A' && text[i] <= 'Z') || (text[i] >= 'a' && text[i] <= 'z');
				if (isLetter == false) return false;
			}
			return true;
		}
		public static string Align(this string text, int spaces)
		{
			return string.Format("{0," + spaces + "}", text);
		}
		public static string Repeat(this string text, uint times)
		{
			var result = "";
			for (int i = 0; i < times; i++)
				result = $"{result}{text}";
			return result;
		}

		public static string Compress(this string text)
		{
			var buffer = Encoding.UTF8.GetBytes(text);
			var memoryStream = new MemoryStream();
			using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
				gZipStream.Write(buffer, 0, buffer.Length);

			memoryStream.Position = 0;

			var compressedData = new byte[memoryStream.Length];
			memoryStream.Read(compressedData, 0, compressedData.Length);

			var gZipBuffer = new byte[compressedData.Length + 4];
			Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
			return Convert.ToBase64String(gZipBuffer);
		}
		public static string Decompress(this string compressedText)
		{
			var gZipBuffer = Convert.FromBase64String(compressedText);
			using var memoryStream = new MemoryStream();
			var dataLength = BitConverter.ToInt32(gZipBuffer, 0);
			memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

			var buffer = new byte[dataLength];

			memoryStream.Position = 0;
			using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
				gZipStream.Read(buffer, 0, buffer.Length);

			return Encoding.UTF8.GetString(buffer);
		}

		public static bool IsSigned(this float number)
		{
			return number.ToString()[0] == '-';
		}
		public static float ToAngle(this float number)
		{
			return ((number % 360) + 360) % 360;
		}
		public static float Animate(this float progressPercent, ChoiceAnimation animationType, ChoiceAnimationCurve animationCurve)
		{
			var result = 0f;
			progressPercent /= 100;
			var x = progressPercent;
			switch (animationType)
			{
				case ChoiceAnimation.BendWeak:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? 1 - MathF.Cos(x * MathF.PI / 2) :
							animationCurve == ChoiceAnimationCurve.Out ? 1 - MathF.Sin(x * MathF.PI / 2) :
							-(MathF.Cos(MathF.PI * x) - 1) / 2;
						break;
					}
				case ChoiceAnimation.Bend:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? x * x * x :
							animationCurve == ChoiceAnimationCurve.Out ? 1 - MathF.Pow(1 - x, 3) :
							(x < 0.5 ? 4 * x * x * x : 1 - MathF.Pow(-2 * x + 2, 3) / 2);
						break;
					}
				case ChoiceAnimation.BendStrong:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? x * x * x * x :
							animationCurve == ChoiceAnimationCurve.Out ? 1 - MathF.Pow(1 - x, 5) :
							(x < 0.5 ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2);
						break;
					}
				case ChoiceAnimation.Circle:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? 1 - MathF.Sqrt(1 - MathF.Pow(x, 2)) :
							animationCurve == ChoiceAnimationCurve.Out ? MathF.Sqrt(1 - MathF.Pow(x - 1, 2)) :
							(x < 0.5 ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2 : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2);
						break;
					}
				case ChoiceAnimation.Elastic:
					{
						result = animationCurve == ChoiceAnimationCurve.In ?
							(x == 0 ? 0 : x == 1 ? 1 : -MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * ((2 * MathF.PI) / 3))) :
							animationCurve == ChoiceAnimationCurve.Out ?
							(x == 0 ? 0 : x == 1 ? 1 : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * (2 * MathF.PI) / 3) + 1) :
							(x == 0 ? 0 : x == 1 ? 1 : x < 0.5f ? -(MathF.Pow(2, 20 * x - 10) * MathF.Sin((20f * x - 11.125f) *
							(2 * MathF.PI) / 4.5f)) / 2 :
							(MathF.Pow(2, -20 * x + 10) * MathF.Sin((20 * x - 11.125f) * (2 * MathF.PI) / 4.5f)) / 2 + 1);
						break;
					}
				case ChoiceAnimation.Swing:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? 2.70158f * x * x * x - 1.70158f * x * x :
							animationCurve == ChoiceAnimationCurve.Out ? 1 + 2.70158f * MathF.Pow(x - 1, 3) + 1.70158f * MathF.Pow(x - 1, 2) :
							(x < 0.5 ? (MathF.Pow(2 * x, 2) * ((2.59491f + 1) * 2 * x - 2.59491f)) / 2 :
							(MathF.Pow(2 * x - 2, 2) * ((2.59491f + 1) * (x * 2 - 2) + 2.59491f) + 2) / 2);
						break;
					}
				case ChoiceAnimation.Bounce:
					{
						result = animationCurve == ChoiceAnimationCurve.In ? 1 - easeOutBounce(1 - x) :
							animationCurve == ChoiceAnimationCurve.Out ? easeOutBounce(x) :
							(x < 0.5f ? (1 - easeOutBounce(1 - 2 * x)) / 2 : (1 + easeOutBounce(2 * x - 1)) / 2);
						break;
					}
			}
			return result * 100;

			float easeOutBounce(float x)
			{
				return x < 1 / 2.75f ? 7.5625f * x * x : x < 2 / 2.75f ? 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f :
					x < 2.5f / 2.75f ? 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f : 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
			}
		}
		public static float Limit(this float number, float lower, float upper, ChoiceLimit limitType = ChoiceLimit.ClosestBound)
		{
			if (limitType == ChoiceLimit.ClosestBound)
			{
				if (number < lower) return lower;
				else if (number > upper) return upper;
				return number;
			}
			else
			{
				upper += 1;
				var a = number;
				a = Map(a);
				while (a < lower) a = Map(a);
				return a;
				float Map(float b)
				{
					b = ((b % upper) + upper) % upper;
					if (b < lower) b = upper - (lower - b);
					return b;
				}
			}
		}
		public static float Sign(this float number, bool signed)
		{
			return signed ? -MathF.Abs(number) : MathF.Abs(number);
		}
		public static float Precision(this float number)
		{
			var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			var split = number.ToString().Split(cultDecPoint);
			return split.Length > 1 ? split[1].Length : 0;
		}
		public static float Round(this float number, float precision = 0,
			ChoiceRoundWay toward = ChoiceRoundWay.Closest,
			ChoiceRoundWhen5 priority = ChoiceRoundWhen5.AwayFromZero)
		{
			var midpoint = (MidpointRounding)priority;
			precision = (int)Limit(precision, 0, 5, ChoiceLimit.ClosestBound);

			if (toward == ChoiceRoundWay.Down || toward == ChoiceRoundWay.Up)
			{
				var numStr = number.ToString();
				var prec = Precision(number);
				if (prec > 0 && prec > precision)
				{
					var digit = toward == ChoiceRoundWay.Down ? "1" : "9";
					numStr = numStr.Remove(numStr.Length - 1);
					numStr = $"{numStr}{digit}";
					number = float.Parse(numStr);
				}
			}

			return MathF.Round(number, (int)precision, midpoint);
		}
		public static float ToNumber(this string text)
		{
			var result = 0.0f;
			text = text.Replace(',', '.');
			var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

			return parsed ? result : float.NaN;
		}
		public static float ToDataSize(this float number, ChoiceSizeToSize dataSize)
		{
			return dataSize switch
			{
				ChoiceSizeToSize.Bit_Byte => number / 8,
				ChoiceSizeToSize.Bit_KB => number / 8000,
				ChoiceSizeToSize.Byte_Bit => number * 8,
				ChoiceSizeToSize.Byte_KB => number / 1024,
				ChoiceSizeToSize.Byte_MB => number / 1_048_576,
				ChoiceSizeToSize.KB_Bit => number * 8000,
				ChoiceSizeToSize.KB_Byte => number * 1024,
				ChoiceSizeToSize.KB_MB => number / 1024,
				ChoiceSizeToSize.KB_GB => number / 1_048_576,
				ChoiceSizeToSize.MB_Byte => number * 1_048_576,
				ChoiceSizeToSize.MB_KB => number * 1024,
				ChoiceSizeToSize.MB_GB => number / 1024,
				ChoiceSizeToSize.MB_TB => number / 1_048_576,
				ChoiceSizeToSize.GB_KB => number * 1_048_576,
				ChoiceSizeToSize.GB_MB => number * 1024,
				ChoiceSizeToSize.GB_TB => number / 1024,
				ChoiceSizeToSize.TB_MB => number * 1_048_576,
				ChoiceSizeToSize.TB_GB => number * 1024,
				_ => default,
			};
		}
		public static bool IsBetween(this float number, float lower, float upper, bool inclusiveLower = false,
			bool inclusiveUpper = false)
		{
			var l = inclusiveLower ? lower <= number : lower < number;
			var u = inclusiveUpper ? upper >= number : upper > number;
			return l && u;
		}
		public static float Move(this float number, float speed, Time.ChoiceUnit timeUnit = Time.ChoiceUnit.Second)
		{
			if (timeUnit == Time.ChoiceUnit.Second) speed *= Time.Delta;
			return number + speed;
		}
		public static float MoveToward(this float number, float targetNumber, float speed, Time.ChoiceUnit timeUnit = Time.ChoiceUnit.Second)
		{
			var goingPos = number < targetNumber;
			var result = Move(number, goingPos ? Sign(speed, false) : Sign(speed, true), timeUnit);

			if (goingPos && result > targetNumber) return targetNumber;
			else if (goingPos == false && result < targetNumber) return targetNumber;
			return result;
		}
		public static float Map(this float number, float lowerA, float upperA, float lowerB, float upperB)
		{
			return (number - lowerA) / (upperA - lowerA) * (upperB - lowerB) + lowerB;
		}
		public static float MoveTowardAngle(this float angle, float targetAngle, float speed, Time.ChoiceUnit timeUnit = Time.ChoiceUnit.Second)
		{
			angle = ToAngle(angle);
			targetAngle = ToAngle(targetAngle);
			speed = Math.Abs(speed);
			var difference = angle - targetAngle;

			// stops the rotation with an else when close enough
			// prevents the rotation from staying behind after the stop
			var checkedSpeed = speed;
			if (timeUnit == Time.ChoiceUnit.Second) checkedSpeed *= Time.Delta;
			if (Math.Abs(difference) < checkedSpeed) angle = targetAngle;
			else if (difference >= 0 && difference < 180) angle = Move(angle, -speed, timeUnit);
			else if (difference >= -180 && difference < 0) angle = Move(angle, speed, timeUnit);
			else if (difference >= -360 && difference < -180) angle = Move(angle, -speed, timeUnit);
			else if (difference >= 180 && difference < 360) angle = Move(angle, speed, timeUnit);

			// detects speed greater than possible
			// prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
			if (Math.Abs(difference) > 360 - checkedSpeed) angle = targetAngle;

			return angle;
		}
		public static float Random(this float lower, float upper, float precision = 0, float seed = float.NaN)
		{
			precision = (int)precision.Limit(0, 5);
			precision = MathF.Pow(10, precision);

			lower *= precision;
			upper *= precision;

			var s = new Random(float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed);
			var randInt = s.Next((int)lower, (int)upper + 1).Limit((int)lower, (int)upper);

			return randInt / (precision);
		}
		public static bool HasChance(this float percent)
		{
			percent = percent.Limit(0, 100, ChoiceLimit.ClosestBound);
			var n = Random(1f, 100f); // should not roll 0 so it doesn't return true with 0% (outside of roll)
			return n <= percent;
		}
		public static float ToRadians(this float degrees)
		{
			return (PI / 180) * degrees;
		}
		public static float ToDegrees(this float radians)
		{
			return radians * (180 / PI);
		}

		public static bool IsSigned(this int number)
		{
			return number.ToString()[0] == '-';
		}
		public static int Random(this int lowerBound, int upperBound, float seed = float.NaN)
		{
			return (int)Random((float)lowerBound, (float)upperBound, 0, seed);
		}
		public static bool HasChance(this int percent)
		{
			return HasChance((float)percent);
		}
		public static int Limit(this int number, int lower, int upper, ChoiceLimit limitation = ChoiceLimit.ClosestBound)
		{
			return (int)Limit((float)number, lower, upper, limitation);
		}
		public static int Sign(this int number, bool signed) => (int)Sign((float)number, signed);
		public static bool IsBetween(this int number, int lower, int upper, bool inclusiveLower = false,
			bool inclusiveUpper = false) => IsBetween((float)number, lower, upper, inclusiveLower, inclusiveUpper);
		public static int Map(this int number, int lowerA, int upperA, int lowerB, int upperB) =>
			(int)Map((float)number, lowerA, upperA, lowerB, upperB);

		public static bool IsNaN(this Vector3 vec)
		{
			return float.IsNaN(vec.X) || float.IsNaN(vec.Y) || float.IsNaN(vec.Z);
		}
		public static Vector3 NaN(this Vector3 vec)
		{
			return new Vector3(float.NaN, float.NaN, float.NaN);
		}
		public static float IndexToAxis(this Vector3 vec, int i)
		{
			i = i.Limit(0, 2);
			if (i == 0)
				return vec.X;
			else if (i == 1)
				return vec.Y;
			else if (i == 2)
				return vec.Z;

			return float.NaN;
		}

		public static void TryAddStringNewLine(this ListBox listBox, object item, bool scrollToAdded)
		{
			var str = item.ToString();
			var newLine = "";
			if (str.Length >= listBox.Width - 1)
			{
				var noWordNewLine = true;
				for (int i = listBox.Width - 1; i >= 0; i--)
					if (str[i] == ' ')
					{
						newLine = str[(i + 1)..];
						str = str.Substring(0, i);
						noWordNewLine = false;
						break;
					}
				if (noWordNewLine)
				{
					newLine = str[(listBox.Width - 2)..];
					str = str.Substring(0, listBox.Width - 2);
				}
			}

			listBox.Items.Add(str);

			if (scrollToAdded)
			{
				var prev = listBox.SelectedIndex;
				listBox.SelectedIndex = listBox.Items.Count - 1;
				listBox.Update(TimeSpan.Zero);
				listBox.ScrollToSelectedItem();
				listBox.SelectedIndex = prev;
			}

			if (newLine != "")
				TryAddStringNewLine(listBox, newLine, scrollToAdded);
		}
		public static void KeepInConsole(this Window window, SadConsole.Console console)
		{
			if (console.Width - window.Width < 0)
			{
				window.Position = new(0, window.Position.Y);
				return;
			}
			if (console.Height - window.Height < 0)
			{
				window.Position = new(window.Position.X, 0);
				return;
			}

			if (window.Position.X < 0)
				window.Position = new(0, window.Position.Y);
			else if (window.Position.X + window.Width > console.Width)
				window.Position = new(console.Width - window.Width, window.Position.Y);
			else if (window.Position.Y < 0)
				window.Position = new(window.Position.X, 0);
			else if (window.Position.Y + window.Height > console.Height)
				window.Position = new(window.Position.X, console.Height - window.Height);
		}
	}
}
