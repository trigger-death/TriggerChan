using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Context {
	public class BotDatabaseContext : DbContext {
		public BotDatabaseContext(DbContextOptions<BotDatabaseContext> options) : base(options) { }
		public BotDatabaseContext()
			: base(new DbContextOptionsBuilder<BotDatabaseContext>()
				  .UseSqlite("Data Source=trigger_chan.db").Options)
		{
		}

		public DbSet<Guild> Guilds { get; set; }
		public DbSet<GuildChannel> GuildChannels { get; set; }
		public DbSet<Group> Groups { get; set; }
		public DbSet<DM> DMs { get; set; }

		public DbSet<GuildUser> GuildUsers { get; set; }

		public DbSet<Spoiler> Spoilers { get; set; }
		public DbSet<SpoiledUser> SpoiledUsers { get; set; }

		public void UpdateSettings(SettingsBase settings) {
			if (settings is Guild guild)
				Guilds.Update(guild);
			else if (settings is GuildChannel gChannel)
				GuildChannels.Update(gChannel);
			else if (settings is Group group)
				Groups.Update(group);
			else if (settings is DM dm)
				DMs.Update(dm);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<GuildUser>()
				.HasKey(gu => new { gu.UserId, gu.GuildId });
		}
	}

	public class BotDatabaseContextFactory : IDesignTimeDbContextFactory<BotDatabaseContext> {
		public BotDatabaseContext CreateDbContext(string[] args) {
			var optionsBuilder = new DbContextOptionsBuilder<BotDatabaseContext>().UseSqlite("Data Source = trigger_chan.db");
			return new BotDatabaseContext(optionsBuilder.Options);
		}
	}
}
