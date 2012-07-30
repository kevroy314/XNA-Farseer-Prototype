using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using KevinsDemo.ScreenSystem;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml.Serialization;
using KevinsDemo.AudioSystem;

namespace KevinsDemo
{
    #region Game Options Struct

    /// <summary>
    /// This struct represents all saved options on close/change. 
    /// If a value is not present here, it is not saved as part of the options system on restart.
    /// </summary>
    public struct OptionsDataStruct
    {
        public bool IsSoundEffectManagerMuted;
        public bool IsMediaPlayerMuted;
        public bool IsFullScreen;
    }

    #endregion

    /// <summary>
    /// The global game options helper class is a static class which manages the global options in the game.
    /// These include different graphics and sound options.
    /// </summary>
    class GlobalGameOptions
    {
        #region Variables

        //The options file for the game
        private static string _optionsFilename = "options.config";
        //A storage device representing where we save our options
        private static StorageDevice device;

        #endregion

        #region Initialize and Reset Functions

        //Initializer function to load the options from file (if they exist)
        //We initialize to default if we have no options file
        public static void InitOptions()
        {
            if (!LoadAndInitOptionsFromFile())
            {
                ResetOptionsToDefault();
            }
        }

        //Reset all of the options
        public static void ResetOptionsToDefault()
        {
            SoundEffectsManager.IsMuted = false;
            MusicManager.IsMuted = false;
            GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = false;
        }

        #endregion

        #region Options Properties

        public static OptionsDataStruct GameOptions
        {
            get
            {
                OptionsDataStruct data;
                data.IsMediaPlayerMuted = MusicManager.IsMuted;
                data.IsSoundEffectManagerMuted = SoundEffectsManager.IsMuted;
                data.IsFullScreen = GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;
                return data;
            }
            set
            {
                MusicManager.IsMuted = value.IsMediaPlayerMuted;
                SoundEffectsManager.IsMuted = value.IsSoundEffectManagerMuted;
                GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = value.IsFullScreen;
            }
        }

        #endregion

        #region Options File Load and Save Functions

        public static bool LoadAndInitOptionsFromFile()
        {
            //Get storage device
            StorageDevice.BeginShowSelector(GetStorageDevice, null);

            //Get a storage container
            IAsyncResult result = device.BeginOpenContainer("OptionsContainer", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            //If the options file does not exist, we cannot load it - return false to inform caller of that result
            if(!container.FileExists(_optionsFilename))
            {
                //Clean up
                container.Dispose();
                return false;
            }

            //Get the options file stream
            Stream stream = container.OpenFile(_optionsFilename, FileMode.Open);

            //Create a serializer for the data
            XmlSerializer serializer = new XmlSerializer(typeof(OptionsDataStruct));

            //Read the data from the file and store it into the appropriate variables
            OptionsDataStruct data = (OptionsDataStruct)serializer.Deserialize(stream);
            GameOptions = data;

            //Clean up
            stream.Close();
            container.Dispose();

            return true;
        }

        public static void SaveOptionsToFile()
        {
            //Get storage device
            StorageDevice.BeginShowSelector(GetStorageDevice, null);

            //Get a storage container
            IAsyncResult result = device.BeginOpenContainer("OptionsContainer", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            //If an options file exists, clear it
            if(container.FileExists(_optionsFilename))
                container.DeleteFile(_optionsFilename);

            //Create a stream to the file and a serializer for the data
            Stream stream = container.CreateFile(_optionsFilename);
            XmlSerializer serializer = new XmlSerializer(typeof(OptionsDataStruct));

            //Serialize and output the data
            serializer.Serialize(stream, GameOptions);

            //Clean up
            stream.Close();
            container.Dispose();
        }

        #endregion

        #region Private Storage Initialization Functions

        private static void GetStorageDevice(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
        }

        #endregion

        #region Options Menu Management Functions

        //Generate the options menu from scratch given current options
        private static OptionsMenu GenerateOptionsMenu()
        {
            OptionsMenu optionsScreen = new OptionsMenu("Options", false);

            optionsScreen.AddMenuItem(GetMusicStateString, EntryType.OptionsItem, ToggleMusic, null, false);
            optionsScreen.AddMenuItem(GetSoundEffectsStateString, EntryType.OptionsItem, ToggleSoundEffects, null, false);
            optionsScreen.AddMenuItem(GetFullScreenStateString, EntryType.OptionsItem, ToggleFullScreen, null, true);
            optionsScreen.AddMenuItem("Go Back", EntryType.ScreenExitItem, null);

            return optionsScreen;
        }

        #region Options Menu Properties

        //Returns the current options menu
        public static OptionsMenu OptionsMenu
        {
            get { return GenerateOptionsMenu(); }
        }

        #endregion

        #endregion

        #region Options Modification Functions

        //Toggle or set the music mute option
        private static object ToggleMusic(object param)
        {
            if (param != null)
            {
                if (param.GetType() == typeof(bool))
                {
                    MusicManager.IsMuted = (bool)param;
                    return MusicManager.IsMuted;
                }
            }

            MusicManager.ToggleMute();

            return MusicManager.IsMuted;
        }

        //Toggle or set the sound effects mute option
        private static object ToggleSoundEffects(object param)
        {
            if (param != null)
            {
                if (param.GetType() == typeof(bool))
                {
                    SoundEffectsManager.IsMuted = (bool)param;
                    return SoundEffectsManager.IsMuted;
                }
            }

            SoundEffectsManager.IsMuted = !SoundEffectsManager.IsMuted;

            return SoundEffectsManager.IsMuted;
        }

        //Toggle or set the full screen option (requires restart)
        private static object ToggleFullScreen(object param)
        {

            if (param != null)
            {
                if (param.GetType() == typeof(bool))
                {
                    bool isFullScreen = GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;
                    if ((bool)param != isFullScreen)
                        GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = (bool)param;
                    return GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;
                }
            }

            GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = !GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;

            return GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;
        }

        #endregion

        #region Options ToString Functions

        //Get the music mute string representation
        private static string GetMusicStateString()
        {
            return "Music: " + (MusicManager.IsMuted ? "Off" : "On");
        }

        //Get the sound effects mute string representation
        private static string GetSoundEffectsStateString()
        {
            return "Sound Effects: " + (SoundEffectsManager.IsMuted ? "Off" : "On");
        }

        //Get the full screen state string representation
        private static string GetFullScreenStateString()
        {
            return "Fullscreen: " + (GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen ? "Yes" : "No");
        }

        #endregion
    }
}
