using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RoadBuilder.Utilities
{
	public static class Extensions
	{
		public static string FormatWords(this string str, bool forceUpper = false)
		{
			str = Regex.Replace(Regex.Replace(str,
				@"([a-z])([A-Z])", x => $"{x.Groups[1].Value} {x.Groups[2].Value}"),
				@"(\b)(?<!')([a-z])", x => $"{x.Groups[1].Value}{x.Groups[2].Value.ToUpper()}");

			if (forceUpper)
			{
				str = Regex.Replace(str, @"(^[a-z])|(\ [a-z])", x => x.Value.ToUpper(), RegexOptions.IgnoreCase);
			}

			return str;
		}

		public static T Next<T>(this IEnumerable<T> enumerable, T item, bool circleBack = false)
		{
			if (!enumerable?.Any() ?? true)
			{
				return item;
			}

			var found = false;

			foreach (var it in enumerable)
			{
				if (found)
				{
					return it;
				}

				if (it.Equals(item))
				{
					found = true;
				}
			}

			return circleBack ? enumerable.FirstOrDefault() : default;
		}

		public static T Previous<T>(this IEnumerable<T> enumerable, T item, bool circleBack = false)
		{
			if (!enumerable?.Any() ?? true)
			{
				return item;
			}

			var found = false;

			foreach (var it in enumerable.Reverse())
			{
				if (found)
				{
					return it;
				}

				if (it.Equals(item))
				{
					found = true;
				}
			}

			return circleBack ? enumerable.LastOrDefault() : default;
		}
	}
}
