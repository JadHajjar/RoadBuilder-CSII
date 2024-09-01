using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities.Online;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderRoadManagementUISystem : ExtendedUISystemBase
	{
		private readonly CancellationTokenSource _cancellationTokenSource = new();
		private string query;
		private RoadCategory? currentCategory;
		private int sorting;
		private ValueBindingHelper<bool> Loading;
		private ValueBindingHelper<int> CurrentPage;
		private ValueBindingHelper<int> MaxPages;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> Items;

		protected override void OnCreate()
		{
			base.OnCreate();

			Loading = CreateBinding("Discover.Loading", true);
			CurrentPage = CreateBinding("Discover.CurrentPage", 1);
			MaxPages = CreateBinding("Discover.MaxPages", 1);
			Items = CreateBinding("Discover.Items", new RoadConfigurationUIBinder[0]);

			CreateTrigger<int>("Discover.SetPage", SetDiscoverPage);
			CreateTrigger<int>("Discover.SetSorting", SetDiscoverSorting);
			CreateTrigger<string>("Discover.Download", DownloadConfig);
			CreateTrigger<string>("Management.SetSearchQuery", SetSearchQuery);
			CreateTrigger<int>("Management.SetCategory", SetCategory);
		}

		private void SetCategory(int obj)
		{
			currentCategory = obj < 0 ? null : (RoadCategory)obj;

			StartLoad();
		}

		private void SetDiscoverSorting(int obj)
		{
			sorting = obj;

			StartLoad();
		}

		private void SetSearchQuery(string obj)
		{
			query = obj;

			StartLoad();
		}

		private void SetDiscoverPage(int page)
		{
			CurrentPage.Value = page;

			StartLoad();
		}

		private void StartLoad()
		{
			_cancellationTokenSource.Cancel();

			Loading.Value = true;

			Task.Run(LoadPage);
		}

		private async Task LoadPage()
		{
			var token = _cancellationTokenSource.Token;

			await Task.Delay(200);

			if (token.IsCancellationRequested)
			{
				return;
			}

			var result = await ApiUtil.Instance.GetEntries(query, (int?)currentCategory, sorting, CurrentPage.Value);

			if (token.IsCancellationRequested)
			{
				return;
			}

			CurrentPage.Value = result.page;
			MaxPages.Value = result.totalPages;

			var items = new RoadConfigurationUIBinder[result.items.Count];

			for (var i = 0; i < items.Length; i++)
			{
				var item = result.items[i];
				items[i] = new RoadConfigurationUIBinder
				{
					ID = item.iD,
					Category = (RoadCategory)item.category,
					Name = item.name,
					Thumbnail = $"{KEYS.API_URL}/roadicon/{item.iD}.svg",
				};
			}

			Items.Value = items;
		}

		private void DownloadConfig(string obj)
		{
		}
	}
}
