namespace sync.CLI.ReplicateToLocalDirectory;

internal record ReplicateToLocalDirectoryArgs(
    DirectoryInfo OriginDirectory, 
    DirectoryInfo DestinationDirectory,
    double IntervalSeconds,
    FileInfo LogOutput);
