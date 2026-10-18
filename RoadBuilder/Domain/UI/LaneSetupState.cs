namespace RoadBuilder.Domain.UI
{
	// State of the asset editor's Road Builder lane setup panel, for the prefab selected in the inspector
	public class LaneSetupState
	{
		// Available: a net section or lane group is selected. InEditor: the asset editor is open, with or without a selection
		public bool Available;
		public bool InEditor;
		public string? Name;
		public string? DisplayName;
		public string? ListIcon;
		public bool IsGroup;
		public bool ReadOnly;
		public string? ReadOnlyReason;
		public string Role = LaneSetupRoles.None;
		public LaneSetupLaneInfo? LaneInfo;
		public LaneSetupEdge? Edge;
		public LaneSetupDecoration? Decoration;
		public LaneSetupAggregate? Aggregate;
		public LaneSetupVariant? Variant;
		public LaneSetupGroup? Group;
		public bool Ignore;
		public RoadLaneUIBinder[]? Preview;
		public LaneSetupIssue[] Issues = new LaneSetupIssue[0];
	}

	public static class LaneSetupRoles
	{
		public const string None = "none";
		public const string Lane = "lane";
		public const string Edge = "edge";
		public const string Variant = "variant";
		public const string Group = "group";
	}

	public class LaneSetupLaneInfo
	{
		public int RequireAll;
		public int RequireAny;
		public int RequireNone;
		public int GroundTexture;
		public string? Color;
		public bool NoDirection;
		public bool Median;
		public string? FrontThumbnail;
		public string? BackThumbnail;
		public string[] LaneThumbnails = new string[0];
		public int[] PieceRequireAll = new int[0];
		public int[] PieceRequireAny = new int[0];
		public int[] PieceRequireNone = new int[0];
	}

	public class LaneSetupEdge
	{
		public string? LeftSide;
		public string? RightSide;
		public bool AddSidewalkStateOnNode;
		public bool DoNotRequireBeingOnEdge;
	}

	public class LaneSetupDecoration
	{
		public string? GrassThumbnail;
		public string? TreeThumbnail;
		public string? GrassAndTreeThumbnail;
		public string[] LaneGrassThumbnail = new string[0];
		public string[] LaneTreeThumbnail = new string[0];
		public string[] LaneGrassAndTreeThumbnail = new string[0];
	}

	public class LaneSetupAggregate
	{
		public LaneSetupAggregateSection[] Left = new LaneSetupAggregateSection[0];
		public LaneSetupAggregateSection[] Right = new LaneSetupAggregateSection[0];
	}

	public class LaneSetupAggregateSection
	{
		public string? Section;
		public bool Invert;
		public int[] PieceRequireAll = new int[0];
		public int[] PieceRequireAny = new int[0];
		public int[] PieceRequireNone = new int[0];
	}

	public class LaneSetupVariant
	{
		public string? Group;
		public LaneSetupCombination[] Combination = new LaneSetupCombination[0];
	}

	public class LaneSetupCombination
	{
		public string? Option;
		public string? Value;
	}

	public class LaneSetupGroup
	{
		public LaneSetupOption[] Options = new LaneSetupOption[0];
		public LaneSetupLinkedSection[] LinkedSections = new LaneSetupLinkedSection[0];
	}

	public class LaneSetupOption
	{
		public string? Name;
		public int Type;
		public string? DefaultValue;
		public bool IgnoreForSimilarDuplicate;
		public LaneSetupOptionValue[] Values = new LaneSetupOptionValue[0];
	}

	public class LaneSetupOptionValue
	{
		public string? Value;
		public string? Thumbnail;
	}

	public class LaneSetupLinkedSection
	{
		public string? Name;
		public string? DisplayName;
		public LaneSetupCombination[] Combination = new LaneSetupCombination[0];
	}

	// A lane group as offered when making a section a variant of it
	public class LaneSetupGroupSummary
	{
		public string? Name;
		public string? DisplayName;
		public bool BuiltIn;
		public LaneSetupOption[] Options = new LaneSetupOption[0];
	}

	public class LaneSetupEnumValue
	{
		public string? Name;
		public int Value;
	}

	public class LaneSetupIssue
	{
		public string Severity = LaneSetupSeverity.Info;
		public string? Key;
		public string? Message;
	}

	public static class LaneSetupSeverity
	{
		public const string Error = "error";
		public const string Warning = "warning";
		public const string Info = "info";
	}
}
