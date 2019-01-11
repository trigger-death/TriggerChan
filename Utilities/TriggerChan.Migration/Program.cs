using System;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.TriggerChan.Database;

namespace TriggerChan.Migration {
	class Program {
		static void Main(string[] args) {
			DbContextExTransfer.Transfer(new TriggerDbContextFactory(), "npgsql_triggy", "aws_npgsql_triggy");
		}
	}
}
