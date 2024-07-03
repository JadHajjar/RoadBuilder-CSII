using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
	}
}
