using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockSnake
{
    /// <summary>
    /// Plays music in a random order.
    /// </summary>
    public class MusicPlayer
    {
        #region Members
        /// <summary>
        /// Contains a list of all sounds used in the music player.
        /// </summary>
        public List<SoundEffect> sounds = new List<SoundEffect>();

        /// <summary>
        /// Contains the current sound effect loaded.
        /// </summary>
        public SoundEffectInstance sound;

        /// <summary>
        /// Gets the position of the sound in the list.
        /// </summary>
        public int SoundIndex
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new music player instance.
        /// </summary>
        /// <param name="sounds">Sounds to be loaded.</param>
        public MusicPlayer(params SoundEffect[] snds)
        {
            SoundIndex = 0;

            foreach (SoundEffect sfx in snds)
            {
                sounds.Add(sfx);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Selects the next sound to play and returns the sound index.
        /// </summary>
        public int NextSoundRandom()
        {
            SoundIndex = new Random().Next(sounds.Count);
            sound = sounds.ElementAt(SoundIndex).CreateInstance();
            sound.Play();
            return SoundIndex;
        }

        /// <summary>
        /// Shuffles the sound list.
        /// </summary>
        public void Shuffle()
        {
            Random rng = new Random();
            int numSongs = sounds.Count;

            while (numSongs > 1)
            {
                numSongs--;
                int next = rng.Next(numSongs + 1);
                SoundEffect value = sounds[next];
                sounds[next] = sounds[numSongs];
                sounds[numSongs] = value;
            }
        }

        /// <summary>
        /// Progresses through the music list.
        /// </summary>
        public void Update()
        {
            //When the sound finishes, start another.
            if (sound.State == SoundState.Stopped)
            {
                //Randomizes to any song except the current one.
                int tempSoundIndex = SoundIndex;

                while (tempSoundIndex == SoundIndex)
                {
                    sound.Stop();
                    NextSoundRandom();
                }
            }
        }
        #endregion
    }
}
