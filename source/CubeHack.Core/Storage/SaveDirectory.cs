// Copyright (c) the CubeHack authors. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt in the project root.

using System;
using System.IO;
using System.Threading.Tasks;

namespace CubeHack.Storage
{
    public static class SaveDirectory
    {
        /// <summary>
        /// The name of the special save file used for debugging. (This is only necessary until we can load save files from the menu in-game.)
        /// </summary>
        public const string DebugGame = "Debug Game";

        private static readonly string _saveDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "CubeHack", "SaveGames");

        public static Task<ISaveFile> OpenFileAsync(string name)
        {
            return Task.Run<ISaveFile>(
                () =>
                {
                    string path = Path.Combine(_saveDirectoryPath, name);

                    string infoFilePath = Path.Combine(path, "cubesave");
                    SaveFileInfo info;
                    if (File.Exists(infoFilePath))
                    {
                        using (var stream = File.Open(infoFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            info = ProtoBuf.Serializer.Deserialize<SaveFileInfo>(stream);
                        }

                        if (info.Version != SaveFileInfo.CurrentVersion)
                        {
                            /* For now, if the file version doesn't match, simply delete the save file.
                             * This is obviously not how things should behave in a "released" game.
                             */
                            try
                            {
                                Directory.Delete(path, true);
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (!File.Exists(infoFilePath))
                    {
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        info = new SaveFileInfo
                        {
                            Version = SaveFileInfo.CurrentVersion,
                        };

                        using (var stream = File.Open(infoFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            ProtoBuf.Serializer.Serialize(stream, info);
                        }
                    }

                    return new SaveFile(Path.Combine(path, "data"));
                });
        }
    }
}
