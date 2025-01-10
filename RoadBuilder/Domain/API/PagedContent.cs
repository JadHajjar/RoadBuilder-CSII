using System.Collections.Generic;

namespace RoadBuilder.Domain.API
{
	public class PagedContent<T>
	{
		public int page { get; set; }
		public int pageSize { get; set; }
		public int totalPages { get; set; }
		public List<T>? items { get; set; }
	}
}