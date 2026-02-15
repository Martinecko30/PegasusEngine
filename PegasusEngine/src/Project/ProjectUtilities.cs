using Log = PegasusEngine.Core.Log;

namespace PegasusEngine.Project;

public class ProjectUtilities
{
    /// <summary>
    /// Finds all files with a given extension in a directory (non-recursive).
    /// </summary>
    /// <param name="folder">Folder in which to look.</param>
    /// <param name="extension">Extension of the files.</param>
    /// <returns></returns>
    public static IEnumerable<string> FindFilesInFolder(string folder, string extension)
    {
        if (!Directory.Exists(folder))
        {
            Log.EngineWarn("Invalid filepath passed: {0}", folder);
            return Enumerable.Empty<string>();
        }
        
        string searchPattern = $"*{extension}";
        return Directory.EnumerateFiles(folder, searchPattern, SearchOption.AllDirectories);
    }

    /// <summary>
    /// Checks whether the given file resides directly in the specified folder.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="folderPath">Path to the folder.</param>
    /// <returns></returns>
    public static bool IsFileInFolder(String filePath, string folderPath)
    {
        if (!File.Exists(filePath))
        {
            Log.EngineWarn("Invalid filePath passed: {0}", filePath);
            return false;
        }

        if (!Directory.Exists(folderPath))
        {
            Log.EngineWarn("Invalid folderPath passed: {0}", folderPath);
            return false;
        }
        
        string? parentPath = Path.GetDirectoryName(Path.GetFullPath(filePath));
        string targetFolder = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        
        return string.Equals(parentPath, targetFolder, StringComparison.OrdinalIgnoreCase);
    }
    
    /// <summary>
    /// Appends a file extension to a path.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <param name="extension">Extension to append.</param>
    /// <returns></returns>
    public static string AppendExtension(string path, string extension) => path + extension;

    /// <summary>
    /// Removes the last extension from a path while preserving its directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string StripExtension(string path)
    {
        string? directory = Path.GetDirectoryName(path);
        string fileNameWithoutLastExtension = Path.GetFileNameWithoutExtension(path);
        
        return directory != null
            ? Path.Combine(directory, fileNameWithoutLastExtension)
            : fileNameWithoutLastExtension;
    }
}