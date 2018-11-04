using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Text;
using TriggersTools.DiscordBots.Database;
using TriggersTools.DiscordBots.Database.Model;

namespace TriggersTools.DiscordBots.SpoilerBot.Model {
	/// <summary>
	/// The base database model for a guild emote.
	/// </summary>
	public class GuildEmote : IDbGuildEmote {
		/// <summary>
		/// The snowflake Id key.
		/// </summary>
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; set; }
		/// <summary>
		/// The End User (guild) Data snowflake Id key.
		/// </summary>
		[Required]
		public ulong EndUserGuildDataId { get; set; }
		/// <summary>
		/// The guild snowflake Id.
		/// </summary>
		[NotMapped]
		public ulong GuildId {
			get => EndUserGuildDataId;
			set => EndUserGuildDataId = value;
		}

		/// <summary>
		/// Checks if the user has asked this information to be deleted.
		/// </summary>
		/// <param name="euds">The info to that the user requested to be deleted.</param>
		/// <returns>True if the data should be deleted.</returns>
		public bool ShouldKeep(EndUserDataContents euds, EndUserDataType type) {
			return !euds.Contains("emotes");
		}


		/// <summary>
		/// Gets the entity type of this Discord model.
		/// </summary>
		[NotMapped]
		public EntityType Type => EntityType.GuildEmote;

		/*/// <summary>
		/// Gets the name of the emote.
		/// </summary>
		[EncryptedString]
		public string Name { get; set; }
		/// <summary>
		/// Gets the time the emote was created at.
		/// </summary>
		public DateTime CreatedAt { get; set; }*/
		/// <summary>
		/// Gets the data for the emote image.
		/// </summary>
		[EncryptedBytes]
		public byte[] ImageBytes { get; set; }

		/// <summary>
		/// Gets the image of the emote.
		/// </summary>
		public Bitmap GetImage() {
			if (ImageBytes == null || ImageBytes.Length == 0)
				return null;
			using (var stream = new MemoryStream(ImageBytes))
				return (Bitmap) Bitmap.FromStream(stream);
		}

		/// <summary>
		/// The guild assocaited with this role.
		/// </summary>
		[ForeignKey(nameof(GuildId))]
		public Guild Guild { get; set; }
	}
}
