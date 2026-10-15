namespace sync.CLI.ReplicateToLocalDirectory;

internal record ReplicateToLocalDirectoryArgs(
    string OriginDirectoryPath, 
    string DestinationDirectoryPath,
    double IntervalSeconds,
    string LogOutputFilePath);
