﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using GalaSoft.MvvmLight.Ioc;
using MALClient.Adapters;
using MALClient.Adapters.Credentials;
using MALClient.Models.Interfaces;
using MALClient.UWP.Adapters;
using MALClient.XShared.BL;
using MALClient.XShared.Interfaces;
//using IHandyDataStorage = MALClient.XShared.ViewModels.Interfaces.IHandyDataStorage;

namespace MALClient.XShared.ViewModels {
	public static class ResourceLocator {
		//public static IApplicationDataService ApplicationDataService
		//	=> SimpleIoc.Default.GetInstance<IApplicationDataService>();

		//public static IPasswordVault PasswordVaultProvider => SimpleIoc.Default.GetInstance<IPasswordVault>();

		//public static IDataCache DataCacheService => SimpleIoc.Default.GetInstance<IDataCache>();

		//public static IMessageDialogProvider MessageDialogProvider => SimpleIoc.Default.GetInstance<IMessageDialogProvider>();

		//public static IClipboardProvider ClipboardProvider => SimpleIoc.Default.GetInstance<IClipboardProvider>();

		//public static ISystemControlsLauncherService SystemControlsLauncherService => SimpleIoc.Default.GetInstance<ISystemControlsLauncherService>();

		//public static ILiveTilesManager LiveTilesManager => SimpleIoc.Default.GetInstance<ILiveTilesManager>();

		//public static IImageDownloaderService ImageDownloaderService => SimpleIoc.Default.GetInstance<IImageDownloaderService>();

		//public static ITelemetryProvider TelemetryProvider => SimpleIoc.Default.GetInstance<ITelemetryProvider>();

		//public static INotificationsTaskManager NotificationsTaskManager => SimpleIoc.Default.GetInstance<INotificationsTaskManager>();

		//public static IChangeLogProvider ChangelogProvider => SimpleIoc.Default.GetInstance<IChangeLogProvider>();

		//public static IHandyDataStorage HandyDataStorage => SimpleIoc.Default.GetInstance<IHandyDataStorage>();

		//public static ISchdeuledJobsManger SchdeuledJobsManger => SimpleIoc.Default.GetInstance<ISchdeuledJobsManger>();

		//public static ICssManager CssManager => SimpleIoc.Default.GetInstance<ICssManager>();

		//public static IMalHttpContextProvider MalHttpContextProvider => new MalHttpContextProvider();

		//public static ISnackbarProvider SnackbarProvider => SimpleIoc.Default.GetInstance<ISnackbarProvider>();

		public static IConnectionInfoProvider ConnectionInfoProvider => new ConnectionInfoProvider();

		//public static IDispatcherAdapter DispatcherAdapter => SimpleIoc.Default.GetInstance<IDispatcherAdapter>();

		//public static IAiringInfoProvider AiringInfoProvider => SimpleIoc.Default.GetInstance<IAiringInfoProvider>();

		//public static IDialogsProvider DialogsProvider => SimpleIoc.Default.GetInstance<IDialogsProvider>();


		// Purely shared interfaces

		//public static IAnimeLibraryDataStorage AnimeLibraryDataStorage => SimpleIoc.Default.GetInstance<IAnimeLibraryDataStorage>();

		public static void RegisterBase() {
			ConnectionInfoProvider.Init();
		}

		#region UsedByBackgroundTask

		/*public static void RegisterPasswordVaultAdapter(IPasswordVault vault) {
			SimpleIoc.Default.Register<IPasswordVault>(() => vault);
		}

		public static void RegisterAppDataServiceAdapter(IApplicationDataService appData) {
			SimpleIoc.Default.Register<IApplicationDataService>(() => appData);
		}

		public static void RegisterMessageDialogAdapter(IMessageDialogProvider msgDialog) {
			SimpleIoc.Default.Register<IMessageDialogProvider>(() => msgDialog);
		}

		public static void RegisterHttpContextAdapter(IMalHttpContextProvider httpContext) {
			SimpleIoc.Default.Register<IMalHttpContextProvider>(() => httpContext);
		}

		public static void RegisterDataCacheAdapter(IDataCache dataCache) {
			SimpleIoc.Default.Register<IDataCache>(() => dataCache);
		}*/
		#endregion

	}
}
