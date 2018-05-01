using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Services {
	public class LoggingService : BotServiceBase {

		private string logDirectory { get; }
		private string logFile => Path.Combine(logDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

		private static string errorDirectory { get; }
		private static string errorFile => Path.Combine(errorDirectory, $"{DateTime.UtcNow.ToString("yyyy-MM-dd")}.txt");

		static LoggingService() {
			errorDirectory = Path.Combine(AppContext.BaseDirectory, "Errors");
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		}

		private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
			WriteException(e.Exception);
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
			WriteException(e.ExceptionObject);
		}

		private static void WriteException(object ex) {
			try {
				Console.WriteLine($"Caught: { ex.GetType().Name}\n{ex.ToString()}");
				string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{ex.GetType().Name}] {ex.ToString()}";
				File.AppendAllText(errorFile, logText);
			}
			catch (IOException) { }
		}

		// DiscordSocketClient and CommandService are injected automatically from the IServiceProvider
		public LoggingService(DiscordSocketClient client, CommandService commands) {
			logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");

			client.Log += OnLogAsync;
			commands.Log += OnLogAsync;
		}

		protected override void OnInitialized(ServiceProvider services) {
			base.OnInitialized(services);
		}

		private Task OnLogAsync(LogMessage msg) {
			if (!Directory.Exists(logDirectory))     // Create the log directory if it doesn't exist
				Directory.CreateDirectory(logDirectory);
			if (!File.Exists(logFile))               // Create today's log file if it doesn't exist
				File.Create(logFile).Dispose();

			string logText = $"{DateTime.UtcNow.ToString("hh:mm:ss")} [{msg.Severity}] {msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
			try {
				File.AppendAllText(logFile, logText + "\n");     // Write the log text to a file
			}
			catch (IOException) { }
			return Console.Out.WriteLineAsync(logText);       // Write the log text to the console
		}
	}
}
