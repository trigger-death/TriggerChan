using Discord;
using Discord.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.SpoilerBot.Database;
using TriggersTools.DiscordBots.SpoilerBot.Model;
using TriggersTools.DiscordBots.SpoilerBot.Utils;

namespace TriggersTools.DiscordBots.SpoilerBot.Services {
	partial class SpoilerService<TDbContext> {

		#region SpoilTask

		/// <summary>
		/// The state of spoiling or unspoiling a user.
		/// </summary>
		private enum SpoilState {
			/// <summary>
			/// The spoiler is being sent to the user's DM.
			/// </summary>
			Spoil,
			/// <summary>
			/// The spoiler is being removed from the user's DM.
			/// </summary>
			Unspoil,
		}

		/// <summary>
		/// A task for a spoiler being added or removed.
		/// </summary>
		private class SpoilTask : Ref {
			/// <summary>
			/// The task being executed.
			/// </summary>
			public Task Task { get; set; } = null;
			/// <summary>
			/// The current state of the task, spoiling or unspoiling the user.
			/// </summary>
			public SpoilState State { get; set; }
			/// <summary>
			/// The desired state. This value is needed when the user is spamming the spoiler reaction.
			/// </summary>
			public SpoilState DesiredState { get; set; }

			/// <summary>
			/// Gets if the task is still running.
			/// </summary>
			public bool IsRunning => Task != null && !Task.IsCompleted;
		}

		#endregion

		#region SpoilTaskCollection

		/// <summary>
		/// A collection of tasks for managing spoiling and unspoiling of a user.
		/// </summary>
		private class SpoilTaskCollection {

			#region Fields

			/// <summary>
			/// The dictionary spoiler tasks. Key1=SpoilerId, Key2=UserId
			/// </summary>
			private readonly ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SpoilTask>> spoilers
				= new ConcurrentDictionary<ulong, ConcurrentDictionary<ulong, SpoilTask>>();

			#endregion

			#region Create/Remove

			/// <summary>
			/// Gets the existing spoil task, or creates a new one and adds a reference to it.
			/// </summary>
			/// <param name="spoilerId">The message Id of the spoiler.</param>
			/// <param name="userId">The Id of the user this task relates to.</param>
			/// <returns>The existing or created <see cref="SpoilTask"/>.</returns>
			public SpoilTask GetOrCreateAndRef(ulong spoilerId, ulong userId) {
				if (!spoilers.TryGetValue(spoilerId, out var tasks)) {
					tasks = new ConcurrentDictionary<ulong, SpoilTask>();
					if (!spoilers.TryAdd(spoilerId, tasks))
						tasks = spoilers[spoilerId];
				}
				SpoilTask task;
				lock (tasks) {
					if (!tasks.TryGetValue(userId, out task)) {
						SpoilTask newTask = new SpoilTask();
						do {
							if (tasks.TryAdd(userId, newTask)) {
								task = newTask;
								Debug.WriteLine("Ref Created");
								break;
							}
						} while (!tasks.TryGetValue(userId, out task));
					}
					task.AddRef();
				}
				return task;
			}
			/// <summary>
			/// Removes the reference to the spoil task and removes it if it has no more references.
			/// </summary>
			/// <param name="spoilerId">The message Id of the spoiler.</param>
			/// <param name="userId">The Id of the user this task relates to.</param>
			/// <param name="task">The actual spoil task</param>
			/// <returns>True if the task was removed.</returns>
			public bool DerefAndRemove(ulong spoilerId, ulong userId, SpoilTask task) {
				task.RemoveRef();
				if (!task.IsUnused)
					return false;
				if (!spoilers.TryGetValue(spoilerId, out var tasks))
					return false;
				lock (tasks) {
					if (task.IsUnused) {
						if (!tasks.TryRemove(userId, out task))
							return false;
						else
							Debug.WriteLine("Ref removed");
						if (tasks.Count == 0)
							if (spoilers.TryRemove(spoilerId, out _))
								Debug.WriteLine("Spoiler tasks removed");
						return true;
					}
				}
				return false;
			}

			#endregion
		}

		#endregion

		#region Spoil/Unspoil User

		/// <summary>
		/// Tries to spoil the user after they reacted to the spoiler.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="user">The user to spoil.</param>
		/// <param name="force">True if this should be forced</param>
		private async Task SpoilUserAsync(ulong messageId, IUser user, bool force) {
			try {
				Spoiler spoiler;
				using (var db = GetDb<TDbContext>())
					spoiler = await FindSpoilerAsync(db, messageId).ConfigureAwait(false);
				if (spoiler == null)
					return;
				SpoilTask task = spoilerTasks.GetOrCreateAndRef(messageId, user.Id);
				lock (task) {
					Debug.WriteLine("SPOIL TASK LOCK");
					task.DesiredState = SpoilState.Spoil;
					if (!task.IsRunning || force) {
						task.Task = null;
						task.State = SpoilState.Spoil;
						Debug.WriteLine("SPOIL TASK");
						task.Task = Task.Run(() => SpoilUserTaskAsync(spoiler, user))
							.ContinueWith(async (t) => {
								bool revert;
								lock (task) {
									revert = task.DesiredState == SpoilState.Unspoil;
									// Remove the task
									if (!revert)
										task.Task = null;
								}
								if (revert)
									await UnspoilUserAsync(messageId, user, true).ConfigureAwait(false);
								spoilerTasks.DerefAndRemove(messageId, user.Id, task);
							});
					}
					else {
						spoilerTasks.DerefAndRemove(messageId, user.Id, task);
					}
					Debug.WriteLine("SPOIL TASK UNLOCK");
				}
			}
			catch (AggregateException) { }
			catch (TaskCanceledException) { }
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}
		/// <summary>
		/// Tries to unspoil the user after they unreacted to the spoiler.
		/// </summary>
		/// <param name="messageId">The message Id of the spoiler.</param>
		/// <param name="user">The user to unspoil.</param>
		/// <param name="force">True if this should be forced</param>
		private async Task UnspoilUserAsync(ulong messageId, IUser user, bool force) {
			try {
				Spoiler spoiler;
				using (var db = GetDb<TDbContext>())
					spoiler = await FindSpoilerAsync(db, messageId).ConfigureAwait(false);
				// Spoiler does not exist anymore. Goodbye
				if (spoiler == null)
					return;
				SpoilTask task = spoilerTasks.GetOrCreateAndRef(messageId, user.Id);
				lock (task) {
					Debug.WriteLine("UNSPOIL TASK LOCK");

					task.DesiredState = SpoilState.Unspoil;
					if (!task.IsRunning || force) {
						task.Task = null;
						task.State = SpoilState.Unspoil;
						Debug.WriteLine("UNSPOIL TASK");
						task.Task = Task.Run(() => UnspoilUserTaskAsync(spoiler, user))
							.ContinueWith(async (t) => {
								bool revert;
								lock (task) {
									revert = task.DesiredState == SpoilState.Spoil;
									// Remove the task
									if (!revert)
										task.Task = null;
								}
								if (revert)
									await SpoilUserAsync(messageId, user, true).ConfigureAwait(false);
								spoilerTasks.DerefAndRemove(messageId, user.Id, task);
							});
					}
					else {
						spoilerTasks.DerefAndRemove(messageId, user.Id, task);
					}
					Debug.WriteLine("UNSPOIL TASK UNLOCK");
				}
			}
			catch (AggregateException) { }
			catch (TaskCanceledException) { }
			catch (Exception ex) {
				Debug.WriteLine(ex.ToString());
			}
		}

		#endregion
		
		#region Spoil/Unspoil User Task

		/// <summary>
		/// The task run to actually spoil the user.
		/// </summary>
		/// <param name="spoiler">The spoiler being presented.</param>
		/// <param name="task">Unused</param>
		/// <param name="messageId">The message</param>
		/// <param name="user">The user being spoiled.</param>
		private async Task SpoilUserTaskAsync(Spoiler spoiler, IUser user) {
			using (var db = GetDb<TDbContext>()) {
				try {
					SpoiledUser spoiledUser = await FindSpoiledUserAsync(db, spoiler.MessageId, user.Id).ConfigureAwait(false);
					bool wasNull = spoiledUser == null;
					if (spoiledUser == null) {
						Debug.WriteLine("FIRST SPOIL");
						//SpoiledUser last = await db.SpoiledUsers.AsNoTracking().LastOrDefaultAsync();
						//ulong dbId = (last != null ? last.DbId + 1 : 0);
						spoiledUser = new SpoiledUser() {
							//DbId = dbId,
							MessageId = spoiler.MessageId,
							UserId = user.Id,
						};
						await db.SpoiledUsers.AddAsync(spoiledUser).ConfigureAwait(false);
						await db.EnsureEndUserData(spoiledUser).ConfigureAwait(false);
					}
					else {
						Debug.WriteLine("RESPOIL");
					}
					IMessage message = null;
					if (spoiledUser.DMMessageIdA != 0) {
						var channel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
						message = await channel.GetMessageAsync(spoiledUser.DMMessageIdA).ConfigureAwait(false);
					}
					if (message == null) {
						/*if (spoiler.Url != null) {
							using (HttpClient client = new HttpClient())
							using (Stream stream = await client.GetStreamAsync(spoiler.Url)) {
								spoiledUser.UserMessageId = (await user.SendFileAsync(stream, spoiler.Filename, spoiler.Content)).Id;
							}
						}
						else {*/
						await PostSpoilerContentAsync(user, spoiler, spoiledUser).ConfigureAwait(false);
						//var (dmMessageId, dmUrlMessageId) = 
						//	spoiledUser.UserMessageId = (await user.SendMessageAsync(spoiler.Content)).Id;
						//}
						Debug.WriteLine("SPOIL SUCCESS");
					}
					else {
						await message.DeleteAsync().ConfigureAwait(false);
						Debug.WriteLine("SPOIL FAILED");
					}
					if (!wasNull)
						db.SpoiledUsers.Update(spoiledUser);
					await db.SaveChangesAsync().ConfigureAwait(false);
				} catch (TaskCanceledException) {
					Debug.WriteLine("SPOIL CANCEL");
				} catch (TimeoutException) {
					Debug.WriteLine("SPOIL TIMEOUT");
				} catch (HttpException) {
					Debug.WriteLine("SPOIL HttpException");
				} catch (AggregateException) {
					Debug.WriteLine("SPOIL AGGREGATE");
				} catch (Exception ex) {
					Debug.WriteLine(ex.ToString());
				}
				Debug.WriteLine("SPOIL TASK END");
			}
		}
		/// <summary>
		/// The task run to actually unspoil the user.
		/// </summary>
		/// <param name="spoiler">The spoiler being removed.</param>
		/// <param name="user">The user being unspoiled.</param>
		private async Task UnspoilUserTaskAsync(Spoiler spoiler, IUser user) {
			using (var db = GetDb<TDbContext>()) {
				try {
					SpoiledUser spoiledUser = await FindSpoiledUserAsync(db, spoiler.MessageId, user.Id).ConfigureAwait(false);
					if (spoiledUser != null) {
						Debug.WriteLine("UNSPOIL");
						IMessage message = null;
						if (spoiledUser.DMMessageIdA != 0) {
							var channel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
							message = await channel.GetMessageAsync(spoiledUser.DMMessageIdA).ConfigureAwait(false);
							if (message != null) {
								await message.DeleteAsync().ConfigureAwait(false);
								Debug.WriteLine("UNSPOIL A SUCCESS");
								spoiledUser.DMMessageIdA = 0;
							}
							else {
								Debug.WriteLine("UNSPOIL A FAILED");
							}
						}
						if (spoiledUser.DMMessageIdB != 0) {
							var channel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
							message = await channel.GetMessageAsync(spoiledUser.DMMessageIdB).ConfigureAwait(false);
							if (message != null) {
								await message.DeleteAsync().ConfigureAwait(false);
								Debug.WriteLine("UNSPOIL B SUCCESS");
								spoiledUser.DMMessageIdB = 0;
							}
							else {
								Debug.WriteLine("UNSPOIL B FAILED");
							}
						}
						if (spoiledUser.DMMessageIdC != 0) {
							var channel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
							message = await channel.GetMessageAsync(spoiledUser.DMMessageIdC).ConfigureAwait(false);
							if (message != null) {
								await message.DeleteAsync().ConfigureAwait(false);
								Debug.WriteLine("UNSPOIL C SUCCESS");
								spoiledUser.DMMessageIdC = 0;
							}
							else {
								Debug.WriteLine("UNSPOIL C FAILED");
							}
						}
						db.SpoiledUsers.Remove(spoiledUser);
						await db.SaveChangesAsync().ConfigureAwait(false);
					}
				}
				catch (TaskCanceledException) {
					Debug.WriteLine("UNSPOIL CANCEL");
				} catch (TimeoutException) {
					Debug.WriteLine("UNSPOIL TIMEOUT");
				} catch (HttpException) {
					Debug.WriteLine("UNSPOIL HttpException");
				} catch (AggregateException) {
					Debug.WriteLine("UNSPOIL AGGREGATE");
				} catch (Exception ex) {
					Debug.WriteLine(ex.ToString());
				}
				Debug.WriteLine("UNSPOIL TASK END");
			}
		}

		#endregion
	}
}
