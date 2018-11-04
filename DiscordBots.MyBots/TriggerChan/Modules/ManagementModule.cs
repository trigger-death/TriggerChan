using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TriggersTools.DiscordBots.Commands;
using TriggersTools.DiscordBots.TriggerChan.Services;

namespace TriggersTools.DiscordBots.TriggerChan.Modules {
	[Name("Management")]
	[Summary("Commands for managing users and the server")]
	[RequiresContext(ContextType.Guild)]
	[IsLockable(true)]
	public class ManagementModule : TriggerModule {

		private readonly RolesService roles;

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="ManagementModule"/>.
		/// </summary>
		public ManagementModule(TriggerServiceContainer services,
								RolesService roles)
			: base(services)
		{
			this.roles = roles;
		}

		#endregion

		[Name("iam <roleName>")]
		[Command("iam"), Alias("role assign")]
		[Usage("<roleName>")]
		[Summary("Assign a public role to yourself")]
		[Example("beta tester", "Gives you the *Beta Tester* role if it is public")]
		public Task<RuntimeResult> IAm([Remainder] IRole role) {
			return roles.AddRole(Context, role);
		}

		[Name("iamnot <roleName>")]
		[Command("iamnot"), Alias("role unassign")]
		[Usage("<roleName>")]
		[Summary("Assign a public role to yourself")]
		[Example("anime recommender", "Removes your *Anime Recommender* role if it is public")]
		public Task<RuntimeResult> IAmNot([Remainder] IRole role) {
			return roles.RemoveRole(Context, role);
		}

		[Name("role")]
		[Group("role")]
		[ModulePriority(1)]
		//[Usage("[<add|remove> <roleName>]")]
		[Usage("add|remove <roleName>")]
		[Summary("Manage public roles and list what roles are available")]
		[RequiresUserPermission(GuildPermission.ManageRoles)]
		public class RolesGroup : TriggerModule {

			private readonly RolesService roles;

			#region Constructors

			/// <summary>
			/// Constructs the <see cref="RolesGroup"/>.
			/// </summary>
			public RolesGroup(TriggerServiceContainer services,
							  RolesService roles)
				: base(services)
			{
				this.roles = roles;
			}

			#endregion

			/*[Name("roles")]
			[Command("")]
			[Example("List all public roles")]
			public Task ListRoles() {
				return roles.ListRoles(Context);
			}*/

			[Name("role add <roleName>")]
			[Command("add")]
			[Example("beta tester", "Will make the *Beta Tester* role public to users")]
			public Task<RuntimeResult> MakeRolePublic([Remainder] IRole role) {
				return roles.MakeRolePublic(Context, role);
			}

			[Name("role remove <roleName>")]
			[Command("remove")]
			[Example("beta tester", "Will make the *Beta Tester* role private to users")]
			public Task<RuntimeResult> MakeRolePrivate([Remainder] IRole role) {
				return roles.MakeRolePrivate(Context, role);
			}
		}

		[Name("roles")]
		[Command("roles")]
		[Summary("List all public roles that you can assign or unassign with `iam` and `iamnot`")]
		[Priority(0)]
		public Task ListRoles() {
			return roles.ListRoles(Context);
		}
	}
}
