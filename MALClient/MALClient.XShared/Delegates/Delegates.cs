﻿using System;
using System.Threading.Tasks;
using MALClient.Models.Enums;
using MALClient.XShared.ViewModels;

namespace MALClient.XShared.Delegates {
	public delegate void OffContentPaneStateChanged();

	public delegate void AnimeItemListInitialized();

	//public delegate void ScrollIntoViewRequest(AnimeItemViewModel item, bool select = false);

	public delegate void SelectionResetRequest(AnimeListDisplayModes mode);

	public delegate void SortingSettingChange(SortOptions option, bool descencing);

	public delegate void NavigationRequest(Type page, object args = null);

	public delegate void AmbiguousNavigationRequest<T>(T target, object args);

	public delegate void WebViewNavigationRequest(string content, bool arg);

	public delegate void PivotItemSelectionRequest(int index);

	public delegate void EmptyEventHander();

	public delegate void SearchQuerySubmitted(string query);

	public delegate void SearchDelayedQuerySubmitted(string query);

	public delegate void SettingsNavigationRequest(SettingsPageIndex page);

	public delegate void BackgroundTaskCall(BgTasks task);

	//public delegate void AnimeAbstractionTransaction(AnimeItemAbstraction abstraction);

	public delegate Task<string> NavigationInterceptPossible(string targetUrl);
}
