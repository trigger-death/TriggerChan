using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class DiscordExtensions {

		public static string GetName(this IUser user, SocketGuild guild) {
			if (guild != null) {
				IGuildUser gUser;
				if (user is IGuildUser)
					gUser = (IGuildUser) user;
				else
					gUser = guild.GetUser(user.Id);
				return Format.Sanitize(gUser.Nickname ?? user.Username);
			}
			return Format.Sanitize(user.Username);
		}

		public static async Task<string> GetName(this IUser user, IGuild guild) {
			if (guild != null) {
				IGuildUser gUser;
				if (user is IGuildUser)
					gUser = (IGuildUser) user;
				else
					gUser = await guild.GetUserAsync(user.Id);
				return Format.Sanitize(gUser.Nickname ?? user.Username);
			}
			return Format.Sanitize(user.Username);
		}
	}
}
