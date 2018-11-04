using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TriggersTools.DiscordBots.TriggerChan.Audio {
	partial class AudioQueue {

		public Task UpdateVoiceState(SocketVoiceState oldState, SocketVoiceState newState) {
			//lock (VoiceLock) {
			if (newState.VoiceChannel != null)
				return VoiceSwitch(newState.VoiceChannel);
			else if (oldState.VoiceChannel != null)
				return VoiceLeave();
			else
				return VoiceJoin(newState.VoiceChannel);
			//}
		}

		private async Task VoiceJoin(IVoiceChannel channel) {
			Channel = channel;
			AudioClient = await channel.ConnectAsync().ConfigureAwait(false);
		}
		private async Task VoiceSwitch(IVoiceChannel channel) {
			Cancel?.Cancel();
			CancelComplete.WaitOne();
			Channel = channel;
			AudioClient = await channel.ConnectAsync().ConfigureAwait(false);
		}
		private async Task VoiceLeave() {

		}

		private async Task StartPlayback() {
			//FFMpeg
		}


		private async Task ContinuePlayback() {

		}

		private void CreateFFMpegStream() {
			string path = CurrentSong.FilePath;
			FFMpeg?.Kill();
			FFMpeg?.Dispose();
			FFMpeg = Process.Start(new ProcessStartInfo {
				FileName = @"ffmpeg",
				Arguments = $"-hide_banner -xerror -loglevel quiet -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				//Arguments = $"-hide_banner -i \"{path}\" -sample_fmt s16 -ar 48000 -ac 2 -acodec libopus -b:a 192k -vbr on -compression_level 10 -map 0:a -f data pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}
	}
}
