using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetDeploy.Utils {
	public static class ColorConsole {
		#region Write Normal

		public static void WriteLine() => Console.WriteLine();
		public static void WriteLine(string line) => Console.WriteLine(line);
		public static void WriteLine(object value) => Console.WriteLine(value);
		public static void Write(string line) => Console.Write(line);
		public static void Write(object value) => Console.Write(value);
		public static string ReadLine() => Console.ReadLine();

		#endregion

		#region Write Color

		public static void WriteLine(ConsoleColor foreground, string line) {
			Console.ForegroundColor = foreground;
			Console.WriteLine(line);
			Console.ResetColor();
		}
		public static void WriteLine(ConsoleColor foreground, object value) {
			Console.ForegroundColor = foreground;
			Console.WriteLine(value);
			Console.ResetColor();
		}
		public static void Write(ConsoleColor foreground, string line) {
			Console.ForegroundColor = foreground;
			Console.Write(line);
			Console.ResetColor();
		}
		public static void Write(ConsoleColor foreground, object value) {
			Console.ForegroundColor = foreground;
			Console.Write(value);
			Console.ResetColor();
		}

		#endregion

		#region Write Special

		public static void ErrorMessageLine(Exception ex) => WriteLine(ConsoleColor.Red, ex.Message);
		public static void ErrorMessage(Exception ex) => Write(ConsoleColor.Red, ex.Message);
		public static void ErrorLine(object value) => WriteLine(ConsoleColor.Red, value);
		public static void Error(object value) => Write(ConsoleColor.Red, value);
		public static void WarningLine(object value) => WriteLine(ConsoleColor.Yellow, value);
		public static void Warning(object value) => Write(ConsoleColor.Yellow, value);
		public static void InfoLine(object value) => WriteLine(ConsoleColor.Cyan, value);
		public static void Info(object value) => Write(ConsoleColor.Cyan, value);

		#endregion

		#region Write Watermark

		public static void Watermark(object value) {
			int left = Console.CursorLeft;
			int top = Console.CursorTop;
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.Write(value);
			Console.ResetColor();
			Console.CursorLeft = left;
			Console.CursorTop = top;
		}

		#endregion

		#region Beep

		public static void Beep() => Console.Beep();
		public static void Beep(int frequency, int duration) => Console.Beep(frequency, duration);

		#endregion

		#region Clear

		public static void Clear() => Console.Clear();

		#endregion
	}
}
