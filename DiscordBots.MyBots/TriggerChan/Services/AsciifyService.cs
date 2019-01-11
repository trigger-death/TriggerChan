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
using TriggersTools.DiscordBots.Extensions;
using TriggersTools.DiscordBots.Services;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using TriggersTools.DiscordBots.Utils;
using Image = System.Drawing.Image;
using Color = System.Drawing.Color;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class AsciifyTask {
		public ITextChannel Channel { get; set; }
		public IUser User { get; set; }
		public IMessage Message { get; set; }
		//public IMessage AttachmentMessage { get; set; }
		public bool Delete { get; set; }
		public IAttachment Attachment { get; set; }

		public bool Sectioned { get; set; }
		public int Smoothness { get; set; }
		public double Scale { get; set; }

		public DateTime TimeStamp { get; set; }
		public Task Task { get; set; }
	}
	public class AsciifyService : TriggerService {
		#region Constants

		public const int MaxWidth = 2048;
		public const int MaxHeight = 2048;

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

		public async Task Asciify(SocketCommandContext context, AsciifyTask asciify) {

			AsciifyTask currentAsciify = asciifyTasks.GetOrAdd(context.Guild.Id, asciify);
			if (currentAsciify == asciify) {
				string ext = Path.GetExtension(asciify.Attachment.Filename).ToLower();
				if (ext != ".png" && ext != ".bmp" && ext != ".jpg") {
					await asciify.Channel.SendMessageAsync("Image must be a `png`, `jpg`, or `bmp`").ConfigureAwait(false);
					return;
				}
				lock (asciify) {
					asciify.Task = Task.Run(() => AsciifyTask(context, asciify))
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
		private async Task AsciifyTask(SocketCommandContext context, AsciifyTask asciify) {
			Stopwatch watch = Stopwatch.StartNew();
			double scale = asciify.Scale;
			bool smooth = asciify.Sectioned;
			string filename = asciify.Attachment.Filename;
			IAttachment attach = context.Message.Attachments.First();
			string url = attach.Url;
			string ext = Path.GetExtension(filename);
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smooth {(smooth ? "yes" : "no")}, Scale {scale:P}";
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smoothness {smoothness}, Scale {scale:P}";
			var embed = new EmbedBuilder {
				Title = $"{configParser.EmbedPrefix}Asciifying... <a:processing:526524731487158283>",
				Color = configParser.EmbedColor,
				Description = $"Algorithm: **{(smooth ? "Sectioned" : "Dot")}**\n" +
							  $"Scale: **{scale:P1}**",
			};
			IAsciifier asciifier = null;

			async Task updateProgress(MessageUpdater updater) {
				embed.Fields.Clear();
				//embed.AddField("Progress", $"{(asciifier?.Progress ?? 0d):P0} - {watch.Elapsed.ToDHMSString()}");
				embed.AddField("Progress", $"{(asciifier?.Progress ?? 0d):P0} - {watch.Elapsed.ToDHMSString()}");
				await updater.UpdateAsync(embed: embed.Build()).ConfigureAwait(false);
			}

			//var asciifyMsg = await context.Channel.SendMessageAsync(asciifyStr).ConfigureAwait(false);
			var asciifyMsg = await context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
			MessageUpdater mu = null;
			try {
				using (var typing = context.Channel.EnterTypingState())
				using (mu = MessageUpdater.Create(asciifyMsg, updateProgress))
				using (HttpClient client = new HttpClient())
				using (var stream = await client.GetStreamAsync(url).ConfigureAwait(false))
				using (var bitmap = (Bitmap) Image.FromStream(stream)) {
					Size size = bitmap.Size;
					Size oldSize = size;
					size = new Size((int) (size.Width * scale), (int) (size.Height * scale));
					embed.Description = $"Algorithm: **{(smooth ? "Sectioned" : "Dot")}**\n" +
										$"Scale: **{scale:P1}**\n" +
										$"Image Dimensions: **{oldSize.Width}**x**{oldSize.Height}**";
					if (scale != 1)
						embed.Description += $"\nScaled Dimensions: **{size.Width}**x**{size.Height}**";
					if (size.Width > MaxWidth || size.Height > MaxHeight) {
						scale = Math.Min((float) MaxWidth / oldSize.Width, (float) MaxHeight / oldSize.Height);
						size = new Size((int) (oldSize.Width * scale), (int) (oldSize.Height * scale));
						asciify.Scale = scale;
						embed.Description = $"Algorithm: **{(smooth ? "Sectioned" : "Dot")}**\n" +
											$"Reduced Scale: **{scale:P1}**\n" +
											$"Image Dimensions: **{oldSize.Width}**x**{oldSize.Height}**";
						if (scale != 1)
							embed.Description += $"\nReduced Dimensions: **{size.Width}**x**{size.Height}**";
					}
					await updateProgress(mu).ConfigureAwait(false);
					if (asciify.Sectioned)
						asciifier = Asciifier.SectionedColor;
					else
						asciifier = Asciifier.DotColor;
					asciifier.MaxDegreeOfParallelism = 1;

					// Text
					ICharacterSet charset = CharacterSets.Bitmap;
					IAsciifyFont font = new BitmapAsciifyFont("Terminal", new Size(8, 12), charset);

					// Color
					Color background = Color.Black;
					AsciifyPalette palette = AsciifyPalette.WindowsConsole;
					TimeSpan lastReport = TimeSpan.Zero;

					// Asciify
					mu.Start();
					asciifier.Initialize(font, charset, palette);
					using (Bitmap prepared = asciifier.PrepareImage(bitmap, asciify.Scale, background))
					using (Bitmap output = asciifier.AsciifyImage(prepared)) {
						mu.Stop();
						if (asciify.Delete) {
							try {
								try {
									await asciify.Message.DeleteAsync().ConfigureAwait(false);
								} catch { }
								asciify.Message = null;
							} catch { }
						}
						try {
							await mu.Message.DeleteAsync().ConfigureAwait(false);
						} catch { }
						embed.Fields.Clear();
						embed.Title = $"{configParser.EmbedPrefix}Asciify took: {watch.Elapsed.ToDHMSString()} <:ascii:526524712130576405>";
						await context.Channel.SendBitmapAsync(output, filename, embed: embed.Build()).ConfigureAwait(false);
					}
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
				if (mu?.Message != null) {
					try {
						await mu.Message.DeleteAsync().ConfigureAwait(false);
					} catch { }
				}
				await context.Channel.SendMessageAsync($"An error occurred while asciifying the image").ConfigureAwait(false);
			}
		}
		/*private async Task AsciifyTask(SocketCommandContext context, AsciifyTask asciify, bool triggersTools) {
			float scale = asciify.Scale;
			bool smooth = asciify.Smooth;
			int smoothness = asciify.Smoothness;
			string filename = asciify.Attachment.Filename;
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smooth {(smooth ? "yes" : "no")}, Scale {scale:P}";
			//string asciifyStr = $"**Asciifying:** `{filename}` with Smoothness {smoothness}, Scale {scale:P}";
			var embed = new EmbedBuilder {
				Title = $"{configParser.EmbedPrefix}Asciifying... <a:processing:526524731487158283>",
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
					string inputFile = TriggerResources.GetAsciifyIn(context.Guild.Id, ext);
					string outputFile = TriggerResources.GetAsciifyOut(context.Guild.Id);


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
							scale = Math.Min((float) MaxWidth / oldSize.Width, (float) MaxHeight / oldSize.Height);
							//await context.Channel.SendMessageAsync($"Scaled dimensions cannot be larger than {MaxWidth}x{MaxHeight}.\n" +
							//	$"The scaled image size is {size.Width}x{size.Height}\n" +
							//	$"Maximum scale is {maxScale:P0}").ConfigureAwait(false);
							size = new Size((int) (oldSize.Width * scale), (int) (oldSize.Height * scale));
							asciify.Scale = scale;
							embed.Description = (triggersTools ?
												$"__Smooth:__ {(smooth ? "yes" : "no")}\n" :
												$"__Smoothness:__ {smoothness}\n") +
												$"__Reduced Scale:__ {scale:P1}\n" +
												$"__Image Dimensions:__ {oldSize.Width}x{oldSize.Height}";
							if (scale != 1)
								embed.Description += $"\n__Reduced Dimensions:__ {size.Width}x{size.Height}";
							//return;
						}
						//IMessage scaledMsg = null;
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
		*/
		/*private async Task UpdateProgress(SocketCommandContext context, Stopwatch watch, double progress, IUserMessage asciifyMsg, EmbedBuilder embed) {
			embed.Fields.Clear();
			embed.AddField("Progress", $"{progress:P0} - {watch.Elapsed.ToDHMSString()}");
			await asciifyMsg.ModifyAsync(p => p.Embed = embed.Build()).ConfigureAwait(false);
		}*/

		/*private async Task AsciifyAsciiArtist(SocketCommandContext context, Stopwatch watch, IUserMessage asciifyMsg, EmbedBuilder embed, AsciifyTask asciify) {
			string ext = Path.GetExtension(asciify.Attachment.Filename);
			string inputFile = TriggerResources.GetAsciifyIn(context.Guild.Id, ext);
			string outputFile = TriggerResources.GetAsciifyOut(context.Guild.Id);
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
		*/
		/*private async Task AsciifyTriggersTools(SocketCommandContext context, Stopwatch watch, Bitmap bitmap, IUserMessage asciifyMsg, EmbedBuilder embed, AsciifyTask asciify) {
			string filename = asciify.Attachment.Filename;
			string ext = Path.GetExtension(filename);
			string inputFile = TriggerResources.GetAsciifyIn(context.Guild.Id, ext);
			string outputFile = TriggerResources.GetAsciifyOut(context.Guild.Id);
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
			Color background = Color.Black;
			AsciifyPalette palette = AsciifyPalette.WindowsConsole;
			//TimeSpan lastReport = TimeSpan.Zero;
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
			using (Bitmap output = asciifier.AsciifyImage(prepared)) {
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
				embed.Title = $"{configParser.EmbedPrefix}Asciify took: {watch.Elapsed.ToDHMSString()} <:ascii:526524712130576405>";
				await context.Channel.SendBitmapAsync(output, filename, embed: embed.Build()).ConfigureAwait(false);
			}
		}*/
	}
}
