namespace sync.CLI.ReplicateToLocalDirectory;

internal record ReplicateToLocalDirectoryArgs(
    DirectoryInfo OriginDirectory, 
    DirectoryInfo DestinationDirectory,
    TimeSpan Interval,
    FileInfo LogOutput);
