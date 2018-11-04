using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Discord;
using TriggersTools.DiscordBots.Services;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public enum Note {
		NewStaff = -3,
		Bar = -2,
		Rest = -1,

		LT = 0,
		Up = 1,
		RT = 2,
		Left = 3,
		Right = 4,
		Start = 5,
		Down = 6,
		B = 7,
		A = 8,

		// These are here for ordering purposes
	}
	public class Staff : IEnumerable<Note> {
		public Note[] Notes { get; }

		public Staff() {
			Notes = new Note[0];
		}
		public Staff(IEnumerable<Note> notes) {
			Notes = notes.ToArray();
		}

		public int Length => Notes.Length;
		public Note this[int index] {
			get => Notes[index];
			set => Notes[index] = value;
		}

		public IEnumerator<Note> GetEnumerator() => ((IEnumerable<Note>) Notes).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool Equals(Staff staff, bool allowWhitespace) {
			Note[] a = this.Notes;
			Note[] b = staff.Notes;
			if (allowWhitespace) {
				a = RemoveWhitespace(a);
				b = RemoveWhitespace(b);
			}
			if (a.Length != b.Length)
				return false;
			for (int i = 0; i < a.Length; i++) {
				if (a[i] != b[i])
					return false;
			}
			return true;
		}
		public bool StartsWith(Staff staff, bool allowWhitespace) {
			Note[] a = this.Notes;
			Note[] b = staff.Notes;
			if (allowWhitespace) {
				a = RemoveWhitespace(a);
				b = RemoveWhitespace(b);
			}
			if (a.Length < b.Length)
				return false;
			for (int i = 0; i < b.Length; i++) {
				if (a[i] != b[i])
					return false;
			}
			return true;
		}

		/*public override bool Equals(object obj) {
			if (obj is Staff staff)
				return Equals(staff, false);
			return base.Equals(obj);
		}*/

		private Note[] RemoveWhitespace(Note[] notes) {
			return notes.Where(n => n != Note.Rest).ToArray();
		}

		public override string ToString() {
			return $"{string.Join(" ", Notes)}";
		}
	}
	public struct NoteAlias {

		public string Alias { get; }
		public bool IsEmoji { get; }
		public bool IsPrimary { get; }

		public NoteAlias(string alias, bool isEmoji, bool isPrimary) {
			Alias = alias;
			IsEmoji = isEmoji;
			IsPrimary = isPrimary;
		}

		public override string ToString() => (IsEmoji ? Alias : $"`{Alias}`");
	}

	public class NoteAliases {

		public NoteAliases(Note note, IEmote emote, IEnumerable<NoteAlias> aliases) {
			Note = note;
			Emote = emote;
			if (note == Note.NewStaff)
				Aliases = new[] { new NoteAlias("*newline*", true, true) };
			else
				Aliases = aliases.ToArray();
		}

		public Note Note { get; }
		public IEmote Emote { get; }
		public NoteAlias[] Aliases { get; }
		public int Length => Aliases.Length;
		public NoteAlias this[int index] {
			get => Aliases[index];
			set => Aliases[index] = value;
		}
	}
	public class Song {
		public string Title { get; }
		public Staff Staff { get; }

		public Song(string title, Staff staff) {
			Title = title;
			Staff = staff;
		}

		public override string ToString() {
			return $"{Title}: {Staff}";
		}
	}

	public class OcarinaService : DiscordBotService {

		#region Constants

		private const string ScarecrowsSongFile = "Scarecrow.ocarina";

		#endregion


		#region Fields

		private readonly Dictionary<string, Note> notesMap = new Dictionary<string, Note>(StringComparer.InvariantCultureIgnoreCase);
		private readonly Dictionary<Note, List<NoteAlias>> noteAliases = new Dictionary<Note, List<NoteAlias>>();
		private readonly Dictionary<Note, IEmote> noteEmotes = new Dictionary<Note, IEmote>();

		public Song ScarecrowsSong { get; private set; }

		private readonly List<Song> songs = new List<Song>();

		private readonly object fileLock = new object();

		private readonly Staff[] lossSong;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs the <see cref="OcarinaService"/>.
		/// </summary>
		public OcarinaService(DiscordBotServiceContainer services) : base(services) {
			//public OcarinaService() {
			// Notes: ▲ ▶ ◀ ▼ A ❤️ ♥️
			// ▶️◀️🔼🔽
			staffImage = LoadBitmap("StaffTransparent");
			staffBarImage = LoadBitmap("StaffBar");
			staffExpansionImage = LoadBitmap("StaffExpansion");
			sandstoneImages[0] = LoadBitmap("Sandstone");
			sandstoneImages[1] = LoadBitmap("SandstoneFlipped");
			titleBarImage = LoadBitmap("TitleBar");
			foreach (Note note in Notes) {
				noteImages.Add(note, LoadBitmap(note.ToString()));
				noteAliases.Add(note, new List<NoteAlias>());
			}
			noteAliases.Add(Note.Rest, new List<NoteAlias>());
			noteAliases.Add(Note.Bar, new List<NoteAlias>());
			noteAliases.Add(Note.NewStaff, new List<NoteAlias>());
			
			AddNote("U", Note.Up);
			AddNote("Up", Note.Up);
			AddNote("^", Note.Up);
			AddNote("▲", Note.Up, primary: true); // Unicode
			AddNote("⬆️", Note.Up, emoji: true, noSelector: true); // Arrow Emoji
			AddNote("🔼", Note.Up, emoji: true); // Triangle Emoji

			AddNote("L", Note.Left);
			AddNote("Left", Note.Left);
			AddNote("<", Note.Left);
			AddNote("◄", Note.Left, primary: true); // Unicode
			AddNote("⬅️", Note.Left, emoji: true, noSelector: true); // Arrow Emoji
			AddNote("◀️", Note.Left, emoji: true, noSelector: true); // Triangle Emoji

			AddNote("R", Note.Right);
			AddNote("Right", Note.Right);
			AddNote(">", Note.Right);
			AddNote("►", Note.Right, primary: true); // Unicode
			AddNote("➡️", Note.Right, emoji: true, noSelector: true); // Arrow Emoji
			AddNote("▶️", Note.Right, emoji: true, noSelector: true); // Triangle Emoji

			AddNote("D", Note.Down);
			AddNote("Down", Note.Down);
			AddNote("v", Note.Down);
			AddNote("▼", Note.Down, primary: true); // Unicode
			AddNote("⬇️", Note.Down, emoji: true, noSelector: true); // Arrow Emoji
			AddNote("🔽", Note.Down, emoji: true); // Triangle Emoji

			AddNote("A", Note.A, primary: true);
			AddNote("🅰️", Note.A, emoji: true, noSelector: true); // Blood Type
			AddNote("🇦", Note.A, emoji: true); // Regional Indicator

			AddNote("B", Note.B, primary: true);
			AddNote("🅱️", Note.B, emoji: true, noSelector: true); // Blood Type
			AddNote("🇧", Note.B, emoji: true); // Regional Indicator

			AddNote("S", Note.Start, primary: true);
			AddNote("Start", Note.Start);
			AddNote("🔘", Note.Start, emoji: true); // Radio Button
			AddNote("🇸", Note.Start, emoji: true); // Regional Indicator

			AddNote("LT", Note.LT, primary: true);
			AddNote("LB", Note.LT);
			AddNote("⏪", Note.LT, emoji: true); // Rewind
			AddNote("⏮️", Note.LT, emoji: true, noSelector: true); // Previous Track
			AddNote("🇱", Note.LT, emoji: true); // Regional Indicator

			AddNote("RT", Note.RT, primary: true);
			AddNote("RB", Note.RT);
			AddNote("⏩", Note.RT, emoji: true); // Fast-Forward
			AddNote("⏭️", Note.RT, emoji: true, noSelector: true); // Next Track
			AddNote("🇷", Note.RT, emoji: true); // Regional Indicator

			AddNote("#", Note.Rest); // (Easily accessible on iOS keyboard without shift)
			AddNote("3", Note.Rest);
			AddNote("Rest", Note.Rest);
			AddNote("-", Note.Rest, primary: true);
			AddNote("_", Note.Rest);
			AddNote("💤", Note.Rest, emoji: true); // ZZZ

			AddNote("|", Note.Bar, primary: true);
			AddNote("Bar", Note.Bar);

			AddNote("\n", Note.NewStaff);
			AddNote("\r\n", Note.NewStaff, primary: true);

			noteEmotes.Add(Note.LT, Emote.Parse("<:OcarinaLT:504055910969245697>"));
			noteEmotes.Add(Note.RT, Emote.Parse("<:OcarinaRT:504055920003645482>"));
			noteEmotes.Add(Note.Up, Emote.Parse("<:OcarinaUp:501908088635392001>"));
			noteEmotes.Add(Note.Left, Emote.Parse("<:OcarinaLeft:501908096713621515>"));
			noteEmotes.Add(Note.Right, Emote.Parse("<:OcarinaRight:501908104737456128>"));
			noteEmotes.Add(Note.Down, Emote.Parse("<:OcarinaDown:501908112249192449>"));
			noteEmotes.Add(Note.A, Emote.Parse("<:OcarinaA:501908119933157387>"));
			noteEmotes.Add(Note.B, Emote.Parse("<:OcarinaB:504055892191346708>"));
			noteEmotes.Add(Note.Start, Emote.Parse("<:OcarinaStart:504055901116694539>"));
			noteEmotes.Add(Note.Rest, null);
			noteEmotes.Add(Note.Bar, null);
			noteEmotes.Add(Note.NewStaff, null);

			// Normal
			// Ocarina of Time
			AddSong("Zelda's Lullaby", "< ^ > < ^ >");
			AddSong("Epona's Lullaby", "^ < > ^ < >");
			AddSong("Saria's Lullaby", "v > < v > <");
			AddSong("Sun's Lullaby", "> v ^ > v ^");
			AddSong("Song of Time", "> A v > A v");
			AddSong("Song of Storms", "A v ^ A v ^");
			// Majora's Mask
			AddSong("Song of Healing", "< > v < > v");
			AddSong("Song of Soaring", "v < ^ v < ^");
			AddSong("Song of Double Time", "> > A A v v");
			AddSong("Inverted Song of Time", "v A > v A >");
			
			// Special
			// Ocarina of Time
			AddSong("Minuet of Forest", "A ^ < > < >");
			AddSong("Bolero of Fire", "v A v A > v > v");
			AddSong("Serenade of Water", "A v > > ^");
			AddSong("Requiem of Spirit", "A v A > v A");
			AddSong("Nocturne of Shadow", "< > > A < > v");
			AddSong("Prelude of Light", "^ > ^ > < ^");
			// Majora's Mask
			AddSong("Sonata of Awakening", "^ < ^ < A > A");
			AddSong("Goron Lullaby", "A > < A > < > A");
			AddSong("New Wave Bossa Nova", "< ^ < > v < >");
			AddSong("Elegy of Emptiness", "> < > v > ^ <");
			AddSong("Oath to Order", "> v A v > ^");


			// Other
			AddSong("Skyward Sword", "> ^ < > ^ <");

			AddSong("Konami Code", "^ ^ v v < > < > B A Start");

			UpdateScarecrowsSong();

			lossSong = ParseStaffs("^#|^^\n^^|^>");
		}
		const char VariationSelector = unchecked((char) 0xFE0F);
		private void AddNote(string alias, Note note, bool emoji = false, bool noSelector = false, bool primary = false) {
			// Fix pain in the ass Discord emoji differences with less emoji control characters
			if (emoji) {
				string aliasSelector;
				if (alias.EndsWith(new string(VariationSelector, 1))) {
					aliasSelector = alias.Substring(0, alias.Length - 1);
					if (noSelector) {
						string aliasSwap = alias;
						alias = aliasSelector;
						aliasSelector = aliasSwap;
					}
				}
				else {
					aliasSelector = alias + VariationSelector;
				}
				if (!notesMap.ContainsKey(aliasSelector))
					notesMap.Add(aliasSelector, note);
			}
			/*if (emoji && alias.Length > 2) {
				alias = alias.Substring(0, 2);
			}*/

			if (!notesMap.TryGetValue(alias, out Note otherNote))
				notesMap.Add(alias, note);
			else if (otherNote != note)
				throw new InvalidOperationException($"{alias} is already assigned to note {otherNote}");
			noteAliases[note].Add(new NoteAlias(alias, emoji, primary));
		}
		private Bitmap LoadBitmap(string name) {
			return (Bitmap) Image.FromFile(Path.Combine(AppContext.BaseDirectory, @"Resources\Images\Ocarina", $"{name}.png"));
		}
		private void AddSong(string title, string notesStr) {
			songs.Add(new Song(title, ParseStaffs(notesStr)[0]));
		}
		private void UpdateScarecrowsSong() {
			if (!File.Exists(ScarecrowsSongFile)) {
				GenerateScarecrowsSong();
			}
			else if (ScarecrowsSong == null) {
				LoadScarecrowsSong();
			}
			else {
				DateTime lastWriteDate = File.GetLastWriteTimeUtc(ScarecrowsSongFile).Date;
				if (DateTime.UtcNow.Date - lastWriteDate >= TimeSpan.FromDays(1)) {
					GenerateScarecrowsSong();
				}
			}
		}
		private void GenerateScarecrowsSong() {
			Staff staff;
			do {
				Random random = new Random();
				Note[] realNotes = RealNotes;
				Note[] notes = new Note[8];
				for (int i = 0; i < notes.Length; i++) {
					notes[i] = realNotes[random.Next(realNotes.Length)];
				}
				staff = new Staff(notes);
			} while (FindSongStartsWith(staff) != null); // Don't generate an existing song
			ScarecrowsSong = new Song("Scarecrow's Song", staff);
			File.WriteAllText(ScarecrowsSongFile, string.Join(" ", staff.Select(n => noteAliases[n].First(na => na.IsPrimary).Alias)));
		}
		private void LoadScarecrowsSong() {
			Random random = new Random();
			Note[] realNotes = RealNotes;
			Note[] notes = new Note[8];
			for (int i = 0; i < notes.Length; i++) {
				notes[i] = realNotes[random.Next(realNotes.Length)];
			}
			ScarecrowsSong = new Song("Scarecrow's Song", ParseStaffs(File.ReadAllText(ScarecrowsSongFile))[0]);
		}

		public Song FindSong(Staff staff) {
			return songs.Find(s => s.Staff.Equals(staff, true));
		}
		public Song FindSongStartsWith(Staff staff) {
			return songs.Find(s => s.Staff.StartsWith(staff, true));
		}

		#endregion

		#region Properties

		public Note[] RealNotes => new[] {
			Note.Up,
			Note.Left,
			Note.Right,
			Note.Down,
			Note.A,
		};

		public Note[] Notes => new[] {
			Note.LT,
			Note.Up,
			Note.RT,
			Note.Left,
			Note.Right,
			Note.Start,
			Note.Down,
			Note.B,
			Note.A,
		};

		public Note[] Special => new[] {
			Note.Rest,
			Note.Bar,
			Note.NewStaff,
		};

		public NoteAliases[] NoteAliases {
			get {
				return noteAliases
					.Where(n => n.Key >= 0)
					.Select(n => new NoteAliases(n.Key, noteEmotes[n.Key], n.Value))
					.OrderBy(n => n.Note)
					.ToArray();
			}
		}

		public NoteAliases[] SpecialAliases {
			get {
				return noteAliases
					.Where(n => n.Key < 0)
					.Select(n => new NoteAliases(n.Key, noteEmotes[n.Key], n.Value))
					.OrderByDescending(n => n.Note)
					.ToArray();
			}
		}

		#endregion
		
		public Note ParseNote(string s) {
			if (notesMap.TryGetValue(s, out Note note))
				return note;
			throw new FormatException(s);
		}
		public bool TryParseNote(string s, out Note note) {
			return notesMap.TryGetValue(s, out note);
		}

		private string SelectNextNote(string s, ref int index) {
			bool wordStarted = false;
			int startIndex = index;
			for (int i = index; i < s.Length; i++) {
				char c = s[i];
				// Treat these characters as whitespace too
				/*if (!wordStarted && (c == '\n' || c == '\r')) {
					index = i + 1;
					return s.Substring(i, index - i);
				}
				else */if (char.IsWhiteSpace(c) || c == '.' || c == ',') {
					bool newLine = (c == '\n' || c == '\r');
					if (wordStarted) {
						int newLineOffset = (newLine ? 0 : 1);
						index = i + newLineOffset;
						// Ignore the last whitespace character
						return s.Substring(startIndex, index - startIndex - newLineOffset);
					}
					else if (newLine) {
						index = i + 1;
						return s.Substring(i, index - i);
					}
					else {
						startIndex = i;
					}
				}
				else if (char.IsLetterOrDigit(c)) {
					wordStarted = true;
				}
				// Emoji Handling
				else if (c == 0xD83C || c == 0xD83D) {
					i++;
					if (s.Length > i) {
						if (s.Length > i + 1 && s[i + 1] == 0xFE0F)
							i++;
						index = i + 1;
						return s.Substring(startIndex, index - startIndex);
					}
					index = i;
					return s.Substring(startIndex, index - startIndex);
				}
				else {
					if (s.Length > i + 1 && s[i + 1] == 0xFE0F)
						i++;
					index = i + 1;
					return s.Substring(startIndex, index - startIndex);
				}
			}
			if (wordStarted) {
				index = s.Length;
				return s.Substring(startIndex, index - startIndex);
			}
			return null;
		}

		public Staff[] ParseStaffs(string s) {
			//string[] notes = s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			List<string> noteStrs = new List<string>();
			int index = 0;
			string noteStr = SelectNextNote(s, ref index);
			while (noteStr != null) {
				if (noteStr != "\n" && noteStr != "\r")
					noteStr = noteStr.Trim();
				noteStrs.Add(noteStr);
				noteStr = SelectNextNote(s, ref index);
			}
			List<Staff> staffs = new List<Staff>();
			List<Note> notes = new List<Note>();
			foreach (Note note in noteStrs.Select(n => ParseNote(n))) {
				if (note == Note.NewStaff) {
					if (notes.Count > 0) {
						staffs.Add(new Staff(notes));
						notes.Clear();
					}
				}
				else {
					notes.Add(note);
				}
			}
			if (notes.Any() || !staffs.Any())
				staffs.Add(new Staff(notes));
			return staffs.ToArray();
			//return notes.Select(n => ParseNote(n.Trim())).ToArray();
		}

		#region Dimensions

		private const int TitleOffsetX = 16;
		private const int TitleOffsetY = 14;
		private const int TitleSpacingY = 86;
		private const int TitleWidth = 668;
		private const int TitleHeight = 68;
		//private const int ImageHeight = 320;
		private const int SandstoneWidth = 700;
		private const int SandstoneHeight = 465;
		//private const int NoteOffsetY = 106;
		//private const int NoteOffsetX = 122;
		private const int BaseImageHeight = 234;
		private const int NoteOffsetY = 0;
		private const int NoteOffsetX = 106;
		private const int NoteSpacingY = 18;
		private const int NoteSpacingX = 55;
		private const int StaffWidth = 668;
		private const int StaffHeight = 194;
		private const int StaffOffsetX = 16;
		private const int StaffOffsetY = 20;
		private const int StaffLineSpacing = 12;

		#endregion

		#region Graphics

		private readonly Bitmap titleBarImage;
		private readonly Bitmap staffImage;
		private readonly Bitmap staffBarImage;
		private readonly Bitmap staffExpansionImage;
		private readonly Bitmap[] sandstoneImages = new Bitmap[2];
		private readonly Dictionary<Note, Bitmap> noteImages = new Dictionary<Note, Bitmap>();

		#endregion


		public byte[] DrawStaffsToBytes(string s, string title = null) {
			return DrawStaffsToBytes(ParseStaffs(s), title);
		}
		public byte[] DrawStaffsToBytes(Staff[] staffs, string title = null) {
			using (Bitmap bitmap = DrawStaffs(staffs, title))
			using (MemoryStream stream = new MemoryStream()) {
				bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
				return stream.ToArray();
			}
		}
		public Bitmap DrawStaffs(string s, string title = null) {
			return DrawStaffs(ParseStaffs(s), title);
		}
		public Bitmap DrawStaffs(Staff[] staffs, string title = null) {
			Song song = null;
			if (staffs.Length == 1) {
				UpdateScarecrowsSong();
				song = FindSong(staffs[0]);
				if (song == null && ScarecrowsSong.Staff.Equals(staffs[0], true))
					song = ScarecrowsSong;
			}
			else if (staffs.Length == 2 && lossSong[0].Equals(staffs[0], false) && lossSong[1].Equals(staffs[1], false)) {
				song = new Song("Loss", lossSong[0]);
			}
			title = title ?? song?.Title;
			bool hasTitle = title != null;
			int offsetY = (hasTitle ? TitleSpacingY : 0);
			int imageHeight = BaseImageHeight + offsetY;

			int maxNotes = staffs.Max(s => s.Length);
			int notesPastTen = Math.Max(0, maxNotes - 10);
			int staffWidth = StaffWidth + notesPastTen * NoteSpacingX;
			float scale = (float) StaffWidth / staffWidth;

			float scaledWidth = staffWidth * scale;
			float scaledHeight = StaffHeight * scale;
			float scaledSpacing = (StaffHeight + StaffLineSpacing) * scale;
			
			imageHeight += (int) Math.Ceiling((StaffHeight + StaffLineSpacing) * (staffs.Length - 1) * scale - (StaffHeight - scaledHeight));

			Bitmap bitmap = new Bitmap(SandstoneWidth, imageHeight);
			try {
				using (Graphics g = Graphics.FromImage(bitmap))
				using (Bitmap staffBitmap = new Bitmap(staffWidth, StaffHeight)) {
					g.PageUnit = GraphicsUnit.Pixel;
					g.InterpolationMode = InterpolationMode.NearestNeighbor;
					
					if (imageHeight > SandstoneHeight) {
						int y = 0;
						int flipped = 0;
						while (y < imageHeight) {
							g.DrawImageUnscaled(sandstoneImages[flipped], 0, y);
							y += SandstoneHeight;
							flipped = 1 - flipped;
						}
					}
					else {
						g.DrawImageUnscaled(sandstoneImages[0], 0, -(SandstoneHeight - imageHeight) / 2);
					}
					if (hasTitle) {
						using (Bitmap titleBitmap = new Bitmap(TitleWidth, TitleHeight)) {
							using (Graphics gt = Graphics.FromImage(titleBitmap))
							using (Font font = new Font("Gabriola", 38, FontStyle.Bold))  {
								gt.InterpolationMode = InterpolationMode.NearestNeighbor;
								gt.TextRenderingHint = TextRenderingHint.AntiAlias;
								gt.DrawImageUnscaled(titleBarImage, 0, 0);
								gt.DrawString($"You played {song.Title}", font, Brushes.White, TitleWidth / 2, 14,
									new StringFormat {
										Alignment = StringAlignment.Center,
										LineAlignment = StringAlignment.Near,
									});
							}
							g.DrawImageUnscaled(titleBitmap, TitleOffsetX, TitleOffsetY);
						}
					}
					for (int i = 0; i < staffs.Length; i++) {
						Staff staff = staffs[i];
						using (Graphics gs = Graphics.FromImage(staffBitmap)) {
							gs.PageUnit = GraphicsUnit.Pixel;
							gs.InterpolationMode = InterpolationMode.NearestNeighbor;
							gs.Clear(Color.Transparent);
							int index = 0;
							gs.DrawImageUnscaled(staffImage, 0, 0);
							for (int j = 0; j < notesPastTen; j++) {
								gs.DrawImageUnscaled(staffExpansionImage, StaffWidth + NoteSpacingX * j, 0);
							}
							//int noteCount = staff.Length;// Math.Min(notes.Length, 10);
							foreach (Note note in staff) {
								int x = NoteOffsetX + index * NoteSpacingX;
								if (note == Note.Bar) {
									gs.DrawImageUnscaled(staffBarImage, x, 0);
								}
								else if (note != Note.Rest) {
									Bitmap noteImage = noteImages[note];
									int y = NoteOffsetY + ((int) note * NoteSpacingY);
									gs.DrawImageUnscaled(noteImage, x, y);
								}
								index++;
							}
						}
						g.PageUnit = GraphicsUnit.Pixel;
						g.InterpolationMode = InterpolationMode.HighQualityBicubic;
						g.DrawImage(staffBitmap, StaffOffsetX, offsetY + StaffOffsetY + scaledSpacing * i, scaledWidth, scaledHeight);
					}
				}
				//OpenInMSPaint(bitmap);
			}
			catch {
				bitmap.Dispose();
				throw;
			}
			return bitmap;
		}

		#region IDisposable Implementation
		
		/// <summary>
		/// Disposes of the <see cref="OcarinaService"/>.
		/// </summary>
		public override void Dispose() {
			staffImage.Dispose();
			foreach (Bitmap note in noteImages.Values)
				note.Dispose();
		}

		#endregion
		/// <summary>This only works in Windows. :(</summary>
		private static void OpenInMSPaint(Bitmap bitmap) {
			bitmap.Save("image.png");
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				ProcessStartInfo start = new ProcessStartInfo {
					UseShellExecute = false,
					FileName = "mspaint",
					Arguments = "image.png",
				};
				Process.Start(start);
				Thread.Sleep(1000);
				try {
					File.Delete("image.png");
				}
				catch { }
			}
		}
	}
}
