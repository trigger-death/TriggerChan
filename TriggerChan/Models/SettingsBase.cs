using Discord.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TriggersTools.DiscordBots.TriggerChan.Util;

namespace TriggersTools.DiscordBots.TriggerChan.Models {
	public enum SettingsType {
		Guild,
		GuildChannel,
		Group,
		DM,
	}

	public enum ListMode {
		Blacklist,
		Whitelist,
	}

	public abstract class SettingsBase {
		[NotMapped]
		public ulong Id { get; set; }
		
		public string Prefix { get; set; }

		[NotMapped]
		public abstract SettingsType Type { get; }

		[NotMapped]
		public bool IsGuild { get => Type == SettingsType.Guild; }
		[NotMapped]
		public bool IsGuildChannel { get => Type == SettingsType.GuildChannel; }
		[NotMapped]
		public bool IsChannel { get => Type != SettingsType.Guild; }
		[NotMapped]
		public bool IsGroup { get => Type == SettingsType.Group; }
		[NotMapped]
		public bool IsDM { get => Type == SettingsType.DM; }

		public TimeSpan TalkBackCooldown { get; set; }// = TimeSpan.FromMinutes(5);

		//public ListMode ListMode { get; set; }

		public string LockedCommandsStr { get; set; }// = "|purge|";
		public string LockedModulesStr { get; set; }
		
		public bool TalkBack { get; set; }

		public int PinReactCount { get; set; }

		public bool IsCommandLocked(CommandInfo cmd) {
			bool locked = LockedCommandsStr.IndexOf($"|{cmd.Aliases.First()}|",
				StringComparison.OrdinalIgnoreCase) != -1;
			if (!locked)
				return IsModuleLocked(cmd.Module);
			return locked;
		}

		public bool IsCommandLockedDirectly(CommandInfo cmd) {
			return LockedCommandsStr.IndexOf($"|{cmd.Aliases.First()}|",
				StringComparison.OrdinalIgnoreCase) != -1;
		}

		public bool IsModuleLocked(ModuleInfo mod) {
			mod = mod.RootModule();
			return LockedModulesStr.IndexOf($"|{mod.Name}|",
				StringComparison.OrdinalIgnoreCase) != -1;
		}

		public bool IsModuleLocked(string name) {
			return LockedModulesStr.IndexOf($"|{name}|",
				StringComparison.OrdinalIgnoreCase) != -1;
		}

		public bool LockCommand(string command) {
			int index = LockedCommandsStr.IndexOf($"|{command}|",
				StringComparison.OrdinalIgnoreCase);
			if (index == -1) {
				LockedCommandsStr += command + "|";
			}
			return index == -1;
		}

		public bool UnlockCommand(string command) {
			int index = LockedCommandsStr.IndexOf($"|{command}|",
				StringComparison.OrdinalIgnoreCase);
			if (index != -1) {
				LockedCommandsStr = LockedCommandsStr.Remove(index + 1, command.Length + 1);
			}
			return index != -1;
		}

		public bool LockModule(string module) {
			int index = LockedModulesStr.IndexOf($"|{module}|",
				StringComparison.OrdinalIgnoreCase);
			if (index == -1) {
				LockedModulesStr += module + "|";
			}
			return index == -1;
		}

		public bool UnlockModule(string module) {
			int index = LockedModulesStr.IndexOf($"|{module}|",
				StringComparison.OrdinalIgnoreCase);
			if (index != -1) {
				LockedModulesStr = LockedModulesStr.Remove(index + 1, module.Length + 1);
			}
			return index != -1;
		}
	}

	public abstract class LocalSettingsBase {
		public ulong Id { get; set; }

		public abstract SettingsType Type { get; }
		
		public bool IsGuild { get => Type == SettingsType.Guild; }
		public bool IsGuildChannel { get => Type == SettingsType.GuildChannel; }
		public bool IsChannel { get => Type != SettingsType.Guild; }
		public bool IsGroup { get => Type == SettingsType.Group; }
		public bool IsDM { get => Type == SettingsType.DM; }

		//public Task MessageCountTask { get; set; }
		//public CancellationTokenSource MessageCountToken { get; set; }
		//public bool MessageCountStatusRequest { get; set; }
	}
}
