using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpGen
{
    public static class FileUtil {
        public static IEnumerable<string> EnumerateFilesWithRelativePaths(string path, string searchPattern = "*") {
            return
                from filePath in Directory.EnumerateFiles(
                    path,
                    searchPattern,
                    SearchOption.AllDirectories)
                select Path.GetRelativePath(path, filePath);
        }
    }
}