using System;

namespace DiscordKeyGenerator {
	class Program {
		static void Main(string[] args) {
			string input = ReadTypeInput();
			bool validChoice = true;
			while (input != "exit" || !validChoice) {
				validChoice = true;
				switch (input) {
				case "key":
					GenerateEncryptionKey();
					break;
				case "token":
					GenerateDummyToken();
					break;
				case "guid":
					GenerateGuid();
					break;
				default:
					WriteError("Invalid Choice!");
					validChoice = false;
					Console.Write("> ");
					input = Console.ReadLine().ToLower();
					continue;
				}
				input = ReadTypeInput();
			}
		}
		static string ReadTypeInput() {
			Console.WriteLine("==== Discord Key Generator ====");
			Console.WriteLine("   key) Generate an encryption key");
			Console.WriteLine(" token) Generate a dummy Discord Token");
			Console.WriteLine("  guid) Generate a new 128 GUID");
			Console.WriteLine("  exit) Exit");
			Console.WriteLine();
			Console.Write("> ");
			return Console.ReadLine().ToLower();
		}

		static void WriteError(string line) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(line);
			Console.ResetColor();
		}
		static void WriteGenerated(string key) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(key);
			Console.ResetColor();
			Console.WriteLine();
			TextCopy.Clipboard.SetText(key);
			Console.WriteLine("Copied to clipboard!");
			Console.WriteLine();
		}

		static void GenerateDummyToken() {
			const string defaultCharset = "Aa1-_";
			Console.WriteLine($"Enter valid charset: (default is \"{defaultCharset}\")");

			bool validInput;
			do {
				validInput = true;
				Console.Write("> ");
				string charset = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(charset))
					charset = defaultCharset;
				string token = Token.GenerateDummy(charset);
				if (token == null) {
					WriteError("No valid characters!");
					validInput = false;
					continue;
				}
				WriteGenerated(token);
			} while (!validInput);
		}
		static void GenerateGuid() {
			WriteGenerated(Guid.NewGuid().ToString());
		}
		static void GenerateEncryptionKey() {
			const string defaultLength = "16";
			Console.WriteLine($"Enter key length in bytes: (default is {defaultLength})");

			bool validInput;
			do {
				validInput = true;
				Console.Write("> ");
				string lengthStr = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(lengthStr))
					lengthStr = defaultLength;
				if (!int.TryParse(lengthStr, out int length)) {
					WriteError("Not an integer!");
					validInput = false;
					continue;
				}
				else if (length < 1) {
					WriteError("Less than one!");
					validInput = false;
					continue;
				}
				else if (length > 256) {
					WriteError("Greater than 256!");
					validInput = false;
					continue;
				}
				string key = Encryption.GenerateBase64(length);
				WriteGenerated(key);
			} while (!validInput);
		}
	}
}
