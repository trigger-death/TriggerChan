using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDeploy {
	public class AsciiImage {
		public struct Pixel {
			public static readonly Encoding ConsoleEncoding =
				Encoding.GetEncoding("Windows-1252");

			public char Character;
			public ConsoleColor Foreground;
			public ConsoleColor Background;

			public Pixel(char character, ConsoleColor foreground, ConsoleColor background) {
				Character = character;
				Foreground = foreground;
				Background = background;
			}

			public Pixel(byte character, byte color) {
				Character = ConsoleEncoding.GetChar(character);
				Foreground = (ConsoleColor) (color & 0xF);
				Background = (ConsoleColor) ((color >> 4) & 0xF);
			}
		}

		public Pixel[,] Pixels { get; private set; }

		public int Width => Pixels.GetLength(0);
		public int Height => Pixels.GetLength(1);

		public static AsciiImage FromStream(Stream stream) {
			AsciiImage image = new AsciiImage();
			BinaryReader reader = new BinaryReader(stream);
			stream.Position += 8;
			int width = reader.ReadInt16();
			int height = reader.ReadInt16();
			stream.Position += 4;
			image.Pixels = new Pixel[width, height];
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					image.Pixels[x, y] = new Pixel(
						reader.ReadByte(), reader.ReadByte());
					stream.Position += 2;
				}
			}

			return image;
		}

		public void Draw(int left) {
			Console.ResetColor();

			// Make sure we start on a brand-spanking-new line
			if (Console.CursorLeft > 0)
				Console.WriteLine();

			// Optimized by writing to the console as few times as possible
			int bufferWidth = Console.BufferWidth - 2;
			//string currentLine = "";
			StringBuilder currentLine = new StringBuilder();
			if (left > 0)
				currentLine.Append(' ', left);
			for (int y = 0; y < Height; y++) {
				for (int x = 0; x < Width && x < bufferWidth; x++) {
					if (Pixels[x, y].Foreground != Console.ForegroundColor ||
						Pixels[x, y].Background != Console.BackgroundColor)
					{
						// We've reached a new color, gotta write the current line
						// (Check to make sure this isn't the first pixel
						if (currentLine.Length > 0) {
							Console.Write(currentLine);
							currentLine.Clear();
						}
						// Set the new console color for the next line
						Console.ForegroundColor = Pixels[x, y].Foreground;
						Console.BackgroundColor = Pixels[x, y].Background;
					}
					currentLine.Append(Pixels[x, y].Character);
				}

				if (currentLine.Length > 0) {
					Console.Write(currentLine);
					currentLine.Clear();
				}

				// Keep drawing the current string onto the next line
				// (If we don't automatically go to the next line due to wrapping)
				//if ((Console.CursorLeft + currentLine.Length) % bufferWidth != 0) {
				Console.ResetColor();
				Console.ForegroundColor = Console.BackgroundColor;
				Console.Write('.');
				currentLine.AppendLine();
				if (left > 0 && y + 1 < Height)
					currentLine.Append(' ', left);
				//}
			}
			// Write the remaining line
			//if (currentLine.Length > 0)
			//	Console.Write(currentLine);

			Console.ResetColor();
		}
	}
	internal static partial class EncodingExtensions {
		/// <summary>
		/// Designed for use with ASCII encoding to easily get a single char from a single byte.
		/// </summary>
		public static char GetChar(this Encoding encoding, byte singleByte) {
			return encoding.GetChars(new byte[] { singleByte })[0];
		}
	}
}
