using Colossal.UI;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace RoadBuilder.Utilities
{
	public class ThumbnailGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;

		public INetworkBuilderPrefab NetworkPrefab { get; }

		public ThumbnailGenerationUtil(INetworkBuilderPrefab prefab, RoadGenerationData roadGenerationData)
		{
			_roadGenerationData = roadGenerationData;

			NetworkPrefab = prefab;
		}

		public string GenerateThumbnail()
		{
			var svgs = GetSvgItems();

			var totalWidth = svgs[0].ExtentsRect.Width - svgs[0].PositionRect.Width + svgs.Sum(x => x.PositionRect.Width);
			var totalHeight = svgs[svgs.Count - 1].ExtentsRect.Height - svgs[svgs.Count - 1].PositionRect.Height + svgs.Sum(x => x.PositionRect.Height);
			var totalSize = Math.Max(totalWidth, totalHeight) + 10;

			var elements = new List<XElement>();

			var currentX = (totalSize - totalWidth) / 4;
			var currentY = (totalSize - totalHeight) / 2 + totalHeight - 50;

			foreach (var item in svgs)
			{
				elements.Insert(0, item.SetBounds(currentX, currentY));

				currentX += item.PositionRect.Width;
				currentY -= item.PositionRect.Height;
			}

			XNamespace aw = "http://www.w3.org/2000/svg";
			var combinedSvg = new XElement(aw + "svg",
				new XAttribute("width", "100%"),
				new XAttribute("height", "100%"),
				new XAttribute("viewBox", $"0 0 {totalSize} {totalSize}"),
				new XAttribute("version", "1.1"),
				new XAttribute(XNamespace.Xmlns + "xlink", "http://www.w3.org/1999/xlink"),
				new XAttribute(XNamespace.Xmlns + "serif", "http://www.serif.com/"),
				new XAttribute(XNamespace.Xmlns + "space", "preserve"),
				new XAttribute("xmlns", "http://www.w3.org/2000/svg"),
				new XAttribute("style", "fill-rule:evenodd;clip-rule:evenodd;stroke-linejoin:round;stroke-miterlimit:2;"),
				elements
			);

			var combinedXml = new XDocument(
				new XDeclaration("1.0", "UTF-8", "no"),
				new XDocumentType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null),
				combinedSvg
			);

			combinedXml.Save(Path.Combine(FoldersUtil.TempFolder, NetworkPrefab.Config.ID + ".svg"));

			return $"coui://roadbuilderthumbnails/{NetworkPrefab.Config.ID}.svg";
		}

		private List<SvgItem> GetSvgItems()
		{
			var svgs = new List<SvgItem>();

			foreach (var lane in NetworkPrefab.Config.Lanes.SelectMany(GetLaneIcons))
			{
				var file = GetFileName(lane);

				if (file is null)
				{
					continue;
				}

				try
				{
					var xml = XDocument.Load(file);
					var svg = xml.Root;
					var positionRect = GetRectValues(xml, "Position");
					var extentsRect = GetRectValues(xml, "Extents");

					svgs.Add(new SvgItem(svg, positionRect, extentsRect));
				}
				catch (Exception ex)
				{
					Mod.Log.Warn(ex, "Failed to get SVG info from file: " + file);
				}
			}

			return svgs;
		}

		private IEnumerable<string> GetLaneIcons(LaneConfig lane)
		{
			if (!NetworkPrefabGenerationUtil.GetNetSection(_roadGenerationData, NetworkPrefab.Config, lane, out var section, out var groupPrefab))
			{
				yield break;
			}

			if (section.TryGet<RoadBuilderLaneDecorationInfo>(out var decorationInfo) && groupPrefab?.Options.FirstOrDefault(x => x.Type is LaneOptionType.Decoration) is RoadBuilderLaneOption decorationOption)
			{
				switch (LaneOptionsUtil.GetSelectedOptionValue(NetworkPrefab.Config, lane, decorationOption))
				{
					case "G":
						if (decorationInfo.LaneGrassThumbnail?.Any() ?? false)
						{
							foreach (var item in lane.Invert ? decorationInfo.LaneGrassThumbnail.Reverse() : decorationInfo.LaneGrassThumbnail)
							{
								yield return item;
							}

							yield break;
						}

						break;
					case "T":
						if (decorationInfo.LaneTreeThumbnail?.Any() ?? false)
						{
							foreach (var item in lane.Invert ? decorationInfo.LaneTreeThumbnail.Reverse() : decorationInfo.LaneTreeThumbnail)
							{
								yield return item;
							}

							yield break;
						}

						break;
					case "GT":
						if (decorationInfo.LaneGrassAndTreeThumbnail?.Any() ?? false)
						{
							foreach (var item in lane.Invert ? decorationInfo.LaneGrassAndTreeThumbnail.Reverse() : decorationInfo.LaneGrassAndTreeThumbnail)
							{
								yield return item;
							}

							yield break;
						}

						break;
				}
			}

			if (section.TryGet<RoadBuilderLaneInfo>(out var sectionInfo) && (sectionInfo.LaneThumbnails?.Any() ?? false))
			{
				foreach (var item in lane.Invert ? sectionInfo.LaneThumbnails.Reverse() : sectionInfo.LaneThumbnails)
				{
					yield return item;
				}

				yield break;
			}

			if (groupPrefab != null && groupPrefab.TryGet<RoadBuilderLaneInfo>(out var groupInfo) && (groupInfo.LaneThumbnails?.Any() ?? false))
			{
				foreach (var item in lane.Invert ? groupInfo.LaneThumbnails.Reverse() : groupInfo.LaneThumbnails)
				{
					yield return item;
				}

				yield break;
			}

			yield return "coui://roadbuildericons/Thumb_CarLane.svg";
		}

		private static string GetFileName(string icon)
		{
			if (string.IsNullOrEmpty(icon) || icon.StartsWith("thumbnail://"))
			{
				return null;
			}

			var regex = Regex.Match(icon, "coui://(.+?)/(.+)");

			if (!regex.Success)
			{
				return Path.Combine(FoldersUtil.GameUIPath, icon);
			}

			var hostMap = (IDictionary<string, HashSet<string>>)typeof(DefaultResourceHandler).GetField("m_HostLocationsMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(UIManager.defaultUISystem.resourceHandler);

			if (hostMap?.TryGetValue(regex.Groups[1].Value, out var paths) ?? false)
			{
				foreach (var path in paths)
				{
					if (File.Exists(Path.Combine(path, regex.Groups[2].Value)))
					{
						return Path.Combine(path, regex.Groups[2].Value);
					}
				}
			}

			return null;
		}

		private Rectangle GetRectValues(XDocument xmlDocument, string rectId)
		{
			var rectElement = xmlDocument
				.Descendants()
				.FirstOrDefault(e => e.Name.LocalName == "rect" && ((string)e.Attribute("id") == rectId || (string)e.Parent.Attribute("id") == rectId));

			if (rectElement == null)
			{
				throw new Exception($"Rect element with id '{rectId}' not found.");
			}

			double.TryParse(rectElement.Attribute("x").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var x);
			double.TryParse(rectElement.Attribute("y").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var y);
			double.TryParse(rectElement.Attribute("width").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var width);
			double.TryParse(rectElement.Attribute("height").Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var height);

			return new Rectangle(x, y, width, height);
		}

		public static void DeleteThumbnail(string id)
		{
			var path = Path.Combine(FoldersUtil.TempFolder, id + ".svg");

			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private class SvgItem
		{
			public SvgItem(XElement svg, Rectangle positionRect, Rectangle extentsRect)
			{
				Svg = svg;
				PositionRect = positionRect;
				ExtentsRect = extentsRect;
			}

			public XElement Svg { get; }
			public Rectangle PositionRect { get; }
			public Rectangle ExtentsRect { get; }

			public XElement SetBounds(double offsetX, double offsetY)
			{
				return new XElement("g"
					, new XAttribute("transform", $"matrix(1,0,0,1,{(offsetX - PositionRect.X).ToString(CultureInfo.InvariantCulture)},{(offsetY - PositionRect.Y - PositionRect.Height).ToString(CultureInfo.InvariantCulture)})")
					, Svg.Elements());
			}

			private static XElement RemoveAllNamespaces(XElement xmlDocument)
			{
				if (!xmlDocument.HasElements)
				{
					var xElement = new XElement(xmlDocument.Name.LocalName)
					{
						Value = xmlDocument.Value
					};

					foreach (var attribute in xmlDocument.Attributes())
					{
						xElement.Add(attribute);
					}

					return xElement;
				}

				return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
			}
		}

		private readonly struct Rectangle
		{
			public Rectangle(double x, double y, double width, double height)
			{
				X = x - 50;
				Y = y - 50;
				Width = width;
				Height = height;
			}

			public double X { get; }
			public double Y { get; }
			public double Width { get; }
			public double Height { get; }
			public bool Empty => X == 0 && Y == 0 && Width == 0 && Height == 0;

			public override readonly string ToString()
			{
				return $"({X}, {Y}, {Width}, {Height})";
			}
		}
	}
}
