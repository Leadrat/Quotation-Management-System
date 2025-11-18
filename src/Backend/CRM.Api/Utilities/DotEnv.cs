using System;
using System.Collections.Generic;
using System.IO;

namespace CRM.Api.Utilities
{
    internal static class DotEnv
    {
        public static void Load(params string[] fileNames)
        {
            if (fileNames == null || fileNames.Length == 0)
            {
                fileNames = new[] { ".env.local", ".env" };
            }

            var searched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in EnumerateCandidateFiles(fileNames))
            {
                if (searched.Add(file) && File.Exists(file))
                {
                    LoadFile(file);
                }
            }
        }

        private static IEnumerable<string> EnumerateCandidateFiles(string[] fileNames)
        {
            var startDirs = new List<string>
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory()
            };

            foreach (var start in startDirs)
            {
                foreach (var dir in WalkUp(start))
                {
                    foreach (var name in fileNames)
                    {
                        yield return Path.Combine(dir, name);
                    }
                }
            }
        }

        private static IEnumerable<string> WalkUp(string startDirectory)
        {
            var dir = new DirectoryInfo(startDirectory);
            while (dir != null)
            {
                yield return dir.FullName;
                dir = dir.Parent;
            }
        }

        private static void LoadFile(string path)
        {
            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine?.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                int equalsIndex = line.IndexOf('=');
                if (equalsIndex <= 0) continue;

                var key = line.Substring(0, equalsIndex).Trim();
                var value = line.Substring(equalsIndex + 1).Trim();

                // Remove optional surrounding quotes
                if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                    (value.StartsWith("'") && value.EndsWith("'")))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                // Only set if not already set to avoid clobbering real envs
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
    }
}
