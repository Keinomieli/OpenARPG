using Godot;
using System;
//using Tomlyn.Model;

namespace OpenARPG.System
{
    public partial class UserPrefs : Resource
    {
        [Export] float musicVolume = 1.0f; 
        [Export] float sfxVolume = 1.0f; 

        const string fileName = "user_prefs";

        /*public void Save()
        {
            TomlTable tomlTable = new()
            {
                {
                    "Settings", new TomlTable()
                    {
                        { "musicVolume", musicVolume },
                        { "sfxVolume", sfxVolume }
                    }
                }
            };

            FileHandler.SaveToml(fileName, tomlTable);

            Logger.Log($"UserPrefs: LoadOrCreate: User preferences saved to user data folder");
        }*/
        
        public static UserPrefs LoadOrCreate()
        {
            /*TomlTable savedPrefs = FileHandler.LoadToml(fileName);

            if (savedPrefs == null)
            {
                UserPrefs prefs = new();
                prefs.Save();
                return new UserPrefs();
            }
            else*/
            {
                try
                {
                    UserPrefs prefs = new();

                    /*if (savedPrefs["Settings"] is TomlTable settings)
                    {
                        if (settings.ContainsKey("musicVolume"))
                            prefs.musicVolume = float.Parse(settings["musicVolume"].ToString());

                        if (settings.ContainsKey("sfxVolume"))
                            prefs.sfxVolume = float.Parse(settings["sfxVolume"].ToString());
                    }*/

                    Logger.Log($"UserPrefs: LoadOrCreate: User preferences loaded from user data folder. MusicVolume = {prefs.musicVolume} SfxVolume = {prefs.sfxVolume}");

                    return prefs;
                }
                catch (Exception e)
                {
                    GameStateManager.FatalError();

                    Logger.Error($"UserPrefs: LoadOrCreate: Exception while parsing userprefs toml file");
                    Logger.Error(e.ToString());
                    
                    return new UserPrefs();
                }
            }
        }
    }
}