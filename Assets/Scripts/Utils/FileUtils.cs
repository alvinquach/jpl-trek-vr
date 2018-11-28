using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrekVRApplication {

    public static class FileUtils {

        public static IList<string> ListFiles(string directory, string searchPattern = "*", bool createIfNotExist = false) {
            DirectoryInfo dirInfo = new DirectoryInfo(directory);
            if (!dirInfo.Exists) {
                if (createIfNotExist) {
                    dirInfo.Create();
                }
                else {
                    return new List<string>();
                }
            }
            FileInfo[] files = dirInfo.GetFiles(searchPattern);
            return files.Select(file => file.Name).ToList();
        }

    }

}