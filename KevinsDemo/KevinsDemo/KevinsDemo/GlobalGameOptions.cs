using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;
using KevinsDemo.ScreenSystem;

namespace KevinsDemo
{
    class GlobalGameOptions
    {
        public static bool SoundEffectsOn = true;
        public static bool FullScreen = false;

        private static MenuScreen _optionsMenu = GenerateOptionsMenu();

        private static MenuScreen GenerateOptionsMenu()
        {
            MenuScreen optionsScreen = new MenuScreen("Options", false);

            optionsScreen.AddMenuItem(GetMusicStateString, EntryType.OptionsItem, ToggleMusic, null);
            optionsScreen.AddMenuItem(GetSoundEffectsStateString, EntryType.OptionsItem, ToggleSoundEffects, null);

            return optionsScreen;
        }

        public static MenuScreen OptionsMenu
        {
            get { return _optionsMenu; }
        }

        private static object ToggleMusic(object param)
        {
            if (param != null)
            {
                if (param.GetType() == Type.GetType("bool"))
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
                if (param.GetType() == Type.GetType("bool"))
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
    }
}
