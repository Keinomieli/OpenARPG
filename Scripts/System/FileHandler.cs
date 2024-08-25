using Godot;
using System;
//using Tomlyn;
//using Tomlyn.Model;

namespace OpenARPG.System
{
    public static class FileHandler
    {
        public static bool TomlExists(string fileName)
        {
            try
            {
                return FileAccess.FileExists($"user://{fileName}.toml");
            }
            catch (Exception e)
            {
                GameStateManager.FatalError();

                Logger.Error("FileHandler: TomlExists: Exception while accessing file system");
                Logger.Error(e.ToString());

                return false;
            }
        }

        /*public static TomlTable LoadToml(string fileName)
        {
            string filePath = $"user://{fileName}.toml";

            try
            {
                if (!FileAccess.FileExists(filePath))
                    return null;

                using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                string fileString = file.GetAsText();

                return Toml.ToModel(fileString);;
            }
            catch (Exception e)
            {
                GameStateManager.FatalError();

                Logger.Error($"FileHandler: LoadToml: Exception while reading file \"{filePath}\"");
                Logger.Error(e.ToString());

                return null;
            }
        }*/

        /*public static void SaveToml(string fileName, TomlTable tomlTable)
        {
            string filePath = $"user://{fileName}.toml";

            try
            {
                using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
                string tomlString = Toml.FromModel(tomlTable);
                
                file.StoreString(tomlString);
            }
            catch (Exception e)
            {
                GameStateManager.FatalError();

                Logger.Error($"FileHandler: SaveToml: Exception while writing file \"{filePath}\"");
                Logger.Error(e.ToString());
            }
        }*/
    }
}