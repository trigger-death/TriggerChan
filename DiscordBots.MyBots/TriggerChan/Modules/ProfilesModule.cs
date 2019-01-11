using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.TriggerChan.Profiles;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Profiles")]
	[Summary("Commands for managing user profile information")]
	[IsLockable(true)]
	public partial class ProfilesModule : DiscordBotModule {

		public ProfilesModule(DiscordBotServiceContainer services) : base(services) { }

		[Group("mal")]
		[Usage("[<user>|profile <malUser>|assign <malUser>|unassign]")]
		[Summary("Lookup or register your MyAnimeList profile")]
		public class MALGroup : DatabaseProfileModule {

			public MALGroup(TriggerServiceContainer services)
				: base(services, DatabaseType.MyAnimeList, "mal") { }
			
			[Name("mal")]
			[Command("")]
			[Example("Get your MyAnimeList profile registed with me")]
			public Task MyProfile() {
				return MyProfileAsync();
			}

			[Name("mal <user>")]
			[Command("")]
			[Example(@"@​trigger_death", "Gets *trigger_death's* MyAnimeList profile registed with me")]
			[Usage("<user>")]
			public Task UserProfile(IUser user) {
				return UserProfileAsync(user);
			}
			[Name("mal profile <malUser>")]
			[Command("profile")]
			[Summary("Lookup the MyAnimeList user's profile")]
			[Usage("<malUser>")]
			[Example("trigger_death", "Gets the profile of the MyAnimeList user *trigger_death*")]
			[Example("https://myanimelist.net/profile/trigger_death", "Gets the profile of the MyAnimeList user *trigger_death*")]
			public Task<RuntimeResult> LookupProfile(string input) {
				return LookupProfileAsync(input);
			}

			[Name("mal assign <malUser>")]
			[Command("assign")]
			[Example("trigger_death", "Registers your MyAnimeList profile with me as *trigger_death*")]
			[Example("https://myanimelist.net/profile/trigger_death", "Registers your MyAnimeList profile with me as *trigger_death*")]
			[Usage("<malUser>")]
			public Task<RuntimeResult> Assign(string input) {
				return AssignAsync(input);
			}

			[Name("mal unassign")]
			[Command("unassign")]
			[Example("Unregisters your MyAnimeList profile with me")]
			public Task<RuntimeResult> Unassign() {
				return UnassignAsync();
			}
		}
		
		[Group("ani"), Alias("anilist")]
		[Usage("[<user>|profile <aniUser>|assign <aniUser>|unassign]")]
		[Summary("Lookup or register your AniList profile")]
		public class AniListGroup : DatabaseProfileModule {

			public AniListGroup(TriggerServiceContainer services)
				: base(services, DatabaseType.AniList, "ani") { }

			[Name("ani")]
			[Command("")]
			[Example("Get your AniList profile registed with me")]
			public Task MyProfile() {
				return MyProfileAsync();
			}

			[Name("ani <user>")]
			[Command("")]
			[Example(@"@​trigger_death", "Gets *trigger_death's* AniList profile registed with me")]
			[Usage("<user>")]
			public Task UserProfile(IUser user) {
				return UserProfileAsync(user);
			}
			[Name("ani profile <aniUser>")]
			[Command("profile")]
			[Summary("Lookup the AniList user's profile")]
			[Usage("<aniUser>")]
			[Example("trigger_death", "Gets the profile of the AniList user *trigger_death*")]
			[Example("https://myanimelist.net/profile/trigger_death", "Gets the profile of the AniList user *trigger_death*")]
			public Task<RuntimeResult> LookupProfile(string input) {
				return LookupProfileAsync(input);
			}

			[Name("ani assign <aniUser>")]
			[Command("assign")]
			[Example("trigger_death", "Registers your AniList profile with me as *trigger_death*")]
			[Example("https://myanimelist.net/profile/trigger_death", "Registers your AniList profile with me as *trigger_death*")]
			[Usage("<aniUser>")]
			public Task<RuntimeResult> Assign(string input) {
				return AssignAsync(input);
			}

			[Name("ani unassign")]
			[Command("unassign")]
			[Example("Unregisters your AniList profile with me")]
			public Task<RuntimeResult> Unassign() {
				return UnassignAsync();
			}
		}

		[Group("mfc")]
		[Usage("[<user>|profile <mfcUser>|assign <mfcUser>|unassign]")]
		[Summary("Lookup or register your MyFigureCollection profile")]
		public class MFCGroup : DatabaseProfileModule {

			public MFCGroup(TriggerServiceContainer services)
				: base(services, DatabaseType.MyFigureCollection, "mfc") { }

			[Name("mfc")]
			[Command("")]
			[Example("Get your MyFigureCollection profile registed with me")]
			public Task MyProfile() {
				return MyProfileAsync();
			}

			[Name("mfc <user>")]
			[Command("")]
			[Example(@"@​trigger_death", "Gets *trigger_death's* MyFigureCollection profile registed with me")]
			[Usage("<user>")]
			public Task UserProfile(IUser user) {
				return UserProfileAsync(user);
			}
			[Name("mfc profile <mfcUser>")]
			[Command("profile")]
			[Summary("Lookup the MyFigureCollection user's profile")]
			[Usage("<mfcUser>")]
			[Example("trigger_death", "Gets the profile of the MyFigureCollection user *trigger_death*")]
			[Example("https://myfigurecollection.net/profile/trigger_death", "Gets the profile of the MyFigureCollection user *trigger_death*")]
			public Task<RuntimeResult> LookupProfile(string input) {
				return LookupProfileAsync(input);
			}

			[Name("mfc assign <mfcUser>")]
			[Command("assign")]
			[Example("trigger_death", "Registers your MyFigureCollection profile with me as *trigger_death*")]
			[Example("https://myfigurecollection.net/profile/trigger_death", "Registers your MyFigureCollection profile with me as *trigger_death*")]
			[Usage("<mfcUser>")]
			public Task<RuntimeResult> Assign(string input) {
				return AssignAsync(input);
			}

			[Name("mfc unassign")]
			[Command("unassign")]
			[Example("Unregisters your MyFigureCollection profile with me")]
			public Task<RuntimeResult> Unassign() {
				return UnassignAsync();
			}
		}

		/*[Group("profile")]
		public class ProfileGroup : DiscordBotModule {

			public ProfileGroup(TriggerServiceContainer services) : base(services) { }

		}*/
	}
}
