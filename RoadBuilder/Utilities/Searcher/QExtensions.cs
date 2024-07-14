using Colossal.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Unity.Entities;
using Unity.Mathematics;

namespace RoadBuilder.Utilities.Searcher
{
	internal static class QExtensions
	{
		public static float3 Center(this Bounds3 bounds)
		{
			var x = bounds.x.min + ((bounds.x.max - bounds.x.min) / 2);
			var y = bounds.y.min + ((bounds.y.max - bounds.y.min) / 2);
			var z = bounds.z.min + ((bounds.z.max - bounds.z.min) / 2);
			return new float3(x, y, z);
		}

		public static float3 Center(this Quad3 quad)
		{
			return (quad.a + quad.b + quad.c + quad.d) / 4;
		}

		public static float2 Center(this Bounds2 bounds)
		{
			var x = bounds.x.min + ((bounds.x.max - bounds.x.min) / 2);
			var y = bounds.y.min + ((bounds.y.max - bounds.y.min) / 2);
			return new float2(x, y);
		}

		public static float Center(this Bounds1 bounds)
		{
			return bounds.min + ((bounds.max - bounds.min) / 2);
			;
		}

		public static float2 Center2D(this Bounds3 bounds)
		{
			var x = bounds.x.min + ((bounds.x.max - bounds.x.min) / 2);
			var z = bounds.z.min + ((bounds.z.max - bounds.z.min) / 2);
			return new float2(x, z);
		}

		public static Circle2 XZ(this Circle3 circle)
		{
			return new Circle2(circle.radius, new(circle.position.x, circle.position.z));
		}

		public static float3 Position(this Bezier4x3 bezier)
		{
			var total = bezier.b + bezier.c;
			return total / 2;
			//float3 total = bezier.a + bezier.b + bezier.c + bezier.d;
			//return total / 4;
		}

		public static float3 Get(this Bezier4x3 curve, short idx)
		{
			return idx switch
			{
				0 => curve.a,
				1 => curve.b,
				2 => curve.c,
				3 => curve.d,
				_ => throw new Exception($"Invalid Bezier curve key get '{idx}'")
			};
		}

		public static void Set(ref this Bezier4x3 curve, short idx, float3 val)
		{
			switch (idx)
			{
				case 0:
					curve.a = val;
					break;
				case 1:
					curve.b = val;
					break;
				case 2:
					curve.c = val;
					break;
				case 3:
					curve.d = val;
					break;
				case 4:
					throw new Exception($"Invalid Bezier curve key set '{idx}'");
			}
		}

		public static bool IsNodeA(this short idx)
		{
			if (idx < 2)
			{
				return true;
			}

			return false;
		}

		public static bool IsNodeB(this short idx)
		{
			if (idx is > 1 and <= 3)
			{
				return true;
			}

			return false;
		}

		public static bool IsEnd(this short idx)
		{
			if (idx is 0 or 3)
			{
				return true;
			}

			return false;
		}

		public static bool IsMiddle(this short idx)
		{
			if (idx is 1 or 2)
			{
				return true;
			}

			return false;
		}

		public static string D(this UnityEngine.Color c)
		{
			return $"R{c.r:.00}G{c.g:.00}B{c.b:.00}A{c.a:.00}";
		}

		public static string D(this Entity e)
		{
			return $"E{e.Index}.{e.Version}";
		}

		public static string D(this Game.Objects.Transform t)
		{
			return $"{t.m_Position.DX()}/{t.m_Rotation.Y():0.##}";
		}

		public static string D(this int2 i)
		{
			return $"{i.x},{i.y}";
		}

		public static string D(this float2 f)
		{
			return $"{f.x:0.##},{f.y:0.##}";
		}

		public static string D(this float3 f)
		{
			return $"{f.x:0.0},{f.z:0.0}";
		}

		public static string DX(this float3 f)
		{
			return $"{f.x:0.00},{f.y:0.00},{f.z:0.00}";
		}

		public static string D(this Quad2 q)
		{
			return $"({q.a.x:0.##},{q.a.y:0.##}),({q.b.x:0.##},{q.b.y:0.##}),({q.c.x:0.##},{q.c.y:0.##}),({q.d.x:0.##},{q.d.y:0.##})";
		}

		public static float2 XZ(this float3 f)
		{
			return new float2(f.x, f.z);
		}

		public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
		{
			key = tuple.Key;
			value = tuple.Value;
		}

		public static float DistanceXZ(this float3 a, float3 b)
		{
			return math.abs(math.distance(new float2(a.x, a.z), new float2(b.x, b.z)));
		}

		public static Bounds3 Encapsulate(this Bounds3 a, Bounds3 b)
		{
			Bounds3 c = default;
			c.min.x = Math.Min(a.min.x, b.min.x);
			c.min.y = Math.Min(a.min.y, b.min.y);
			c.min.z = Math.Min(a.min.z, b.min.z);
			c.max.x = Math.Max(a.max.x, b.max.x);
			c.max.y = Math.Max(a.max.y, b.max.y);
			c.max.z = Math.Max(a.max.z, b.max.z);
			return c;
		}

		public static Bounds2 Encapsulate(this Bounds2 a, Bounds2 b)
		{
			Bounds2 c = default;
			c.min.x = Math.Min(a.min.x, b.min.x);
			c.min.y = Math.Min(a.min.y, b.min.y);
			c.max.x = Math.Max(a.max.x, b.max.x);
			c.max.y = Math.Max(a.max.y, b.max.y);
			return c;
		}

		public static Bounds3 Expand(this Bounds3 b, float3 size)
		{
			return new Bounds3(
				b.min - size,
				b.max + size
			);
		}

		public static quaternion Inverse(this quaternion q)
		{
			var num = (q.value.x * q.value.x) + (q.value.y * q.value.y) + (q.value.z * q.value.z) + (q.value.w * q.value.w);
			var num2 = 1f / num;
			quaternion result = default;
			result.value.x = (0f - q.value.x) * num2;
			result.value.y = (0f - q.value.y) * num2;
			result.value.z = (0f - q.value.z) * num2;
			result.value.w = q.value.w * num2;
			return result;
		}

		public static Quad3 ToQuad3(this Bounds3 b)
		{
			return new(
				new(b.min.x, b.min.y, b.min.z),
				new(b.max.x, b.min.y, b.min.z),
				new(b.max.x, b.max.y, b.max.z),
				new(b.min.x, b.max.y, b.max.z));
		}

		public static float3 Lerp(this float3 a, float3 b, float t)
		{
			return new float3(a.x + ((b.x - a.x) * t), a.y + ((b.y - a.y) * t), a.z + ((b.z - a.z) * t));
		}

		public static float3 LerpAbs(this float3 a, float3 b, float t)
		{
			var length = math.distance(a, b);
			t = (length - t) / length;
			return a.Lerp(b, t);
		}

		public static float3 Max(this Quad3 q)
		{
			return q.a.Max(q.b).Max(q.c.Max(q.d));
		}

		public static float3 Max(this float3 a, float3 b)
		{
			return new(math.max(a.x, b.x), math.max(a.y, b.y), math.max(a.z, b.z));
		}

		public static float3 Min(this Quad3 q)
		{
			return q.a.Min(q.b).Min(q.c.Min(q.d));
		}

		public static float3 Min(this float3 a, float3 b)
		{
			return new(math.min(a.x, b.x), math.min(a.y, b.y), math.min(a.z, b.z));
		}

		public static string RemoveWhitespace(this string input)
		{
			return new string(input.ToCharArray()
				.Where(c => !char.IsWhiteSpace(c))
				.ToArray());
		}

		public static float3 ToEulerDegrees(this quaternion quat)
		{
			var q1 = quat.value;

			var sqw = q1.w * q1.w;
			var sqx = q1.x * q1.x;
			var sqy = q1.y * q1.y;
			var sqz = q1.z * q1.z;
			var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			var test = (q1.x * q1.w) - (q1.y * q1.z);
			float3 v;

			if (test > 0.4995f * unit)
			{ // north pole
				v.y = 2f * math.atan2(q1.y, q1.x);
				v.x = math.PI / 2;
				v.z = 0;
				return ClampDegreesAll(math.degrees(v));
			}

			if (test < -0.4995f * unit)
			{ // south pole
				v.y = -2f * math.atan2(q1.y, q1.x);
				v.x = -math.PI / 2;
				v.z = 0;
				return ClampDegreesAll(math.degrees(v));
			}

			quaternion q3 = new(q1.w, q1.z, q1.x, q1.y);
			var q = q3.value;

			v.y = math.atan2((2f * q.x * q.w) + (2f * q.y * q.z), 1 - (2f * ((q.z * q.z) + (q.w * q.w))));
			v.x = math.asin(2f * ((q.x * q.z) - (q.w * q.y)));
			v.z = math.atan2((2f * q.x * q.y) + (2f * q.z * q.w), 1 - (2f * ((q.y * q.y) + (q.z * q.z))));

			return ClampDegreesAll(math.degrees(v));
		}

		private static float3 ClampDegreesAll(float3 angles)
		{
			angles.x = ClampDegrees(angles.x);
			angles.y = ClampDegrees(angles.y);
			angles.z = ClampDegrees(angles.z);
			return angles;
		}

		private static float ClampDegrees(float angle)
		{
			angle %= 360;
			if (angle < 0)
			{
				angle += 360;
			}

			return angle;
		}

		public static float3 ToFloat3(this float2 pos, float y)
		{
			return new float3(pos.x, y, pos.y);
		}

		public static string ToStringNoTrace(this Exception e)
		{
			StringBuilder sb = new(e.GetType().ToString());
			sb.Append(": ").Append(e.Message);
			return sb.ToString();
		}


		public static float X(this Game.Objects.Transform transform)
		{
			return transform.m_Rotation.ToEulerDegrees().x;
		}

		public static float Y(this Game.Objects.Transform transform)
		{
			return transform.m_Rotation.ToEulerDegrees().y;
		}

		public static float Z(this Game.Objects.Transform transform)
		{
			return transform.m_Rotation.ToEulerDegrees().z;
		}


		public static float X(this quaternion quat)
		{
			return quat.ToEulerDegrees().x;
		}

		public static float Y(this quaternion quat)
		{
			return quat.ToEulerDegrees().y;
		}

		public static float Z(this quaternion quat)
		{
			return quat.ToEulerDegrees().z;
		}
	}
}
