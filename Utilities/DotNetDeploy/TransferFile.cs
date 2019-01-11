using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetDeploy.Utils;
using WinSCP;

namespace DotNetDeploy {
	public enum TransferType {
		None = 0,
		Remove,
		Update,
		Add,
		Skip,
		Ignore,
	}
	public class TransferFile : IComparable<TransferFile> {
		public TransferType Type { get; set; }
		public FileData Source { get; set; }
		public FileData Destination { get; set; }

		public bool IsDirectory => Source?.IsDirectory ?? Destination?.IsDirectory ?? false;
		public string SourceName => Source?.FullName;
		public string DestinationName => Destination?.FullName;
		public string RelativeName => Source?.RelativeName ?? Destination?.RelativeName;
		public bool IgnoreOrSkip => Type == TransferType.Skip || Type == TransferType.Ignore;

		public TransferFile(TransferType type, FileData source, FileData destination) {
			Type = type;
			Source = source;
			Destination = destination;
		}
		public TransferFile(TransferType type, string source, FileData destination) {
			Type = type;
			Source = new FileData(destination, source);
			Destination = destination;
		}
		public TransferFile(TransferType type, FileData source, string destination) {
			Type = type;
			Source = source;
			Destination = new FileData(source, destination);
		}
		public TransferFile(FileData sourceOrDestination) {
			Type = TransferType.Ignore;
			Source = sourceOrDestination;
			Destination = sourceOrDestination;
		}

		public override string ToString() => RelativeName;
		public int CompareTo(TransferFile other) {
			int diff = (int) Type - (int) other.Type;
			if (diff == 0) {
				int levels = Levels;
				int otherLevels = other.Levels;
				if (levels != otherLevels) {
					if (Type == TransferType.Remove)
						return other.Levels - Levels;
					else
						return Levels - other.Levels;
				}
				/*if (IsDirectory && other.IsDirectory) {
					if (Type == TransferType.Remove)
						return other.Levels - Levels;
					else
						return Levels - other.Levels;
				}*/
				else if (IsDirectory != other.IsDirectory) {
					return (IsDirectory ? -1 : 0) + (other.IsDirectory ? 1 : 0);
				}
				else {
					return string.Compare(RelativeName, other.RelativeName, true);
				}
			}
			return diff;
		}
		private int Levels => RelativeName.Count(c => c == '/');
	}
	public class FileData : IComparable<FileData> {
		public bool IsDirectory { get; set; }
		public string RelativeName { get; set; }
		public string FullName { get; set; }
		public DateTime LastWriteTimeUtc { get; set; }
		public bool Exists { get; set; }
		public bool Ignore { get; set; }

		private static DateTime FloorToSeconds(DateTime date) {
			return new DateTime(date.Ticks - (date.Ticks % TimeSpan.TicksPerSecond), date.Kind);
		}

		public FileData(FileSystemInfo info, string location) {
			IsDirectory = info.Attributes.HasFlag(FileAttributes.Directory);
			FullName = info.FullName.Replace('\\', '/');
			RelativeName = FullName.Substring(location.Length + 1).Replace('\\', '/');
			DateTime date = 
			LastWriteTimeUtc = FloorToSeconds(info.LastWriteTimeUtc);
			Exists = true;
		}
		public FileData(RemoteFileInfo info, string location) {
			IsDirectory = info.IsDirectory;
			FullName = info.FullName;
			RelativeName = FullName.Substring(location.Length + 1).Replace('\\', '/');
			LastWriteTimeUtc = FloorToSeconds(info.LastWriteTime.ToUniversalTime());
			Exists = true;
		}
		public FileData(FileData data, string location) {
			IsDirectory = data.IsDirectory;
			FullName = PathUtils.Combine(location, data.RelativeName);
			RelativeName = data.RelativeName;
			LastWriteTimeUtc = DateTime.MinValue;
			Exists = false;
		}

		public int CompareTo(FileData other) {
			return string.Compare(RelativeName, other.RelativeName);
		}

		public override string ToString() => RelativeName;
	}
}
