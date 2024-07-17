using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
    [Serializable]
    public class RoadBuilderLaneOption
    {
        public string Name;
        public bool IsValue;
        public bool IsDecoration;
        public bool MultiSelection;
        public string DefaultValue;
        public RoadBuilderLaneOptionValue[] Options;
    }
}
