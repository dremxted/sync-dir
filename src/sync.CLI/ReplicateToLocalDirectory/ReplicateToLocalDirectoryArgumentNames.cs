namespace sync.CLI;

public sealed record ReplicateToLocalDirectoryArgumentNames(
    string OriginDirectory,
    string ReplicaDirectory,
    string IntervalMs,
    string LogOutput);