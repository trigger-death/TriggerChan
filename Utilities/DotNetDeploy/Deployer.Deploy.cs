using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetDeploy.Utils;
using WinSCP;
using Console = DotNetDeploy.Utils.ColorConsole;

namespace DotNetDeploy {
	partial class Deployer {

		private void Deploy() {
			if (!Method.DirectoryExists(Destination))
				Method.CreateDirectory(Destination);
			//if (!Method.DirectoryExists(ConfigDestination))
			//	Method.CreateDirectory(ConfigDestination);

			List<FileData> publishFiles = new List<FileData>(LocalMethod.Enumerate(Source));
			List<FileData> deployFiles = new List<FileData>(Method.Enumerate(Destination));
			//List<FileData> configPublishFiles = new List<FileData>(LocalMethod.Enumerate(Source));
			//List<FileData> configDeployFiles = new List<FileData>(Method.Enumerate(ConfigDestination));
			
			//Deployment.ConfigInclude.Run(configPublishFiles);
			//Deployment.ConfigIgnore.Run(configDeployFiles);
			Deployment.BuildIgnore.Run(publishFiles);
			Deployment.DeployIgnore.Run(deployFiles);
			
			List<TransferFile> transfers = CreateTransfers(publishFiles, deployFiles, Destination);
			//List<TransferFile> configTransfers = CreateTransfers(configPublishFiles, configDeployFiles, ConfigDestination);

			/*// Filter ignored and skipped files so they are not logged more than once
			for (int i = 0; i < transfers.Count; i++) {
				TransferFile deploy = transfers[i];
				for (int j = 0; j < configTransfers.Count; j++) {
					TransferFile config = configTransfers[j];
					if (deploy.RelativeName == config.RelativeName) {
						if (deploy.IgnoreOrSkip != config.IgnoreOrSkip) {
							if (deploy.IgnoreOrSkip) {
								transfers.RemoveAt(i);
								i--;
							}
							else {
								configTransfers.RemoveAt(j);
								//j--; // Not necissary, we break here anyways
							}
						}
						else if (deploy.IgnoreOrSkip) {
							// Skip has precedence
							if (deploy.Type == TransferType.Skip || config.Type != TransferType.Skip) {
								configTransfers.RemoveAt(j);
								//j--; // Not necissary, we break here anyways
							}
							else {
								transfers.RemoveAt(i);
								i--;
							}
						}
						break;
					}
				}
			}*/

			RunTransfers(transfers);
			//RunTransfers(configTransfers);
		}
		private List<TransferFile> CreateTransfers(IEnumerable<FileData> publishFiles, IEnumerable<FileData> deployFiles, string destination) {
			List<TransferFile> transfers = new List<TransferFile>();

			List<FileData> dstFiles = new List<FileData>(deployFiles);

			foreach (FileData srcFile in publishFiles) {
				FileData dstFile = null;
				for (int i = 0; i < dstFiles.Count; i++) {
					FileData dstFileTmp = dstFiles[i];
					if (srcFile.CompareTo(dstFileTmp) == 0) {
						if (srcFile.IsDirectory != dstFileTmp.IsDirectory)
							break;
						dstFile = dstFileTmp;
						dstFiles.RemoveAt(i);
						i--;
					}
				}
				TransferType type;
				if (dstFile == null) {
					if (srcFile.Ignore)
						type = TransferType.Ignore;
					else
						type = TransferType.Add;
				}
				else {
					if (srcFile.Ignore) {
						if (dstFile.Ignore)
							type = TransferType.Ignore;
						else
							type = TransferType.Remove;
					}
					else if (!srcFile.IsDirectory && srcFile.LastWriteTimeUtc != dstFile.LastWriteTimeUtc)
						type = TransferType.Update;
					else
						type = TransferType.Skip;
				}
				transfers.Add(new TransferFile(type, srcFile, destination));
			}
			foreach (FileData dstFile in dstFiles) {
				TransferType type;
				if (dstFile.Ignore)
					type = TransferType.Ignore;
				//else if (!dstFile.IsDirectory && RemotePath.GetDirectoryName(dstFile.FullName) == destination)
				//	type = TransferType.Skip; // Old filter method
				else
					type = TransferType.Remove;
				transfers.Add(new TransferFile(type, Source, dstFile));
			}

			transfers.Sort();

			return transfers;
		}
		private void RunTransfers(List<TransferFile> transfers) {
			foreach (TransferFile t in transfers) {
				string dirStr = (t.IsDirectory ? "/" : string.Empty);
				switch (t.Type) {
				case TransferType.Add:
					if (t.IsDirectory)
						Method.CreateDirectory(t.DestinationName);
					else
						Method.CopyFile(t.SourceName, t.DestinationName);
					Console.WriteLine(ConsoleColor.Green, $"Added {t}{dirStr}");
					break;
				case TransferType.Update:
					if (t.IsDirectory)
						throw new InvalidOperationException();
					Method.CopyFile(t.SourceName, t.DestinationName);
					Console.WriteLine(ConsoleColor.Yellow, $"Updated {t}");
					break;
				case TransferType.Remove:
					if (t.IsDirectory)
						Method.DeleteDirectory(t.DestinationName);
					else
						Method.DeleteFile(t.DestinationName);
					Console.WriteLine(ConsoleColor.Red, $"Removed {t}{dirStr}");
					break;
				case TransferType.Skip:
					if (IsVerbose)
						Console.WriteLine(ConsoleColor.Gray, $"Skipped {t}{dirStr}");
					break;
				case TransferType.Ignore:
					if (IsVerbose)
						Console.WriteLine(ConsoleColor.DarkGray, $"Ignored {t}{dirStr}");
					break;
				}
			}
		}
	}
}
