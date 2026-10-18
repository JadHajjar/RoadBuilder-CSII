using Colossal.Core;
using Colossal.Json;

using RoadBuilder.Domain.API;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;
using RoadBuilder.Utilities.Online;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderRoadManagementUISystem : ExtendedUISystemBase
	{
		private enum ActionType
		{
			ShowInToolbar,
			TogglePlayset,
			UploadRoad
		}

		private int sorting;
		private string query = string.Empty;
		private string discoverQuery = string.Empty;
		private RoadCategory? currentCategory;
		private INetworkBuilderPrefab? SelectedRoad;
		private CancellationTokenSource cancellationTokenSource = new();
		private readonly HashSet<string> activeDownloads = new();

#nullable disable
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderRoadTrackerSystem roadBuilderRoadTrackerSystem;
		private RoadBuilderConfigurationsUISystem roadBuilderConfigurationsUISystem;
		private RoadBuilderGenericFunctionsSystem genericFunctionsSystem;
		private ValueBindingHelper<bool> Loading;
		private ValueBindingHelper<bool> ErrorLoading;
		private ValueBindingHelper<bool> Uploading;
		private ValueBindingHelper<int> CurrentPage;
		private ValueBindingHelper<int> MaxPages;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> Items;
		private ValueBindingHelper<OptionSectionUIEntry[]> SelectedRoadOptions;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> RoadConfigurations;
		private ValueBindingHelper<RoadConfigurationUIBinder> ManagedRoad;
		private ValueBindingHelper<string[]> Downloading;
#nullable enable

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderRoadTrackerSystem = World.GetOrCreateSystemManaged<RoadBuilderRoadTrackerSystem>();
			roadBuilderConfigurationsUISystem = World.GetOrCreateSystemManaged<RoadBuilderConfigurationsUISystem>();
			genericFunctionsSystem = World.GetOrCreateSystemManaged<RoadBuilderGenericFunctionsSystem>();

			roadBuilderConfigurationsUISystem.ConfigurationsUpdated += RoadBuilderConfigurationsUISystem_ConfigurationsUpdated;

			CreateBinding("Management.RestrictPlayset", () => !Mod.Settings!.NoPlaysetIsolation);

			Loading = CreateBinding("Discover.Loading", true);
			ErrorLoading = CreateBinding("Discover.ErrorLoading", false);
			Uploading = CreateBinding("Discover.Uploading", false);
			CurrentPage = CreateBinding("Discover.CurrentPage", 1);
			MaxPages = CreateBinding("Discover.MaxPages", 1);
			Items = CreateBinding("Discover.Items", new RoadConfigurationUIBinder[0]);
			SelectedRoadOptions = CreateBinding("Management.GetRoadOptions", new OptionSectionUIEntry[0]);
			RoadConfigurations = CreateBinding("Management.GetRoadConfigurations", new RoadConfigurationUIBinder[0]);
			ManagedRoad = CreateBinding("Management.ManagedRoad", (RoadConfigurationUIBinder?)null);
			Downloading = CreateBinding("Discover.Downloading", new string[0]);

			CreateTrigger<int>("Discover.SetPage", SetDiscoverPage);
			CreateTrigger<int>("Discover.SetSorting", SetDiscoverSorting);
			CreateTrigger<string>("Discover.Download", DownloadConfig);
			CreateTrigger<string>("Discover.SetSearchQuery", SetDiscoverSearchQuery);
			CreateTrigger<string>("Management.SetSearchQuery", SedSearchQuery);
			CreateTrigger<int>("Management.SetCategory", SetCategory);
			CreateTrigger<int, int, int>("Management.RoadOptionClicked", RoadOptionClicked);
			CreateTrigger<string>("Management.SetRoad", UpdateSelectedRoadConfiguration);
			CreateTrigger<string>("Management.SetRoadName", SetRoadName);
		}

		private void RoadBuilderConfigurationsUISystem_ConfigurationsUpdated()
		{
			SedSearchQuery(query);

			// A deleted road stays selected otherwise, and could still be uploaded or edited
			if (SelectedRoad?.Deleted == true)
			{
				SelectedRoad = null;
				SelectedRoadOptions.Value = new OptionSectionUIEntry[0];
			}

			ManagedRoad.Value = roadBuilderConfigurationsUISystem.GetRoadBinder(SelectedRoad);
		}

		private void SedSearchQuery(string obj)
		{
			query = obj;

			RoadConfigurations.Value = roadBuilderConfigurationsUISystem.AvailableConfigurations
				.Where(x => Filter(x.Name))
				.ToArray();
		}

		private bool Filter(string? name)
		{
			return string.IsNullOrWhiteSpace(query) || query.SearchCheck(name);
		}

		private void SetRoadName(string obj)
		{
			if (SelectedRoad?.Config != null)
			{
				SelectedRoad.Config.Name = obj;

				roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false, true);

				roadBuilderConfigurationsUISystem.UpdateConfigurationList();
			}
		}

		private void RoadOptionClicked(int option, int id, int value)
		{
			if (SelectedRoad?.Config == null)
			{
				return;
			}

			switch ((ActionType)option)
			{
				case ActionType.ShowInToolbar:
					SelectedRoad.Config.ToolbarState = (ShowInToolbarState)id;
					break;

				case ActionType.TogglePlayset:
					if (SelectedRoad.Config.IsInPlayset())
					{
						RemoveFromPlayset();
					}
					else
					{
						AddToPlayset();
					}

					break;

				case ActionType.UploadRoad:
					StartUpload();
					return;
			}

			roadBuilderSystem.UpdateRoad(SelectedRoad.Config, Entity.Null, false, true);

			roadBuilderConfigurationsUISystem.UpdateConfigurationList();

			UpdateSelectedRoadOptions();
		}

		private void UpdateSelectedRoadConfiguration(string id)
		{
			if (roadBuilderSystem.Configurations.TryGetValue(id ?? string.Empty, out SelectedRoad))
			{
				UpdateSelectedRoadOptions();
			}
			else
			{
				ManagedRoad.Value = null;
				SelectedRoadOptions.Value = new OptionSectionUIEntry[0];
			}
		}

		private void UpdateSelectedRoadOptions()
		{
			if (SelectedRoad?.Config is null)
			{
				return;
			}

			var config = SelectedRoad.Config;
			var options = new List<OptionSectionUIEntry>
			{
				new()
				{
					Id = (int)ActionType.ShowInToolbar,
					Name = LocaleHelper.Translate("RoadBuilder.ShowInToolbar", "Toolbar View"),
					IsToggle = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Hide,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Hide}]",
							Icon = "coui://roadbuildericons/RB_Hide.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Hide
						},
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Inherit,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Inherit}]",
							Icon = "coui://roadbuildericons/RB_Any.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Inherit
						},
						new OptionItemUIEntry
						{
							Id = (int)ShowInToolbarState.Show,
							Name = $"RoadBuilder.ShowInToolbarState[{ShowInToolbarState.Show}]",
							Icon = "coui://roadbuildericons/RB_Show.svg",
							Selected = config.ToolbarState == ShowInToolbarState.Show
						},
					}
				}
			};

			if (!Mod.Settings!.NoPlaysetIsolation)
			{
				var isInPlayset = config.IsInPlayset();
				options.Add(new()
				{
					Id = (int)ActionType.TogglePlayset,
					Name = LocaleHelper.Translate("RoadBuilder.Playset", "Playset"),
					IsButton = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Name = isInPlayset ? "RoadBuilder.RemoveFromPlayset" : "RoadBuilder.AddToPlayset",
							Icon = isInPlayset ? "coui://roadbuildericons/RB_Remove.svg" : "coui://roadbuildericons/RB_Add.svg",
							Disabled = roadBuilderRoadTrackerSystem.UsedNetworkPrefabs.Contains(SelectedRoad)
						}
					}
				});
			}

			if (!config.Uploaded && !string.IsNullOrEmpty(PdxModsUtil.UserId))
			{
				options.Add(new()
				{
					Id = (int)ActionType.UploadRoad,
					Name = LocaleHelper.Translate("RoadBuilder.Sharing", "Sharing"),
					IsButton = true,
					Options = new[]
					{
						new OptionItemUIEntry
						{
							Name = "RoadBuilder.UploadRoad",
							Icon = "coui://roadbuildericons/RB_Upload.svg",
							Disabled = Uploading || config.Name?.StartsWith("Copy of")==true || config.Name?.StartsWith("Custom ")==true
						}
					}
				});
			}

			ManagedRoad.Value = roadBuilderConfigurationsUISystem.GetRoadBinder(SelectedRoad);
			SelectedRoadOptions.Value = options.ToArray();
		}

		private void AddToPlayset()
		{
			if (PdxModsUtil.CurrentPlayset is not null && SelectedRoad?.Config is not null)
			{
				SelectedRoad.Config.Playsets ??= new();

				SelectedRoad.Config.Playsets.Remove($"-{PdxModsUtil.CurrentPlayset}");

				// A network with no positive playsets is available everywhere already, adding one would restrict it
				var positivePlaysets = SelectedRoad.Config.GetPositivePlaysets();

				if (positivePlaysets.Count > 0 && !positivePlaysets.Contains(PdxModsUtil.CurrentPlayset))
				{
					SelectedRoad.Config.Playsets.Add(PdxModsUtil.CurrentPlayset);
				}
			}
		}

		private void RemoveFromPlayset()
		{
			if (PdxModsUtil.CurrentPlayset is not null && SelectedRoad?.Config is not null)
			{
				SelectedRoad.Config.Playsets ??= new();

				var positivePlaysets = SelectedRoad.Config.GetPositivePlaysets();

				// Removing the last positive playset would make the network available everywhere, exclude it instead
				if (positivePlaysets.Count > 1 && positivePlaysets.Contains(PdxModsUtil.CurrentPlayset))
				{
					SelectedRoad.Config.Playsets.Remove(PdxModsUtil.CurrentPlayset);
				}
				else if (!SelectedRoad.Config.Playsets.Contains($"-{PdxModsUtil.CurrentPlayset}"))
				{
					SelectedRoad.Config.Playsets.Add($"-{PdxModsUtil.CurrentPlayset}");
				}
			}
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

		private void SetDiscoverSearchQuery(string obj)
		{
			discoverQuery = obj;

			StartLoad();
		}

		private void SetDiscoverPage(int page)
		{
			CurrentPage.Value = page;

			StartLoad();
		}

		private void StartLoad()
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource = new();

			Loading.Value = true;
			ErrorLoading.Value = false;

			// Request parameters are captured here so the background task never reads changing state
			var token = cancellationTokenSource.Token;
			var query = discoverQuery;
			var category = currentCategory;
			var order = sorting;
			var page = CurrentPage.Value;

			Task.Run(() => LoadPage(query, category, order, page, token));
		}

		private async Task LoadPage(string query, RoadCategory? category, int order, int page, CancellationToken token)
		{
			try
			{
				// Debounces quick successive changes, a newer request cancels this one
				await Task.Delay(250, token);

				Mod.Log.Debug(nameof(LoadPage));

				var result = await ApiUtil.Instance.GetEntries(query, (int?)category, order, page, token) ?? throw new Exception("No response received");

				Mod.Log.Debug((result.items?.Count ?? 0) + " entries");

				foreach (var item in result.items ?? new())
				{
					if (!LocalSaveUtil.IsSafeFileName(item.id))
					{
						continue;
					}

					var path = Path.Combine(FoldersUtil.TempFolder, item.id + ".svg");

					try
					{
						if (!File.Exists(path))
						{
							File.WriteAllText(path, item.icon);
						}
					}
					catch (Exception ex)
					{
						Mod.Log.Warn(ex, "Failed to save thumbnail");
					}
				}

				// Bindings and road configurations are only touched on the main thread
				MainThreadDispatcher.RegisterUpdater(() =>
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					CurrentPage.Value = result.page;
					MaxPages.Value = result.totalPages;
					Items.Value = result.items?.Select(item => new RoadConfigurationUIBinder
					{
						ID = item.id,
						Category = (RoadCategory)item.category,
						Name = item.name,
						Author = item.author,
						Available = roadBuilderSystem.Configurations.ContainsKey(item.id ?? string.Empty),
						Thumbnail = LocalSaveUtil.IsSafeFileName(item.id) ? $"coui://roadbuilderthumbnails/{item.id}.svg" : null,
					}).ToArray() ?? new RoadConfigurationUIBinder[0];
					Loading.Value = false;
					ErrorLoading.Value = false;
				});
			}
			catch (OperationCanceledException) when (token.IsCancellationRequested)
			{ }
			catch (Exception ex)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				Mod.Log.Warn(ex, "Failed to load discover items");

				MainThreadDispatcher.RegisterUpdater(() =>
				{
					if (token.IsCancellationRequested)
					{
						return;
					}

					Loading.Value = false;
					ErrorLoading.Value = true;
				});
			}
		}

		private void DownloadConfig(string id)
		{
			if (string.IsNullOrEmpty(id) || !activeDownloads.Add(id))
			{
				return;
			}

			Downloading.Value = activeDownloads.ToArray();

			Task.Run(async () =>
			{
				INetworkConfig? config = null;

				try
				{
					config = await ApiUtil.Instance.GetConfig(id);
				}
				catch (Exception ex)
				{
					Mod.Log.Warn(ex, "Failed to download road: " + id);
				}

				// Prefab creation is only allowed on the main thread
				MainThreadDispatcher.RegisterUpdater(() => FinishDownload(id, config));
			});
		}

		private void FinishDownload(string id, INetworkConfig? config)
		{
			activeDownloads.Remove(id);

			try
			{
				if (config is null)
				{
					return;
				}

				if (!LocalSaveUtil.IsSafeFileName(config.ID))
				{
					Mod.Log.Warn("Downloaded road has an invalid ID: " + config.ID);

					return;
				}

				config.ApplyVersionChanges();

				// The uploader's local file and playsets don't apply to this installation
				config.OriginalID = config.ID;
				config.Playsets = PdxModsUtil.CurrentPlayset is null ? new() : new() { PdxModsUtil.CurrentPlayset };

				if (roadBuilderSystem.AddPrefab(config) is null)
				{
					return;
				}

				var items = Items.Value?.ToArray() ?? new RoadConfigurationUIBinder[0];

				foreach (var item in items)
				{
					if (item.ID == id)
					{
						item.Available = true;
					}
				}

				Items.Value = items;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to add downloaded road: " + id);
			}
			finally
			{
				Downloading.Value = activeDownloads.ToArray();
			}
		}

		private void StartUpload()
		{
			if (Uploading.Value || SelectedRoad?.Config is null)
			{
				return;
			}

			var config = SelectedRoad.Config;
			var id = config.ID;
			RoadBuilderEntryPost? request;

			try
			{
				request = CreateUploadRequest(SelectedRoad);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to prepare your road for upload");

				return;
			}

			if (request is null)
			{
				return;
			}

			Uploading.Value = true;
			UpdateSelectedRoadOptions();

			Task.Run(async () =>
			{
				ApiResponse? result = null;

				try
				{
					result = await ApiUtil.Instance.UploadRoad(request);
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, "Failed to upload your road");
				}

				MainThreadDispatcher.RegisterUpdater(() => FinishUpload(config, id, result));
			});
		}

		// Runs on the main thread, the request is a snapshot so later edits can't race the upload
		private RoadBuilderEntryPost? CreateUploadRequest(INetworkBuilderPrefab prefab)
		{
			Mod.Log.Info(nameof(StartUpload));

			var roadBuilderGenerationDataSystem = World.GetExistingSystemManaged<RoadBuilderGenerationDataSystem>();
			var config = prefab.Config;

			if (config is null || roadBuilderGenerationDataSystem.RoadGenerationData is null)
			{
				return null;
			}

			var svg = new ThumbnailGenerationUtil(prefab, roadBuilderGenerationDataSystem.RoadGenerationData).GenerateSvg();
			using var memory = new MemoryStream();

			svg?.Save(memory, SaveOptions.DisableFormatting);

			var originalId = config.OriginalID;
			string json;

			config.Version = RoadBuilderSerializeSystem.CURRENT_VERSION;
			config.Type = config.GetType().Name;

			// OriginalID tracks the local file, it is only matched to the ID in the uploaded copy
			try
			{
				config.OriginalID = config.ID;

				json = JSON.Dump(config, EncodeOptions.CompactPrint);
			}
			finally
			{
				config.OriginalID = originalId;
			}

			return new()
			{
				ID = config.ID,
				Category = (int)config.Category,
				Name = config.Name,
				Icon = Encoding.UTF8.GetString(memory.ToArray()),
				Config = json
			};
		}

		private void FinishUpload(INetworkConfig config, string? id, ApiResponse? result)
		{
			Uploading.Value = false;

			if (result?.success == true)
			{
				Mod.Log.Info("Upload Successful");

				// The road may have been edited into a new version during the upload
				if (config.ID == id)
				{
					config.Uploaded = true;
				}
			}
			else
			{
				Mod.Log.Error("Failed to upload your road: " + (result?.message ?? "UNKNOWN"));
			}

			UpdateSelectedRoadOptions();
		}
	}
}
