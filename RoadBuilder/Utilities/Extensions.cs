using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Unity.Entities;

namespace RoadBuilder.Utilities
{
	public static class Extensions
	{
		public static bool TryAddComponent<T>(this EntityManager entityManager, Entity entity)
		{
			if (entityManager.HasComponent<T>(entity))
			{
				return false;
			}

			return entityManager.AddComponent(entity, ComponentType.ReadWrite<T>());
		}

		public static bool SearchCheck(this string searchTerm, string termToBeSearched, bool caseCheck = false)
		{
			if (string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(termToBeSearched))
			{
				return true;
			}

			if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(termToBeSearched))
			{
				return false;
			}

			if (termToBeSearched.IndexOf(searchTerm, caseCheck ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				return true;
			}

			if (SpellCheck(searchTerm, termToBeSearched.Substring(0, Math.Min(termToBeSearched.Length, searchTerm.Length + 1)), caseCheck) <= (int)Math.Ceiling((searchTerm.Length - 3) / 5M))
			{
				return true;
			}

			if (searchTerm.AbbreviationCheck(termToBeSearched))
			{
				return true;
			}

			if (searchTerm.Contains(' '))
			{
				var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (terms.All(x => termToBeSearched.IndexOf(x, caseCheck ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0))
				{
					return true;
				}
			}

			return false;
		}

		public static int SpellCheck(this string s1, string s2, bool caseCheck = true)
		{
			s1 = s1.RemoveDoubleSpaces();
			s2 = s2.RemoveDoubleSpaces();

			if (!caseCheck)
			{
				s1 = s1.ToLower();
				s2 = s2.ToLower();
			}

			// Levenshtein Algorithm
			var n = s1.Length;
			var m = s2.Length;
			var d = new int[n + 1, m + 1];

			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			for (var i = 0; i <= n; d[i, 0] = i++)
			{ }

			for (var j = 0; j <= m; d[0, j] = j++)
			{ }

			for (var i = 1; i <= n; i++)
			{
				for (var j = 1; j <= m; j++)
				{
					var cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						 d[i - 1, j - 1] + cost);
				}
			}

			return d[n, m];
		}

		public static bool AbbreviationCheck(this string string1, string string2)
		{
			if (string.IsNullOrWhiteSpace(string1) || string.IsNullOrWhiteSpace(string1))
			{
				return false;
			}

			string1 = string1.ToLower().Replace("'s ", " ");
			string2 = string2.ToLower().Replace("'s ", " ");

			var abbreviation2 = string2.GetAbbreviation();
			var abbreviation1 = string1.GetAbbreviation();

			return (abbreviation2.StartsWith(string1.Where(x => x != ' ')) && abbreviation2.Length > 2)
				|| (abbreviation1.StartsWith(string2.Where(x => x != ' ')) && abbreviation1.Length > 2);
		}

		public static string GetAbbreviation(this string S)
		{
			var SB = new StringBuilder();
			foreach (var item in S.GetWords(true))
			{
				SB.Append(item.All(char.IsDigit) ? item : item[0].ToString());
			}

			if (Regex.IsMatch(SB.ToString(), "^[A-z]+[0-9]+$"))
			{
				var match = Regex.Match(SB.ToString(), "^([A-z]+)([0-9]+)$");
				return $"{match.Groups[1]} {match.Groups[2]}";
			}

			return SB.ToString();
		}

		public static IEnumerable<string> GetWords(this string text, bool includeNumbers = false)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				yield break;
			}

			foreach (Match match in Regex.Matches(text, $@"\b{(includeNumbers ? "" : "(?![0-9])")}(\w+)(?:'\w+)?\b"))
			{
				yield return match.Groups[1].Value;
			}
		}

		public static string Where(this string text, Func<char, bool> Test)
		{
			var builder = new StringBuilder(text.Length);

			foreach (var c in text)
			{
				if (Test(c))
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		public static string RemoveDoubleSpaces(this string text)
		{
			return Regex.Replace(text, @" {2,}", " ")?.Trim() ?? string.Empty;
		}

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
