using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace TriggersTools.DiscordBots.Extensions {
	public static class EmbedExtensions {
		
		public static EmbedBuilder PagifyField(this EmbedBuilder embed, string name, IEnumerable<string> items) {
			StringBuilder str = new StringBuilder();
			int pageIndex = 1;
			void AddField(bool forcePage) {
				string page = string.Empty;
				if (pageIndex != 1 || forcePage)
					page = $" `(pg. {pageIndex++})`";
				embed.AddField($"{name}{page}", str.ToString());
			}
			foreach (string item in items) {
				if (item.Length > 1024)
					throw new InvalidOperationException("Item length is too large for embed field!");
				if (str.Length + item.Length > 1024) {
					AddField(true);
					str.Clear();
				}
				if (str.Length > 0)
					str.AppendLine();
				str.Append(item);
			}
			if (str.Length > 0)
				AddField(false);
			return embed;
		}
	}
}
