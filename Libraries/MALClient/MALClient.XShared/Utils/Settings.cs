using System;
using System.Collections.Generic;
using System.Linq;
using MALClient.Adapters;
using MALClient.Models.Enums;
using MALClient.Models.Models.Anime;
using MALClient.XShared.Comm;
//using MALClient.XShared.ViewModels;

namespace MALClient.XShared.Utils {
	public static class Settings {
		//private static readonly IApplicationDataService ApplicationDataService;

		static Settings() {
			//ApplicationDataService = ResourceLocator.ApplicationDataService;
		}

		public static ApiType SelectedApiType { get; set; } = ApiType.Mal;

		public static int CachePersitence { get; set; } = 7200;

		public static SortOptions AnimeSortOrder { get; set; } = 0;

		public static SortOptions MangaSortOrder { get; set; } = 0;

		public static bool PreferEnglishTitles { get; set; } = false;

		public static bool IsSortDescending { get; set; } = false;

		public static bool IsMangaSortDescending { get; set; } = false;

		public static bool MangaFocusVolumes { get; set; } = false;

		public static int DefaultAnimeFilter { get; set; } = (int) AnimeStatus.Watching;

		public static int DefaultMangaFilter { get; set; } = (int) AnimeStatus.Watching;

		public static int ReviewsToPull { get; set; } = 20;

		public static int RecommsToPull { get; set; } = 20;

		public static int SeasonalToPull { get; set; } = 45;

		public static int AirDayOffset { get; set; } = 0;

		public static int AiringNotificationOffset { get; set; } = 0;

		public static string DefaultMenuTab { get; set; } = "anime";

		public static bool DetailsAutoLoadDetails { get; set; } = false;

		public static bool DetailsAutoLoadReviews { get; set; } = false;

		public static bool DetailsAutoLoadRelated { get; set; } = false;

		public static bool DetailsAutoLoadRecomms { get; set; } = false;


		public static bool HideFilterSelectionFlyout { get; set; } = false;

		public static bool HideViewSelectionFlyout { get; set; } = true;

		public static bool HideSortingSelectionFlyout { get; set; } = false;

		public static bool HamburgerAnimeFiltersExpanded { get; set; } = false;

		public static bool HamburgerTopCategoriesExpanded { get; set; } = true;

		public static bool AnimeListEnsureSelectedItemVisibleAfterOffContentCollapse { get; set; } = false;

		public static bool HamburgerMangaFiltersExpanded { get; set; } = false;

		public static bool HamburgerHideMangaSection { get; set; } = false;

		public static bool HamburgerMenuDefaultPaneState { get; set; } = true;

		public static DataSource PrefferedDataSource { get; set; } = DataSource.AnnHum;

		public static bool EnableHearthAnimation { get; set; } = true;

		public static bool EnableSwipeToIncDec { get; set; } = true;

		public static bool ReverseSwipingDirection { get; set; } = false;

		public static bool DisplayScoreDialogAfterCompletion { get; set; } = false;

		public static bool EnableOfflineSync { get; set; } = true;

		public static bool AnimeSyncRequired { get; set; } = false;

		public static bool MangaSyncRequired { get; set; } = false;

		public static bool DetailsListRecomsView { get; set; } = true;

		public static bool DetailsListReviewsView { get; set; } = true;

		public static bool EnsureRandomizerAlwaysSelectsWinner { get; set; } = false;

		public static bool PullHigherQualityImagesDefault = true;
		public static bool PullHigherQualityImages { get; set; } = PullHigherQualityImagesDefault;

		public static bool ForceSearchIntoOffPage { get; set; } = false;

		public static string AppVersion { get; set; } = "1.0.0.0";

		public static AnimeStatus DefaultStatusAfterAdding { get; set; } = AnimeStatus.PlanToWatch;

		#region Views

		public static AnimeListDisplayModes WatchingDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static AnimeListDisplayModes CompletedDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static AnimeListDisplayModes OnHoldDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static AnimeListDisplayModes DroppedDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static AnimeListDisplayModes PlannedDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static AnimeListDisplayModes AllDisplayMode { get; set; } = AnimeListDisplayModes.IndefiniteGrid;

		public static bool LockDisplayMode { get; set; } = false;

		public static bool AutoDescendingSorting { get; set; } = true;

		public static bool DisplaySeasonWithType { get; set; } = false;

		public static bool EnableImageCache { get; set; } = true;

		#endregion

		#region StartEndDates

		public static bool SetStartDateOnWatching { get; set; } = true;

		public static bool SetStartDateOnListAdd { get; set; } = false;

		public static bool SetEndDateOnDropped { get; set; } = false;

		public static bool SetEndDateOnCompleted { get; set; } = true;

		public static bool OverrideValidStartEndDate { get; set; } = false;

		#endregion

		#region Calendar

		public static bool CalendarIncludeWatching { get; set; } = true;

		public static bool CalendarIncludePlanned { get; set; } = false;

		public static bool CalendarSwitchMonSun { get; set; } = false;

		public static bool CalendarStartOnToday { get; set; } = false;

		public static bool CalendarRemoveEmptyDays { get; set; } = true;

		public static bool CalendarPullExactAiringTime { get; set; } = true;

		#endregion
		
		

		
		

		#region Feeds

		public static bool FeedsIncludePinnedProfiles { get; set; } = false;

		public static int FeedsMaxEntries { get; set; } = 5;

		public static int FeedsMaxEntryAge { get; set; } = 7;

		#endregion
	}
}