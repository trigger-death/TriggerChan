using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TriggersTools.DiscordBots.Extensions {
	/// <summary>
	/// Extension methods for the <see cref="IMessageChannel"/> interface.
	/// </summary>
	public static class MessageChannelExtensions {
		
		/// <summary>
		/// Sends a file to this message channel with an optional caption.
		/// </summary>
		/// <param name="data">The binary data of the file to be sent.</param>
		/// <param name="filename">The name of the attachment.</param>
		/// <param name="text">The message to be sent.</param>
		/// <param name="isTTS">Whether the message should be read aloud by Discord or not.</param>
		/// <param name="embed">The <see cref="EmbedType.Rich"/> <see cref="Embed"/> to be send.</param>
		/// <param name="options">The options to be used when sending the request.</param>
		/// <returns>
		/// A task that represents an asynchronous send operation for delivering the message. The task result
		/// contains the sent message.
		/// </returns>
		/// <remarks>
		/// This method sends a file as if you are uploading an attachment directly from your Discord client.
		/// If you wish to upload an image and have it embedded in a <see cref="EmbedType.Rich"/> embed, you
		/// may upload the file and refer to the file with "attachment://filename.ext" in the
		/// <see cref="EmbedBuilder.ImageUrl"/>. See the example section for its usage.
		/// </remarks>
		public static async Task<IUserMessage> SendFileAsync(this IMessageChannel channel, byte[] data, string filename, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			using (MemoryStream stream = new MemoryStream(data))
				return await channel.SendFileAsync(stream, filename, text, isTTS, embed, options).ConfigureAwait(false);
		}

		/// <summary>
		/// Checks if the bot has the base level of permissions needed to respond in the channel.
		/// </summary>
		/// <param name="channel">The channel to check the permissions of.</param>
		/// <param name="client">The guild client used to get the current user.</param>
		/// <returns>True if the bot can respond.</returns>
		public static async Task<bool> CanRespondAsync(this IMessageChannel channel, DiscordSocketClient client) {
			if (channel is IGuildChannel guildChannel) {
				IGuildUser guildUser = await guildChannel.Guild.GetCurrentUserAsync().ConfigureAwait(false);
				ChannelPermissions permissions = guildUser.GetPermissions(guildChannel);
				return permissions.SendMessages && permissions.AddReactions && permissions.UseExternalEmojis;
			}
			return true;
		}
	}
}
