using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Modules;
using TriggersTools.DiscordBots.TriggerChan.Extensions;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	public class TriggerModule : DiscordBotModule {

		public TriggerModule(TriggerServiceContainer services) : base(services) { }

		/*public Task<IUserMessage> ReplyBitmapAsync(Bitmap bitmap, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			return Context.Channel.SendBitmapAsync(bitmap, filename, text, isTTS, embed, options);
		}
		public Task<IUserMessage> ReplyBitmapAsync(Bitmap bitmap, ImageFormat format, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			return Context.Channel.SendBitmapAsync(bitmap, format, filename, text, isTTS, embed, options);
		}*/
	}
}
