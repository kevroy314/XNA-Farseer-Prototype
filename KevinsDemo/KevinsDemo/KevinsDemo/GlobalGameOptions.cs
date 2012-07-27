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

namespace KevinsDemo
{
    public struct OptionsDataStruct
    {
        public bool IsSoundEffectManagerMuted;
        public bool IsMediaPlayerMuted;
        public bool IsFullScreen;
    }

    class GlobalGameOptions
    {   
        private static string _optionsFilename = "options.config";
        private static StorageDevice device;

        public static void InitOptions()
        {
            if (!LoadAndInitOptionsFromFile())
            {
                ResetOptionsToDefault();
            }
        }

        public static void ResetOptionsToDefault()
        {
            SoundEffectsManager.IsMuted = false;
            MediaPlayer.IsMuted = false;
            GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = false;
        }

        public static OptionsDataStruct GameOptions
        {
            get
            {
                OptionsDataStruct data;
                data.IsMediaPlayerMuted = MediaPlayer.IsMuted;
                data.IsSoundEffectManagerMuted = SoundEffectsManager.IsMuted;
                data.IsFullScreen = GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen;
                return data;
            }
            set
            {
                MediaPlayer.IsMuted = value.IsMediaPlayerMuted;
                SoundEffectsManager.IsMuted = value.IsSoundEffectManagerMuted;
                GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen = value.IsFullScreen;
            }
        }

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

        private static void GetStorageDevice(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
        }

        private static OptionsMenu GenerateOptionsMenu()
        {
            OptionsMenu optionsScreen = new OptionsMenu("Options", false);

            optionsScreen.AddMenuItem(GetMusicStateString, EntryType.OptionsItem, ToggleMusic, null, false);
            optionsScreen.AddMenuItem(GetSoundEffectsStateString, EntryType.OptionsItem, ToggleSoundEffects, null, false);
            optionsScreen.AddMenuItem(GetFullScreenStateString, EntryType.OptionsItem, ToggleFullScreen, null, true);
            optionsScreen.AddMenuItem("Go Back", EntryType.ScreenExitItem, null);

            return optionsScreen;
        }
        public static OptionsMenu OptionsMenu
        {
            get { return GenerateOptionsMenu(); }
        }

        private static object ToggleMusic(object param)
        {
            if (param != null)
            {
                if (param.GetType() == typeof(bool))
                {
                    MediaPlayer.IsMuted = (bool)param;
                    return MediaPlayer.IsMuted;
                }
            }

            MediaPlayer.IsMuted = !MediaPlayer.IsMuted;

            return MediaPlayer.IsMuted;
        }
        private static string GetMusicStateString()
        {
            return "Music: " + (MediaPlayer.IsMuted ? "Off" : "On");
        }

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
        private static string GetSoundEffectsStateString()
        {
            return "Sound Effects: " + (SoundEffectsManager.IsMuted ? "Off" : "On");
        }

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
        private static string GetFullScreenStateString()
        {
            return "Fullscreen: " + (GameMain.ScreenManager.GraphicsDeviceManager.IsFullScreen ? "Yes" : "No");
        }
    }
}
