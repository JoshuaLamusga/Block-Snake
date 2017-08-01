using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    /// <summary>
    /// An extremely basic music player.
    /// </summary>
    public class MusicPlayer
    {
        public List<SoundEffect> sounds = new List<SoundEffect>(); //The list of sounds.
        public SoundEffectInstance sound; //The current sound.
        public int soundIndex = 0; //The position of the sound in the list.

        /// <summary>
        /// Creates a new music player instance.
        /// </summary>
        /// <param name="sounds">Takes any number of sounds.</param>
        public MusicPlayer(params SoundEffect[] snds)
        {
            //If there was anything specified in the parentheses.
            //Adds the listed songs to the playlist.
            foreach (SoundEffect sfx in snds)
            {
                sounds.Add(sfx);
            }
        }

        /// <summary>
        /// Randomly selects the next sound to play.
        /// Returns the sound's index.
        /// </summary>
        public int NextSoundRandom()
        {
            soundIndex = new Random().Next(sounds.Count);
            sound = sounds.ElementAt<SoundEffect>(soundIndex).CreateInstance();
            sound.Play();
            return soundIndex;
        }

        /// <summary>
        /// Shuffles the sound list.
        /// </summary>
        public void Shuffle()
        {
            Random rng = new Random();
            int numSongs = sounds.Count;
            //Iterates through O(1) times.
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
        /// Checks to see if the song ended and begins the next.
        /// </summary>
        public void Update()
        {
            //When the sound finishes, start another.
            if (sound.State == SoundState.Stopped)
            {
                //Keeps track of the old sound index for the loop.
                int tempSoundIndex = soundIndex;

                while (tempSoundIndex == soundIndex)
                {
                    sound.Stop();
                    NextSoundRandom();
                }
            }
        }
    }
}
