/*
 * Code from Youtube Extractor
 * Credits to: Flagbug
 * Link: https://github.com/flagbug/YoutubeExtractor
 */
using System;
using System.Collections.Generic;

namespace Maya.Music.Youtube {
	public class YouTubeInfo : SongDownloadInfo {
		internal static IEnumerable<YouTubeInfo> Defaults = new List<YouTubeInfo> {
            new YouTubeInfo(5, VideoType.Flash, 240, AudioType.Mp3, 64, AdaptiveType.None),
			new YouTubeInfo(6, VideoType.Flash, 270, AudioType.Mp3, 64, AdaptiveType.None),
			new YouTubeInfo(13, VideoType.Mobile, 0, AudioType.Aac, 0, AdaptiveType.None),
			new YouTubeInfo(17, VideoType.Mobile, 144, AudioType.Aac, 24, AdaptiveType.None),
			new YouTubeInfo(18, VideoType.Mp4, 360, AudioType.Aac, 96, AdaptiveType.None),
			new YouTubeInfo(22, VideoType.Mp4, 720, AudioType.Aac, 192, AdaptiveType.None),
			new YouTubeInfo(34, VideoType.Flash, 360, AudioType.Aac, 128, AdaptiveType.None),
			new YouTubeInfo(35, VideoType.Flash, 480, AudioType.Aac, 128, AdaptiveType.None),
			new YouTubeInfo(36, VideoType.Mobile, 240, AudioType.Aac, 38, AdaptiveType.None),
			new YouTubeInfo(37, VideoType.Mp4, 1080, AudioType.Aac, 192, AdaptiveType.None),
			new YouTubeInfo(38, VideoType.Mp4, 3072, AudioType.Aac, 192, AdaptiveType.None),
			new YouTubeInfo(43, VideoType.WebM, 360, AudioType.Vorbis, 128, AdaptiveType.None),
			new YouTubeInfo(44, VideoType.WebM, 480, AudioType.Vorbis, 128, AdaptiveType.None),
			new YouTubeInfo(45, VideoType.WebM, 720, AudioType.Vorbis, 192, AdaptiveType.None),
			new YouTubeInfo(46, VideoType.WebM, 1080, AudioType.Vorbis, 192, AdaptiveType.None),

            /* Adaptive (aka DASH) - Audio */
            /*new YouTubeInfo(139, VideoType.Mp4, 0, AudioType.Aac, 48, AdaptiveType.Audio),
			new YouTubeInfo(140, VideoType.Mp4, 0, AudioType.Aac, 128, AdaptiveType.Audio),
			new YouTubeInfo(141, VideoType.Mp4, 0, AudioType.Aac, 256, AdaptiveType.Audio),
			new YouTubeInfo(171, VideoType.WebM, 0, AudioType.Vorbis, 128, AdaptiveType.Audio),
			new YouTubeInfo(172, VideoType.WebM, 0, AudioType.Vorbis, 192, AdaptiveType.Audio),*/
		};

		internal YouTubeInfo(int formatCode)
			: this(formatCode, VideoType.Unknown, 0, AudioType.Unknown, 0, AdaptiveType.None) { }

		internal YouTubeInfo(YouTubeInfo info)
			: this(info.FormatCode, info.VideoType, info.Resolution, info.AudioType, info.AudioBitrate, info.AdaptiveType) { }

		private YouTubeInfo(int formatCode, VideoType videoType, int resolution, AudioType audioType, int audioBitrate, AdaptiveType adaptiveType) {
			this.FormatCode = formatCode;
			this.VideoType = videoType;
			this.Resolution = resolution;
			this.AudioType = audioType;
			this.AudioBitrate = audioBitrate;
			this.AdaptiveType = adaptiveType;
		}

		/// <summary>
		/// Gets an enum indicating whether the format is adaptive or not.
		/// </summary>
		/// <value>
		/// <c>AdaptiveType.Audio</c> or <c>AdaptiveType.Video</c> if the format is adaptive;
		/// otherwise, <c>AdaptiveType.None</c>.
		/// </value>
		public AdaptiveType AdaptiveType { get; private set; }

		/// <summary>
		/// The approximate audio bitrate in kbit/s.
		/// </summary>
		/// <value>The approximate audio bitrate in kbit/s, or 0 if the bitrate is unknown.</value>
		public int AudioBitrate { get; private set; }

		/// <summary>
		/// Gets the audio extension.
		/// </summary>
		/// <value>The audio extension, or <c>null</c> if the audio extension is unknown.</value>
		public string AudioExtension {
			get {
				switch (this.AudioType) {
				case AudioType.Aac:
					return ".aac";

				case AudioType.Mp3:
					return ".mp3";

				case AudioType.Vorbis:
					return ".ogg";
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the audio type (encoding).
		/// </summary>
		public AudioType AudioType { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the audio of this video can be extracted by YoutubeExtractor.
		/// </summary>
		/// <value>
		/// <c>true</c> if the audio of this video can be extracted by YoutubeExtractor; otherwise, <c>false</c>.
		/// </value>
		public bool CanExtractAudio {
			get { return this.VideoType == VideoType.Flash; }
		}

		/// <summary>
		/// Gets the format code, that is used by YouTube internally to differentiate between
		/// quality profiles.
		/// </summary>
		public int FormatCode { get; private set; }
		
		/// <summary>
		/// Gets a value indicating whether this video info requires a signature decryption before
		/// the download URL can be used.
		///
		/// This can be achieved with the <see cref="YouTubeUrlResolver.DecryptDownloadUrl"/>
		/// </summary>
		public bool RequiresDecryption { get; internal set; }

		/// <summary>
		/// Gets the resolution of the video.
		/// </summary>
		/// <value>The resolution of the video, or 0 if the resolution is unkown.</value>
		public int Resolution { get; private set; }

		/// <summary>
		/// Gets the video extension.
		/// </summary>
		/// <value>The video extension, or <c>null</c> if the video extension is unknown.</value>
		public override string Extension {
			get {
				switch (this.VideoType) {
				case VideoType.Mp4:
					return ".mp4";

				case VideoType.Mobile:
					return ".3gp";

				case VideoType.Flash:
					return ".flv";

				case VideoType.WebM:
					return ".webm";
				}

				return null;
			}
		}

		/// <summary>
		/// Gets the video type (container).
		/// </summary>
		public VideoType VideoType { get; private set; }

		/// <summary>
		/// We use this in the <see cref="YouTubeUrlResolver.DecryptDownloadUrl" /> method to
		/// decrypt the signature
		/// </summary>
		/// <returns></returns>
		internal string HtmlPlayerVersion { get; set; }

		public override string ToString() {
			return string.Format("Full Title: {0}, Type: {1}, Resolution: {2}p", this.Title + this.Extension, this.VideoType, this.Resolution);
		}

		public override void ResolveUrl() {
			if (RequiresDecryption)
				YouTubeUrlResolver.DecryptDownloadUrl(this);
		}
	}
}
