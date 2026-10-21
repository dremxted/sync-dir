namespace sync.Application.Extensions;

public static class PathExtensions
{
    extension(Path)
    {
        public static string AddDirectorySeparatorEnding(string path)
        {
            if(!Path.EndsInDirectorySeparator(path))
            {
                return path += Path.DirectorySeparatorChar;
            }

            return path;
        }
    }
}
