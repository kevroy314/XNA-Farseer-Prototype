using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace KevinsDemo
{
    class SoundEffectsManager
    {
        public static float Volume = 1.0f;
        public static float Pitch = 0.0f;
        public static float Pan = 0.0f;
        public static bool IsMuted = false;

        public static void Play(SoundEffect sound)
        {
            if(!IsMuted)
                sound.Play(Volume, Pitch, Pan);
        }
    }
}
