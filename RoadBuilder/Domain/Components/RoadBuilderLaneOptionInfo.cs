using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain.Components
{
	[Serializable]
	public class RoadBuilderLaneOptionInfo
	{
        public string Name;
        public bool IsValue;    
        public bool MultiSelection;
        public string DefaultValue;
        public RoadBuilderLaneOptionItemInfo[] Options;
    }

	[Serializable]
    public class RoadBuilderLaneOptionItemInfo
	{
		public string Value;
        public string ThumbnailUrl;
    }
}
