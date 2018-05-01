using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using TriggersTools.DiscordBots.TriggerChan.Info;
using TriggersTools.DiscordBots.TriggerChan.Modules;
using TriggersTools.DiscordBots.TriggerChan.Services;
using TriggersTools.DiscordBots.TriggerChan.Models;

namespace TriggersTools.DiscordBots.TriggerChan.Util {
	public static class CommandInfoExtensions {
		
		public static bool IsDuplicateFunctionality(this CommandInfo cmd) {
			var attr = cmd.Attributes.OfType<IsDuplicateAttribute>().FirstOrDefault();
			return (attr?.DifferentFunctionality ?? false);
		}

		public static bool IsDuplicate(this CommandInfo cmd) {
			return cmd.Attributes.OfType<IsDuplicateAttribute>().Any();
		}

		/*public static bool HasUsage(this CommandInfo cmd) {
			return cmd.Attributes.OfType<UsageAttribute>().Any();
		}

		public static string GetUsage(this CommandInfo cmd) {
			var attr = cmd.Attributes.OfType<UsageAttribute>().FirstOrDefault();
			return attr?.Usage;
		}*/

		public static bool IsLocked(this CommandInfo cmd, SettingsBase settings) {
			return settings.IsCommandLocked(cmd);
		}

		public static bool IsLockedDirectly(this CommandInfo cmd, SettingsBase settings) {
			return settings.IsCommandLockedDirectly(cmd);
		}

		public static bool IsLockable(this CommandInfo cmd) {
			return (cmd.GetLockable()?.IsLockable ?? false);
		}

		public static bool IsLockable(this ModuleInfo mod) {
			return (mod.GetLockable()?.IsLockable ?? false);
		}

		public static bool HasLockable(this CommandInfo cmd) {
			return GetLockable(cmd) != null;
		}

		public static bool HasLockable(this ModuleInfo mod) {
			return GetLockable(mod) != null;
		}

		public static bool IsGroupLocked(this CommandGroup group, SettingsBase settings) {
			return settings.IsModuleLocked(group.Name);
		}

		public static IsLockableAttribute GetLockable(this CommandInfo cmd) {
			var attr = cmd.Preconditions.OfType<IsLockableAttribute>().FirstOrDefault();
			if (attr == null)
				attr = cmd.Module.GetLockable();
			return attr;
		}

		public static IsLockableAttribute GetLockable(this ModuleInfo mod) {
			mod = mod.RootModule();
			return mod.Preconditions.OfType<IsLockableAttribute>().FirstOrDefault();
		}

		public static ModuleInfo RootModule(this CommandInfo cmd) {
			ModuleInfo mod = cmd.Module;
			while (mod.Parent != null) {
				mod = mod.Parent;
			}
			return mod;
		}

		public static ModuleInfo RootModule(this ModuleInfo mod) {
			while (mod.Parent != null) {
				mod = mod.Parent;
			}
			return mod;
		}

		public static bool HasParameters(this CommandInfo cmd) {
			return cmd.Attributes.OfType<ParametersAttribute>().Any();
		}

		public static string GetParameters(this CommandInfo cmd) {
			var attr = cmd.Attributes.OfType<ParametersAttribute>().FirstOrDefault();
			return attr?.Parameters;
		}
	}
}
