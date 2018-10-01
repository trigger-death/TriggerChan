using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Context;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Models;
using TriggersTools.DiscordBots.TriggerChan.Util;
using TriggersTools.SteinsGate;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class FunService : BotServiceBase {
		
		private Regex[] ILoveRegex;
		
		protected override void OnInitialized(ServiceProvider services) {
			base.OnInitialized(services);
			Divergence.EnableLimits = true;
			Divergence.MaxLength = 24;
			Divergence.MaxLines = 3;
			Client.MessageReceived += OnMessageReceived;
			Client.ReactionAdded += OnReactionAdded;
			InitILoveTriggerChanRegex();
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			if (arg3.Emote.Equals(BotReactions.PinMessage)) {
				IUserMessage message = null;
				if (arg3.Message.IsSpecified && arg3.Message.Value != null)
					message = arg3.Message.Value;
				else
					message = await arg2.GetMessageAsync(arg1.Id) as IUserMessage;
				if (!message.IsPinned) {
					int pinCount = (await Settings.GetSettings(message)).PinReactCount;
					if (pinCount <= 0)
						return;
					int newCount = message.Reactions[BotReactions.PinMessage].ReactionCount;
					if (newCount >= pinCount) {
						await message.PinAsync();
					}
				}
			}
		}

		public async Task<int> GetPinReactCount(SocketCommandContext context) {
			return (await Settings.GetSettings(context)).PinReactCount;
		}

		public async Task SetPinReactCount(SocketCommandContext context, int count) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);

				if (settings.PinReactCount != count) {
					settings.PinReactCount = count;
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
				}
			}
		}

		public void ResetTalkbackCooldown(SocketCommandContext context) {
			LocalChannelBase channel = Settings.GetLocalChannel(context);
			channel.TalkBackTimer.Reset();
		}

		public async Task<TimeSpan> GetTalkBackCooldown(SocketCommandContext context) {
			return (await Settings.GetSettings(context)).TalkBackCooldown;
		}

		public async Task SetTalkBackCooldown(SocketCommandContext context, TimeSpan time) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);

				if (settings.TalkBackCooldown != time) {
					settings.TalkBackCooldown = time;
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
				}
			}
		}

		public async Task<bool> GetTalkBack(SocketCommandContext context) {
			return (await Settings.GetSettings(context)).TalkBack;
		}
		
		public async Task<bool> SetTalkBack(SocketCommandContext context, bool enabled) {
			using (var database = new BotDatabaseContext()) {
				SettingsBase settings = await Settings.GetSettings(database, context, true);

				if (settings.TalkBack != enabled) {
					settings.TalkBack = enabled;
					database.UpdateSettings(settings);
					await database.SaveChangesAsync();
					return true;
				}
				return false;
			}
		}

		private void InitILoveTriggerChanRegex() {
			List<string> names = new List<string>();
			if (Config["username"] != null) {
				names.Add(Config["username"]);
				names.Add("@" + Config["username"]);
			}
			if (Config["client_id"] != null) {
				names.Add($"<@{Config["client_id"]}>");
			}
			const string W = @"(\s|^)+";
			names.AddRange(new string[] {
				$"triggerchan",
				$"trigger_chan",
				$"trigger-chan",
				$"trigger chan"
			});

			string name = "(";
			for (int i = 0; i < names.Count; i++) {
				if (i != 0)
					name += "|";
				name += Regex.Escape(names[i]);
			}
			name += ")";

			const string I = @"i";
			const string Really = @"(re*a*l+y+)";
			const string Love = @"(l+o+v+e+s*|l+u+v+s*|.?❤.?|\<3+|le?i+e?k+e*s*|daisuki|daisuke|suki)";
			const string Daisuki = @"(daisuki|daisuke|suki|.?❤.?|\<3+)";
			const string Heart = @"(.?❤.?)";
			const string Dont = @"(?<!(do?n'?t|do?e?sn'?t|donn?ot|do not))";
			const string You = @"(yo+u+|u+)";
			const string Begin = @"(\s|^)";
			const string End = @"(\s|$)";

			string[] patterns = new string[] {
				$"{Dont}{W}{Love}{W}({You}{W})?{name}{End}",
				$"{Begin}{name}{W}{I}{W}({Really}{W})*{Love}{W}{You}{End}",
				$"{Dont}{W}{Heart}{W}{name}{End}",
				$"{Begin}{name}{W}{Heart}{End}",
				$"{Begin}{Daisuki}{W}{name}{End}",
				$"{Begin}{name}{W}{Daisuki}{End}",
			};

			ILoveRegex = new Regex[patterns.Length];
			RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
			for (int i = 0; i < patterns.Length; i++) {
				Debug.WriteLine(patterns[i]);
				ILoveRegex[i] = new Regex(patterns[i], options);
			}
		}

		private async Task OnMessageReceived(SocketMessage s) {
			var msg = s as SocketUserMessage;     // Ensure the message is from a user/bot
			if (msg == null)
				return;

			if (msg.Author.IsBot)
				return;

			var context = new BotCommandContext(this, msg);

			if (context.Guild != null) {
				LocalGuild guild = Settings.GetLocalGuild(context.Guild.Id);
				bool success = false;
				lock (guild) {
					if (guild.Asciify != null && guild.Asciify.ExpireTimer != null &&
						guild.Asciify.User.Id == msg.Author.Id && msg.Attachments.Any() &&
						context.Channel.Id == guild.Asciify.Channel.Id)
					{
						guild.Asciify.AttachmentMessage = msg;
						guild.Asciify.Attachment = msg.Attachments.First();
						guild.Asciify.ExpireTimer.Dispose();
						guild.Asciify.ExpireTimer = null;
						success = true;
					}
				}
				if (success)
					await AsciifyImage(context, guild, guild.Asciify);
			}

			if (await GetTalkBack(context)) {
				await ILoveTriggerChan(msg);
			}
		}


		private async Task ILoveTriggerChan(SocketUserMessage msg) {
			var context = new SocketCommandContext(Client, msg);

			LocalChannelBase channel = Settings.GetLocalChannel(msg.Channel);
			if (channel.TalkBackTimer == null)
				channel.TalkBackTimer = new Stopwatch();
			Stopwatch timer = channel.TalkBackTimer;
			TimeSpan cooldown = (await Settings.GetSettings(context)).TalkBackCooldown;

			if (timer.IsRunning && timer.Elapsed < cooldown)
				return;

			// Message Length Cap?
			// Tradeoff: People can't make long rants about their love.
			//if (msg.Content.Length > 500)
			//	return;

			string text = 
			text = msg.Content
				.Replace('.', ' ')
				.Replace(',', ' ')
				.Replace(';', ' ')
				.Replace('!', ' ')
				.Replace('?', ' ')
				.Replace('-', ' ')
				.Replace('_', ' ')
				.Replace('\n', ' ')
				.Replace('\r', ' ');

			//foreach (char c in text)
			//	Debug.Write(((int) c) + "|");
			//Debug.WriteLine("");
			//Debug.WriteLine(text);

			int index = 0;
			Stopwatch watch = Stopwatch.StartNew();
			foreach (Regex regex in ILoveRegex) {
				if (regex.IsMatch(text)) {
					await msg.Channel.SendFileAsync(BotResources.Well_I_Dont_Love_You);
					channel.TalkBackTimer.Restart();
					Console.WriteLine(index);
					break;
				}
				index++;
			}
			//Debug.WriteLine(watch.ElapsedMilliseconds);
		}

		public async Task AsciifyImage(SocketCommandContext context, LocalGuild guild,
			AsciifyParameters asciify)
		{
			string ext = Path.GetExtension(asciify.Attachment.Filename).ToLower();
			if (ext != ".png" && ext != ".bmp" && ext != ".jpg") {
				await asciify.Channel.SendMessageAsync("Image must be a png, jpg, or bmp");
				lock (guild) {
					guild.Asciify = null;
				}
				return;
			}
			lock (asciify) {
				asciify.Task = Task.Run(() => AsciifyTask(context, guild, asciify))
					.ContinueWith((t) => {
						lock (guild) {
							asciify.Task = null;
							guild.Asciify = null;
						}
					});
			}
		}

		public async Task AsciifyTask(SocketCommandContext context, LocalGuild guild,
			AsciifyParameters asciify)
		{
			float scale = asciify.Scale;
			float scaleP = asciify.Scale * 100;
			int smoothness = asciify.Smoothness;
			string filename = asciify.Attachment.Filename;
			string asciifyStr = $"**Asciifying:** `{filename}` with Smoothness {smoothness}, Scale {scaleP}%";
			var asciifyMsg = await context.Channel.SendMessageAsync(asciifyStr);
			try {
				using (var typing = context.Channel.EnterTypingState()) {
					Stopwatch watch = Stopwatch.StartNew();
					SettingsBase settings = await Settings.GetSettings(context);
					IAttachment attach = context.Message.Attachments.First();
					string url = attach.Url;
					string ext = Path.GetExtension(filename);
					string inputFile = BotResources.GetAsciifyIn(settings.Id, ext);
					string outputFile = BotResources.GetAsciifyOut(settings.Id);


					using (HttpClient client = new HttpClient())
						File.WriteAllBytes(inputFile, await client.GetByteArrayAsync(url));
					//using (Stream stream = await client.GetStreamAsync(url)) {
					Size size;
					//using (Stream fileStream = File.OpenRead(inputFile)) {
					using (var image = System.Drawing.Image.FromFile(inputFile)) {
						size = image.Size;
					}
					//}
					size = new Size((int) (size.Width * scale), (int) (size.Height * scale));
					if (size.Width > 1000 || size.Height > 1000) {
						await asciifyMsg.DeleteAsync();
						if (asciify.Delete) {
							try {
								if (asciify.AttachmentMessage != null)
									await asciify.AttachmentMessage.DeleteAsync();
								else
									await asciify.Message.DeleteAsync();
							}
							catch { }
						}
						await context.Channel.SendMessageAsync($"Scaled dimensions cannot be larger than 1000x1000. The scaled image size is {size.Width}x{size.Height}");
						return;
					}
					//IMessage scaledMsg = null;
					if (scale != 1) {
						await asciifyMsg.ModifyAsync((p) => {
							p.Content = $"{asciifyStr}\n**Scaled dimensions:** {size.Width}x{size.Height}";
						});
					}
					//	scaledMsg = await context.Channel.SendMessageAsync($"**Scaled image dimensions:** {size.Width}x{size.Height}");
					//}
					ProcessStartInfo start = new ProcessStartInfo() {
						FileName = "AsciiArtist.exe",
						Arguments = $"-asciify {smoothness} {scale} lab \"{inputFile}\" \"{outputFile}\"",
						CreateNoWindow = true,
					};
					Process asciiArtist = Process.Start(start);
					asciiArtist.WaitForExit();
					if (asciify.Delete) {
						try {
							if (asciify.AttachmentMessage != null)
								await asciify.AttachmentMessage.DeleteAsync();
							else
								await asciify.Message.DeleteAsync();
							asciify.AttachmentMessage = null;
							asciify.Message = null;
						}
						catch { }
					}
					//if (scaledMsg != null)
					//	await scaledMsg.DeleteAsync();
					await asciifyMsg.DeleteAsync();
					asciifyMsg = null;
					if (asciiArtist.ExitCode != 0 || !File.Exists(outputFile)) {
						await context.Channel.SendMessageAsync($"An error occurred while asciifying the image");
						return;
					}
					await context.Channel.SendFileAsync(outputFile, $"Asciify took: {watch.Elapsed.ToDHMSString()}");

					File.Delete(inputFile);
					File.Delete(outputFile);
				}
			}
			catch (IOException) {
				if (asciify.Delete) {
					try {
						if (asciify.AttachmentMessage != null)
							await asciify.AttachmentMessage.DeleteAsync();
						else
							await asciify.Message.DeleteAsync();
					}
					catch { }
				}
				if (asciifyMsg != null)
					await asciifyMsg.DeleteAsync();
				await context.Channel.SendMessageAsync($"An IO error occurred while asciifying the image");
			}
		}
	}
}
