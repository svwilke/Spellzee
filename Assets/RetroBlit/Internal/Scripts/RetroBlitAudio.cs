namespace RetroBlitInternal
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// RetroBlit Audio subsystem, responsible for playing sound and music
    /// </summary>
    public class RetroBlitAudio : MonoBehaviour
    {
        private static long mSequenceCounter = 1;

#pragma warning disable 0414 // Unused warning
        private RetroBlitAPI mRetroBlitAPI = null;
#pragma warning restore 0414

        private SoundChannel mMusicChannel;

        private SoundChannel[] mSoundChannels = new SoundChannel[RetroBlitHW.HW_MAX_SOUND_CHANNELS];

        private AudioClip[] mSoundClips = new AudioClip[RetroBlitHW.HW_SOUND_SLOTS];
        private AudioClip[] mMusicClips = new AudioClip[RetroBlitHW.HW_MUSIC_SLOTS];

        private float mTargetMusicVolume = 1.0f;
        private float mTargetMusicPitch = 1.0f;
        private AudioClip mPreviousMusicClip = null;
        private AudioClip mCurrentMusicClip = null;

        /// <summary>
        /// Initialize the subsystem
        /// </summary>
        /// <param name="api">Reference to subsystem wrapper</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RetroBlitAPI api)
        {
            mRetroBlitAPI = api;

            var audioSourceObj = GameObject.Find("RetroBlitAudio");
            if (audioSourceObj == null)
            {
                Debug.Log("Can't find RetroBlitAudio!");
                return false;
            }

            InitializeChannels();

            return true;
        }

        /// <summary>
        /// Set a sound in given sound slot index
        /// </summary>
        /// <param name="slotIndex">Slot index</param>
        /// <param name="clipName">Clip filename</param>
        public void SoundSet(int slotIndex, string clipName)
        {
            if (slotIndex < 0 || slotIndex >= RetroBlitHW.HW_SOUND_SLOTS)
            {
                return;
            }

            if (clipName == null)
            {
                if (mSoundClips[slotIndex] != null)
                {
                    mSoundClips[slotIndex].UnloadAudioData();
                    mSoundClips[slotIndex] = null;
                }

                return;
            }

#if !RETROBLIT_STANDALONE
            var clip = Resources.Load<AudioClip>(clipName);
#else
            var clip = Resources.LoadAudioSample(clipName);
#endif

            if (clip == null)
            {
                Debug.LogError("Can't find sound file " + clipName + ", it must be under the Assets/Resources folder.");
                return;
            }

            mSoundClips[slotIndex] = clip;
        }

        /// <summary>
        /// Set music in the given music slot index
        /// </summary>
        /// <param name="slotIndex">Music slot index</param>
        /// <param name="clipName">Clip filename</param>
        public void MusicSet(int slotIndex, string clipName)
        {
            if (slotIndex < 0 || slotIndex >= RetroBlitHW.HW_MUSIC_SLOTS)
            {
                return;
            }

            if (clipName == null)
            {
                if (mMusicClips[slotIndex] != null)
                {
                    mMusicClips[slotIndex].UnloadAudioData();
                    mMusicClips[slotIndex] = null;
                }

                return;
            }

#if !RETROBLIT_STANDALONE
            var clip = Resources.Load<AudioClip>(clipName);
#else
            var clip = Resources.LoadAudioMusic(clipName);
#endif

            if (clip == null)
            {
                Debug.LogError("Can't find music file " + clipName + ", it must be under the Assets/Resources folder.");
                return;
            }

            // If crossfading music clip is affected then set it to null
            if (mPreviousMusicClip != null && mMusicClips[slotIndex] == mPreviousMusicClip && clip != mMusicClips[slotIndex])
            {
                mPreviousMusicClip = null;
            }

            // If current music clip is affected then update the clip
            if (mCurrentMusicClip != null && mMusicClips[slotIndex] == mCurrentMusicClip && clip != mMusicClips[slotIndex])
            {
                var channel = mMusicChannel;
                if (channel.Source != null)
                {
                    channel.Source.clip = clip;
                    channel.Source.loop = true;
                    channel.Source.Play();
                }

                mCurrentMusicClip = clip;
            }

            mMusicClips[slotIndex] = clip;
        }

        /// <summary>
        /// Play sound from slot, with given volume and pitch
        /// </summary>
        /// <param name="slotIndex">Slot index</param>
        /// <param name="volume">Volume</param>
        /// <param name="pitch">Pitch</param>
        /// <returns>Sound reference</returns>
        public SoundReference SoundPlay(int slotIndex, float volume, float pitch)
        {
            if (slotIndex < 0 || slotIndex >= RetroBlitHW.HW_SOUND_SLOTS)
            {
                return new SoundReference(-1, -1);
            }

            var clip = mSoundClips[slotIndex];
            if (clip == null)
            {
                return new SoundReference(-1, -1);
            }

            int channelSlot = GetFreeSoundChannel();
            if (channelSlot != -1)
            {
                var channel = mSoundChannels[channelSlot];
                if (channel.Source != null)
                {
                    channel.Source.volume = volume;
                    channel.Source.pitch = pitch;
                    channel.Source.clip = clip;
                    channel.Source.loop = false;
                    channel.Source.Play();

                    long seq = mSequenceCounter++;
                    channel.Sequence = seq;
                    return new SoundReference(channelSlot, seq);
                }
            }

            return new SoundReference(-1, -1);
        }

        /// <summary>
        /// Checks if sound is playing
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <returns>True if playing</returns>
        public bool SoundIsPlaying(SoundReference soundReference)
        {
            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return false;
            }

            return source.isPlaying;
        }

        /// <summary>
        /// Set volume of playing sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <param name="volume">Volume</param>
        public void SoundVolumeSet(SoundReference soundReference, float volume)
        {
            if (volume < 0)
            {
                return;
            }

            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return;
            }

            source.volume = volume;
        }

        /// <summary>
        /// Set pitch of playing sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <param name="pitch">Pitch</param>
        public void SoundPitchSet(SoundReference soundReference, float pitch)
        {
            if (pitch < 0)
            {
                return;
            }

            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return;
            }

            source.pitch = pitch;
        }

        /// <summary>
        /// Get volume of playing sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <returns>Volume</returns>
        public float SoundVolumeGet(SoundReference soundReference)
        {
            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return 0;
            }

            return source.volume;
        }

        /// <summary>
        /// Get pitch of playing sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <returns>Pitch</returns>
        public float SoundPitchGet(SoundReference soundReference)
        {
            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return 0;
            }

            return source.pitch;
        }

        /// <summary>
        /// Stop playing a sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        public void SoundStop(SoundReference soundReference)
        {
            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return;
            }

            var channel = mSoundChannels[soundReference.SoundChannel];
            channel.Sequence = -1;

            source.Stop();
        }

        /// <summary>
        /// Set loop flag for playing sound
        /// </summary>
        /// <param name="soundReference">Sound reference</param>
        /// <param name="loop">True if should loop</param>
        public void SoundLoopSet(SoundReference soundReference, bool loop)
        {
            var source = GetSourceForSoundReference(soundReference);
            if (source == null)
            {
                return;
            }

            source.loop = loop;
        }

        /// <summary>
        /// Play music from given music slot
        /// </summary>
        /// <param name="slotIndex">Slot index</param>
        public void MusicPlay(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= RetroBlitHW.HW_MUSIC_SLOTS)
            {
                return;
            }

            var clip = mMusicClips[slotIndex];
            if (clip == null)
            {
                return;
            }

            if (mPreviousMusicClip == null)
            {
                mPreviousMusicClip = mCurrentMusicClip;
            }

            mCurrentMusicClip = clip;

            // If there is no music currently playing then snap the target volume and pitch to final value instead of fading
            if (mMusicChannel.Source == null || !mMusicChannel.Source.isPlaying)
            {
                mMusicChannel.Source.volume = mTargetMusicVolume;
                mMusicChannel.Source.pitch = mTargetMusicPitch;
            }
        }

        /// <summary>
        /// Stop playing music
        /// </summary>
        public void MusicStop()
        {
            mPreviousMusicClip = mCurrentMusicClip;
            mCurrentMusicClip = null;
        }

        /// <summary>
        /// Set volume of playing music
        /// </summary>
        /// <param name="volume">Volume</param>
        public void MusicVolumeSet(float volume)
        {
            mTargetMusicVolume = volume;

            // If not already playing music then snap the volume to request value instead of lerping
            if (mMusicChannel != null && mCurrentMusicClip == null && mMusicChannel.Source != null)
            {
                mMusicChannel.Source.volume = volume;
            }
        }

        /// <summary>
        /// Set pitch of playing music
        /// </summary>
        /// <param name="pitch">Pitch</param>
        public void MusicPitchSet(float pitch)
        {
            mTargetMusicPitch = pitch;

            // If not already playing music then snap the pitch to request value instead of lerping
            if (mMusicChannel != null && mCurrentMusicClip == null && mMusicChannel.Source != null)
            {
                mMusicChannel.Source.pitch = pitch;
            }
        }

        /// <summary>
        /// Get volume of playing music
        /// </summary>
        /// <returns>Volume</returns>
        public float MusicVolumeGet()
        {
            return mTargetMusicVolume;
        }

        /// <summary>
        /// Get pitch of playing music
        /// </summary>
        /// <returns>Pitch</returns>
        public float MusicPitchGet()
        {
            return mTargetMusicPitch;
        }

        private AudioSource GetSourceForSoundReference(SoundReference soundRef)
        {
            if (soundRef.Sequence < 0)
            {
                return null;
            }

            SoundChannel channel;

            if (soundRef.SoundChannel >= 0 && soundRef.SoundChannel < RetroBlitHW.HW_MAX_SOUND_CHANNELS)
            {
                channel = mSoundChannels[soundRef.SoundChannel];
            }
            else
            {
                return null;
            }

            // Check if this reference is outdated
            if (channel.Sequence != soundRef.Sequence)
            {
                return null;
            }

            return channel.Source;
        }

        private void Update()
        {
            var channel = mMusicChannel;
            if (channel != null && channel.Source != null)
            {
                // Check if crossfading
                if (mPreviousMusicClip != null)
                {
                    // Fade out old sound first
                    channel.Source.volume = Mathf.Lerp(channel.Source.volume, 0, 0.1f);
                    if (channel.Source.volume <= 0.005f)
                    {
                        mPreviousMusicClip = null;
                        channel.Source.clip = null;
                        channel.Source.time = 0;
                        channel.Source.Stop();
                    }
                }
                else
                {
                    if (channel.Source.clip != mCurrentMusicClip)
                    {
                        channel.Source.clip = mCurrentMusicClip;
                        channel.Source.loop = true;
                        channel.Source.time = 0;
                        channel.Source.Play();
                    }

                    channel.Source.volume = Mathf.Lerp(channel.Source.volume, mTargetMusicVolume, 0.15f);
                    channel.Source.pitch = Mathf.Lerp(channel.Source.pitch, mTargetMusicPitch, 0.15f);
                }
            }
        }

        private void InitializeChannels()
        {
            // Delete any existing audio sources
            var audioSources = gameObject.GetComponents<AudioSource>();
            if (audioSources != null)
            {
                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (audioSources[i] != null)
                    {
                        Destroy(audioSources[i]);
                    }
                }
            }

            audioSources = null;

            AudioSource audioSource;

            for (int i = 0; i < mSoundChannels.Length; i++)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.bypassEffects = true;
                audioSource.bypassListenerEffects = true;
                audioSource.bypassReverbZones = true;
                audioSource.dopplerLevel = 0;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0;
                audioSource.volume = 1.0f;

                mSoundChannels[i] = new SoundChannel(audioSource);
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.bypassEffects = true;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassReverbZones = true;
            audioSource.dopplerLevel = 0;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            audioSource.priority = 255;
            audioSource.volume = 0.0f;

            mMusicChannel = new SoundChannel(audioSource);
        }

        private int GetFreeSoundChannel()
        {
            if (mSoundChannels[0] == null)
            {
                InitializeChannels();
            }

            for (int i = 0; i < mSoundChannels.Length; i++)
            {
                if (mSoundChannels[i].Source != null && mSoundChannels[i].Source.isPlaying == false)
                {
                    return i;
                }
            }

            return -1;
        }

        private class SoundChannel
        {
            public AudioSource Source;
            public long Sequence;

            public SoundChannel(AudioSource source)
            {
                Source = source;
                Sequence = 0;
            }
        }
    }
}
