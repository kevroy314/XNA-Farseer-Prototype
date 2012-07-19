using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Media;

namespace KevinsDemo
{
    class GlobalGameOptions
    {
        public static bool musicOn = true;
        public static bool soundEffectsOn = true;
        public static bool fullScreen = false;
        public static Song currentSong = null;

        public static object toggleMusic(object param)
        {
            if (param != null)
            {
                if (param.GetType() == Type.GetType("bool"))
                {
                    musicOn = (bool)param;
                    return musicOn;
                }
            }

            musicOn = !musicOn;

            if (musicOn)
            {
                if (currentSong != null)
                    MediaPlayer.Play(currentSong);
            }
            else
                MediaPlayer.Stop();

            return musicOn;
        }

        public static string getMusicStateString()
        {
            return "Music: " + musicOn;
        }
    }
}
