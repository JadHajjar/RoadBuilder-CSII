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

		public static float CalculateWidth(this NetSectionPrefab netSection)
		{
			return netSection.m_SubSections.Sum(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f)
				+ netSection.m_Pieces.Max(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Piece.m_Width : 0f);
		}
	}
}
