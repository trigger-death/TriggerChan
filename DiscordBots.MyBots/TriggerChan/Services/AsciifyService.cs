using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.Asciify;
using TriggersTools.Asciify.Asciifying;
using TriggersTools.Asciify.Asciifying.Asciifiers;
using TriggersTools.Asciify.Asciifying.Fonts;
using TriggersTools.Asciify.Asciifying.Palettes;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Extensions;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class AsciifyTask {
		public ITextChannel Channel { get; set; }
		public IUser User { get; set; }
		public IMessage Message { get; set; }
		//public IMessage AttachmentMessage { get; set; }
		public bool Delete { get; set; }
		public IAttachment Attachment { get; set; }

		public bool Smooth { get; set; }
		public int Smoothness { get; set; }
		public float Scale { get; set; }

		public DateTime TimeStamp { get; set; }
		public Task Task { get; set; }
	}
	public class AsciifyService : TriggerService {

		#region Constants

		public const int MaxWidth = 1024;
		public const int MaxHeight = 1024;

		#endregion

		#region Fields

		/// <summary>
		/// Per guild asciify tasks.
		/// </summary>
		private readonly ConcurrentDictionary<ulong, AsciifyTask> asciifyTasks = new ConcurrentDictionary<ulong, AsciifyTask>();
		
		private readonly ConfigParserService configParser;

		#endregion

		#region Constructors

		public AsciifyService(TriggerServiceContainer services,
							  ConfigParserService configParser)
			: base(services)
		{
			this.configParser = configParser;
		}

		#endregion

		public async Task Asciify(SocketCommandContext context, AsciifyTask asciify, bool triggersTools) {

			AsciifyTask currentAsciify = asciifyTasks.GetOrAdd(context.Guild.Id, asciify);
			if (currentAsciify == asciify) {
				string ext = Path.GetExtension(asciify.Attachment.Filename).ToLower();
				if (ext != ".png" && ext != ".bmp" && ext != ".jpg") {
					await asciify.Channel.SendMessageAsync("Image must be a png, jpg, or bmp").ConfigureAwait(false);
					return;
				}
				lock (asciify) {
					asciify.Task = Task.Run(() => AsciifyTask(context, asciify, triggersTools))
						.ContinueWith((t) => {
							asciifyTasks.TryRemove(context.Guild.Id, out _);
						});
				}
			}
			else {
				IUser user = currentAsciify.User;
				string name = user.GetName(context.Guild, true);
				await context.Channel.SendMessageAsync($"{name} is currently asciifying an image. Please wait").ConfigureAwait(false);
			}
		}
		private async Task AsciifyTask(SocketCommandContext context, AsciifyTask asciify, bool triggersTools) {
			float scale = asciify.Scale;
			float scaleP = asciify.Scale * 100;
			bool smooth = asciify.Smooth;
			int smoothness = asciify.Smoothness;
			string filename = asciify.Attachment.Filename;
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smooth {(smooth ? "yes" : "no")}, Scale {scale:P}";
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smoothness {smoothness}, Scale {scale:P}";
			var embed = new EmbedBuilder {
				Title = $"{configParser.EmbedPrefix}Asciifying... <a:processing:507585439536906241>",
				Color = configParser.EmbedColor,
				Description = (triggersTools ?
							  $"__Smooth:__ {(smooth ? "yes" : "no")}\n" :
							  $"__Smoothness:__ {smoothness}\n") +
							  $"__Scale:__ {scale:P1}",
			};
			//var asciifyMsg = await context.Channel.SendMessageAsync(asciifyStr).ConfigureAwait(false);
			var asciifyMsg = await context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
			try {
				using (var typing = context.Channel.EnterTypingState()) {
					Stopwatch watch = Stopwatch.StartNew();
					IAttachment attach = context.Message.Attachments.First();
					string url = attach.Url;
					string ext = Path.GetExtension(filename);
					string inputFile = BotResources.GetAsciifyIn(context.Guild.Id, ext);
					string outputFile = BotResources.GetAsciifyOut(context.Guild.Id);


					using (HttpClient client = new HttpClient())
						File.WriteAllBytes(inputFile, await client.GetByteArrayAsync(url).ConfigureAwait(false));
					//using (Stream stream = await client.GetStreamAsync(url)) {
					Size size;
					//using (Stream fileStream = File.OpenRead(inputFile)) {
					using (var bitmap = (Bitmap) System.Drawing.Image.FromFile(inputFile)) {
						size = bitmap.Size;
						//}
						Size oldSize = size;
						size = new Size((int) (size.Width * scale), (int) (size.Height * scale));
						//embed.Description += $"\n__Image Dimensions:__ {oldSize.Width}x{oldSize.Height}\n" +
						//					 $"__Scaled Dimensions:__ {size.Width}x{size.Height}";
						embed.Description = (triggersTools ?
											$"__Smooth:__ {(smooth ? "yes" : "no")}\n" :
											$"__Smoothness:__ {smoothness}\n") +
											$"__Scale:__ {scale:P1}\n" +
											$"__Image Dimensions:__ {oldSize.Width}x{oldSize.Height}";
						if (scale != 1)
							embed.Description = $"\n__Scaled Dimensions:__ {size.Width}x{size.Height}";
						if (size.Width > MaxWidth || size.Height > MaxHeight) {
							//await asciifyMsg.DeleteAsync().ConfigureAwait(false);
							/*if (asciify.Delete) {
								try {
									//if (asciify.AttachmentMessage != null)
									//	await asciify.AttachmentMessage.DeleteAsync().ConfigureAwait(false);
									//else
										await asciify.Message.DeleteAsync().ConfigureAwait(false);
								} catch { }
							}*/
							scale = Math.Min((float) MaxWidth / oldSize.Width, (float) MaxHeight / oldSize.Height);
							scaleP = scale * 100;
							//await context.Channel.SendMessageAsync($"Scaled dimensions cannot be larger than {MaxWidth}x{MaxHeight}.\n" +
							//	$"The scaled image size is {size.Width}x{size.Height}\n" +
							//	$"Maximum scale is {maxScale:P0}").ConfigureAwait(false);
							size = new Size((int) (oldSize.Width * scale), (int) (oldSize.Height * scale));
							/*embed.Description +=
								$"\nScaled dimensions cannot be larger than {MaxWidth}x{MaxHeight}\n" +
								$"__Reduced Scale:__ {scale:P}\n" +
								$"__Reduced Dimensions:__ {size.Width}x{size.Height}";*/
							asciify.Scale = scale;
							embed.Description = (triggersTools ?
												$"__Smooth:__ {(smooth ? "yes" : "no")}\n" :
												$"__Smoothness:__ {smoothness}\n") +
												$"__Reduced Scale:__ {scale:P1}\n" +
												$"__Image Dimensions:__ {oldSize.Width}x{oldSize.Height}";
							if (scale != 1)
								embed.Description += $"\n__Reduced Dimensions:__ {size.Width}x{size.Height}";
							/*embed.Fields.Clear();
							embed.AddField("Progress", "");
							await asciifyMsg.ModifyAsync(p => p.Embed = embed.Build()).ConfigureAwait(false);*/
							//return;
						}
						//IMessage scaledMsg = null;
						/*if (scale != 1) {
							//embed.Description += $"__Image Dimensions:__ {oldSize.Width}x{oldSize.Height}\n" +
							//					 $"__Scaled Dimensions:__ {size.Width}x{size.Height}";
							await asciifyMsg.ModifyAsync(p => p.Embed = embed.Build()).ConfigureAwait(false);
						}*/
						//	scaledMsg = await context.Channel.SendMessageAsync($"**Scaled image dimensions:** {size.Width}x{size.Height}");
						//}
						await UpdateProgress(context, watch, 0, asciifyMsg, embed).ConfigureAwait(false);
						if (triggersTools) {
							await AsciifyTriggersTools(context, watch, bitmap, asciifyMsg, embed, asciify).ConfigureAwait(false);
						}
						else {
							await AsciifyAsciiArtist(context, watch, asciifyMsg, embed, asciify).ConfigureAwait(false);
						}
					}
					try {
						File.Delete(inputFile);
						if (!triggersTools)
							File.Delete(outputFile);
					} catch { }
				}
			} catch (Exception) {
				if (asciify.Delete) {
					try {
						//if (asciify.AttachmentMessage != null)
						//	await asciify.AttachmentMessage.DeleteAsync().ConfigureAwait(false);
						//else
							await asciify.Message.DeleteAsync().ConfigureAwait(false);
					} catch { }
				}
				if (asciifyMsg != null)
					await asciifyMsg.DeleteAsync().ConfigureAwait(false);
				await context.Channel.SendMessageAsync($"An error occurred while asciifying the image").ConfigureAwait(false);
			}
		}

		private async Task UpdateProgress(SocketCommandContext context, Stopwatch watch, double progress, IUserMessage asciifyMsg, EmbedBuilder embed) {
			embed.Fields.Clear();
			embed.AddField("Progress", $"{progress:P0} - {(int) (watch.ElapsedMilliseconds / 1000)} seconds");
			await asciifyMsg.ModifyAsync(p => p.Embed = embed.Build()).ConfigureAwait(false);
		}

		private async Task AsciifyAsciiArtist(SocketCommandContext context, Stopwatch watch, IUserMessage asciifyMsg, EmbedBuilder embed, AsciifyTask asciify) {
			string ext = Path.GetExtension(asciify.Attachment.Filename);
			string inputFile = BotResources.GetAsciifyIn(context.Guild.Id, ext);
			string outputFile = BotResources.GetAsciifyOut(context.Guild.Id);
			ProcessStartInfo start = new ProcessStartInfo() {
				FileName = "AsciiArtist.exe",
				//Arguments = $"-asciify {(smooth ? 2 : 1)} {scale} lab \"{inputFile}\" \"{outputFile}\"",
				Arguments = $"-asciify {asciify.Smoothness} {asciify.Scale} lab \"{inputFile}\" \"{outputFile}\"",
				CreateNoWindow = true,
				UseShellExecute = false,
				//UseShellExecute = true,
			};
			await UpdateProgress(context, watch, 0, asciifyMsg, embed).ConfigureAwait(false);
			Process asciiArtist = Process.Start(start);
			asciiArtist.WaitForExit();
			if (asciify.Delete) {
				try {
					//if (asciify.AttachmentMessage != null)
					//	await asciify.AttachmentMessage.DeleteAsync().ConfigureAwait(false);
					//else
					await asciify.Message.DeleteAsync().ConfigureAwait(false);
					//asciify.AttachmentMessage = null;
					asciify.Message = null;
				} catch { }
			}
			//if (scaledMsg != null)
			//	await scaledMsg.DeleteAsync();
			//await asciifyMsg.DeleteAsync().ConfigureAwait(false);
			//asciifyMsg = null;
			if (asciiArtist.ExitCode != 0 || !File.Exists(outputFile)) {
				await asciifyMsg.ModifyAsync(p => p.Content = $"An error occurred while asciifying the image").ConfigureAwait(false);
				//await context.Channel.SendMessageAsync($"An error occurred while asciifying the image").ConfigureAwait(false);
				try {
					File.Delete(inputFile);
				} catch { }
				return;
			}

			await asciifyMsg.DeleteAsync().ConfigureAwait(false);
			await context.Channel.SendFileAsync(outputFile, $"{configParser.EmbedPrefix}Asciify took: {watch.Elapsed.ToDHMSString()}").ConfigureAwait(false);
		}

		private async Task AsciifyTriggersTools(SocketCommandContext context, Stopwatch watch, Bitmap bitmap, IUserMessage asciifyMsg, EmbedBuilder embed, AsciifyTask asciify) {
			string filename = asciify.Attachment.Filename;
			string ext = Path.GetExtension(filename);
			string inputFile = BotResources.GetAsciifyIn(context.Guild.Id, ext);
			string outputFile = BotResources.GetAsciifyOut(context.Guild.Id);
			IAsciifier asciifier;
			if (asciify.Smooth)
				asciifier = Asciifier.SectionedColor;
			else
				asciifier = Asciifier.DotColor;
			//asciifier.Palette = AsciifyPalette.WindowsConsole;
			asciifier.MaxDegreeOfParallelism = 1;
			//asciifier.CharacterSet = CharacterSets.Bitmap;
			//asciifier.

			// Text
			ICharacterSet charset = CharacterSets.Bitmap;
			IAsciifyFont font = new BitmapAsciifyFont("Terminal", new Size(8, 12), charset);

			// Color
			System.Drawing.Color background = System.Drawing.Color.Black;
			AsciifyPalette palette = AsciifyPalette.WindowsConsole;
			TimeSpan lastReport = TimeSpan.Zero;
			void callback(double progress) {
				TimeSpan ellapsed = watch.Elapsed;
				if (ellapsed - lastReport >= TimeSpan.FromSeconds(10)) {
					lastReport = ellapsed;
					UpdateProgress(context, watch, progress, asciifyMsg, embed).GetAwaiter().GetResult();
				}
			}

			// Asciify
			asciifier.Initialize(font, charset, palette);
			using (Bitmap prepared = asciifier.PrepareImage(bitmap, asciify.Scale, background))
			using (Bitmap output = asciifier.AsciifyImage(prepared, callback)) {
				if (asciify.Delete) {
					try {
						//if (asciify.AttachmentMessage != null)
						//	await asciify.AttachmentMessage.DeleteAsync().ConfigureAwait(false);
						//else
						await asciify.Message.DeleteAsync().ConfigureAwait(false);
						//asciify.AttachmentMessage = null;
						asciify.Message = null;
					} catch { }
				}
				await asciifyMsg.DeleteAsync().ConfigureAwait(false);
				embed.Fields.Clear();
				embed.Title = $"{configParser.EmbedPrefix}Asciify took: {watch.Elapsed.ToDHMSString()} <:ascii:508384924936699914>";
				await context.Channel.SendBitmapAsync(output, filename, embed: embed.Build()).ConfigureAwait(false);
			}
		}
	}
}
