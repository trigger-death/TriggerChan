using System;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Database;
using TriggersTools.DiscordBots.TriggerChan.Model;
using TriggersTools.DiscordBots.Database;
using OldGuild = TriggersTools.DiscordBots.TriggerChan.Models.Guild;
using OldGuildUser = TriggersTools.DiscordBots.TriggerChan.Models.GuildUser;
using OldSpoiler = TriggersTools.DiscordBots.TriggerChan.Models.Spoiler;
using OldSpoiledUser = TriggersTools.DiscordBots.TriggerChan.Models.SpoiledUser;
using System.Collections.Generic;
using System.Linq;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using System.Diagnostics;

namespace TriggerChan.v2.Migration {
	class Program {
		static void Report(Stopwatch watch) {
			Console.WriteLine($"Took {watch.ElapsedMilliseconds:N0}ms");
			watch.Restart();
		}

		static void Main(string[] args) {
			Stopwatch watchFull = Stopwatch.StartNew();
			Console.WriteLine("==== Trigger-Chan v2 Migration ====");
			using (var oldDb = new BotDatabaseContext())
			using (var newDb = new TriggerDbContextFactory().CreateDbContext()) {
				Stopwatch watch = Stopwatch.StartNew();
				int count = 0, count2 = 0;
				Console.Write("Adding Zero-Id Entities...");
				// Because Spoiler.GuildId's are 0
				newDb.Guilds.Add(new Guild {
					Id = 0,
					AllowBotSpoilers = false,
					SpellCheckSpoilers = true,
					PinReactCount = 0,
					TalkBackEnabled = false,
					TalkBackCooldown = TimeSpan.Zero,
				});
				// Because Spoiler.AuthorId's are 0
				newDb.Users.Add(new User {
					Id = 0,
				});
				Console.WriteLine(" Added!");
				Console.Write("Migrating Guilds...");
				foreach (var oldGuild in oldDb.Guilds) {
					Guild newGuild = new Guild {
						Id = oldGuild.GuildId,
						AllowBotSpoilers = false,
						SpellCheckSpoilers = true,
						PinReactCount = oldGuild.PinReactCount,
						TalkBackEnabled = oldGuild.TalkBack,
						TalkBackCooldown = oldGuild.TalkBackCooldown,
						Prefix = oldGuild.Prefix,
					};
					newGuild.PublicRoles.AddRange(oldGuild.GetPublicRoles());
					newDb.Guilds.Add(newGuild);
					count++;
				}
				Console.WriteLine($" {count} Guilds migrated!");
				Report(watch);
				count = 0;
				Console.Write("Migrating GuildUsers...");
				Dictionary<ulong, List<OldGuildUser>> oldGuildUsers = new Dictionary<ulong, List<OldGuildUser>>();
				List<ulong> GuildPriorities = new List<ulong> {
					435855847592296458, // iMAL
					317924870950223872, // Nulls
					446607199695929355, // Degenerates
					163412463239430154, // Terraria Raids
					343060137164144642, // Annak's Lair
					482770853839372288, // Japanese;Shaman;Girls
					436949335947870238, // trigger_bot_testing
				};
				foreach (var oldUser in oldDb.GuildUsers) {
					if (!oldGuildUsers.TryGetValue(oldUser.UserId, out var guilds)) {
						guilds = new List<OldGuildUser>();
						oldGuildUsers.Add(oldUser.UserId, guilds);
					}
					guilds.Add(oldUser);
					count++;
				}
				foreach (var oldUsers in oldGuildUsers.Values) {
					var first = oldUsers.First();
					User newUser = new User {
						Id = first.UserId,
					};
					newDb.Users.Add(newUser);
					var orderedUsers = oldUsers.OrderBy(u => GuildPriorities.IndexOf(u.GuildId)).ToArray();
					T Find<T>(Func<OldGuildUser, T> find, T defaultValue = null) where T : class {
						foreach (OldGuildUser oldUser in orderedUsers) {
							T value = find(oldUser);
							if (value != null)
								return value;
						}
						return defaultValue;
					}
					var newProfile = new UserProfile {
						Id = first.UserId,
						MALUsername = Find(u => u.MALUsername),
						AniListUsername = Find(u => u.AniListUsername),
						MFCUsername = Find(u => u.MFCUsername),
						TimeZone = Find(u => u.TimeZone),
					};
					newDb.UserProfiles.Add(newProfile);
					count2++;
				}
				Console.WriteLine($" {count} GuildUsers migrated to {count2} Users & UserProfiles!");
				Report(watch);
				count = 0; count2 = 0;
				Console.Write("Migrating Spoilers...");
				foreach (var oldSpoiler in oldDb.Spoilers) {
					Spoiler newSpoiler = new Spoiler {
						MessageId = oldSpoiler.MessageId,
						Content = oldSpoiler.Content,
						AttachmentUrl = oldSpoiler.Url,
						TimeStamp = oldSpoiler.TimeStamp,
						RawDisplay = true, // Old spoilers should follow the old format to make sure they still work.
					};
					newDb.Spoilers.Add(newSpoiler);
					count++;
				}
				Console.WriteLine($" {count} Spoilers migrated!");
				Report(watch);
				count = 0;
				Console.Write("Migrating SpoiledUsers...");
				foreach (var oldSpoiledUser in oldDb.SpoiledUsers) {
					SpoiledUser newSpoiledUser = new SpoiledUser {
						MessageId = oldSpoiledUser.MessageId,
						UserId = oldSpoiledUser.UserId,
						DMMessageIdA = oldSpoiledUser.UserMessageId,
						// No more TimeStamp field
					};
					// Add missing users
					newDb.FindUserAsync(newSpoiledUser.UserId).Wait();
					newDb.SpoiledUsers.Add(newSpoiledUser);
					count++;
				}
				Console.WriteLine($" {count} SpoiledUsers migrated!");
				Report(watch);
				count = 0;
				int changes = newDb.SaveChanges();
				Console.WriteLine($"Saved {changes} changes!");
			}
			Console.WriteLine("Finished!");
			Report(watchFull);
			Console.Beep();
			Console.Beep();
			Console.ReadLine();
		}
	}
}
