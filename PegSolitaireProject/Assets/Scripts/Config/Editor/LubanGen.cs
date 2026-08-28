using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Config.Editor
{
    public static class LubanGen
    {
        private const string MenuPath = "Tools/Config/Luban Gen";
        private const string GeneratorRelativePath = "Tools/Excel2Config/gen.bat";

        [MenuItem(MenuPath, false, 1)]
        public static void Generate()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var repositoryRoot = projectRoot != null ? Directory.GetParent(projectRoot)?.FullName : null;
            var generatorPath = repositoryRoot != null
                ? Path.Combine(repositoryRoot, GeneratorRelativePath)
                : null;

            if (string.IsNullOrEmpty(generatorPath) || !File.Exists(generatorPath))
            {
                Debug.LogError($"[Luban] Generator not found: {generatorPath}");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/c \"{generatorPath}\"",
                    WorkingDirectory = Path.GetDirectoryName(generatorPath),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    Debug.LogError("[Luban] Failed to start the generator process.");
                    return;
                }

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    Debug.Log($"[Luban] {output}");
                }

                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[Luban] Generation failed with exit code {process.ExitCode}.\n{error}");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Debug.LogWarning($"[Luban] {error}");
                }

                AssetDatabase.Refresh();
                Debug.Log("[Luban] Generation completed. Output: Assets/Game/Config/");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
