# Rep
**Description**  
Synchronizes source and replica directories in one-way direction.  
Data of the specified origin directory becomes replicated to the specified replica directory.

**WARNING**  
Possible data loss if a file or directory is placed inside the specified replica directory.

- `rep` deletes files in the **replica** directory that are not in the origin directory.

- `rep` overwrites files in the **replica** directory when files with the same name exist in the **origin** directory but have a different MD5 hash.

- `rep` can be canceled manually at any time using the `Ctrl + C` key combination, which may corrupt the replicated file.

# Install
[Releases - GitHub Link](https://github.com/dremxted/sync-dir/releases)

```bash
dotnet tool install rep -g --source <nupkg_directory>
```

# Help
## Usage
> `rep <origin-directory> <replica-directory> <intervals-seconds> <log-output>`

## Arguments
- `<origin-directory>`: Path to the directory FROM which content will be replicated
- `<replica-directory>`: Path to the directory TO which content will be replicated
- `<interval-seconds>`: Interval between synchronization execution in seconds
- `<log-output>`: Path to the file or directory to which logs are persisted

## Options
- `-?, -h, --help`: Show help and usage information
- `--version`: Show version information