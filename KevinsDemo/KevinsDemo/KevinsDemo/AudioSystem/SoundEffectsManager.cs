using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace KevinsDemo
{
    /// <summary>
    /// This class can be used to manage the sound effects globally. 
    /// It is capable of applying a global volume, pitch and pan or playing effects with special volume, pitch or pan.
    /// </summary>
    class SoundEffectsManager
    {
        #region Variables

        //Volume at which all sound effects will play
        private static float _volume = 1.0f;
        //Pitch to modify all sound effects
        private static float _pitch = 0.0f;
        //Pan to apply to all sound effects
        private static float _pan = 0.0f;
        //Mute to apply to all sound effects
        private static bool _isMuted = false;

        #endregion

        #region Accessor and Mutator Methods

        //Play a sound effect with the global audio properties
        public static void Play(SoundEffect sound)
        {
            if(!IsMuted)
                sound.Play(Volume, Pitch, Pan);
        }

        //Play a sound effect with a specified volume but global pitch and pan
        public static void Play(SoundEffect sound, float volume)
        {
            if (!IsMuted)
                sound.Play(volume, Pitch, Pan);
        }

        //Play a sound effect with a specified volume and pitch but global pan
        public static void Play(SoundEffect sound, float volume, float pitch)
        {
            if (!IsMuted)
                sound.Play(volume, pitch, Pan);
        }

        //Play a sound effect with a specified volume, pitch and pan
        public static void Play(SoundEffect sound, float volume, float pitch, float pan)
        {
            if (!IsMuted)
                sound.Play(volume, pitch, pan);
        }

        #endregion

        #region Properties

        //The global sound effects volume
        public static float Volume
        {
            get { return SoundEffectsManager._volume; }
            set { SoundEffectsManager._volume = value; }
        }

        //The global sound effects pitch
        public static float Pitch
        {
            get { return SoundEffectsManager._pitch; }
            set { SoundEffectsManager._pitch = value; }
        }

        //The global sound effects pan
        public static float Pan
        {
            get { return SoundEffectsManager._pan; }
            set { SoundEffectsManager._pan = value; }
        }

        //The global sound effects mute state
        public static bool IsMuted
        {
            get { return SoundEffectsManager._isMuted; }
            set { SoundEffectsManager._isMuted = value; }
        }

        #endregion
    }
}
