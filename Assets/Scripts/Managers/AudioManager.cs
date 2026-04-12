using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace DungeonScavenger.Core
{
    /// <summary>
    /// Professional audio manager with sound pooling, mixer integration, and volume persistence.
    /// Follows Singleton pattern for global access.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #endregion

        #region Inspector Fields

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Audio Sources (Pool)")]
        [SerializeField] private int poolSize = 20;
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource uiSource;

        [Header("Volume Settings")]
        [SerializeField] private float defaultMasterVolume = 0.8f;
        [SerializeField] private float defaultSFXVolume = 0.8f;
        [SerializeField] private float defaultMusicVolume = 0.6f;
        [SerializeField] private float defaultUIVolume = 0.9f;

        [Header("Default Sounds")]
        [SerializeField] private AudioClip defaultPickupSound;
        [SerializeField] private AudioClip defaultDamageSound;
        [SerializeField] private AudioClip defaultButtonClick;
        [SerializeField] private AudioClip defaultInventoryOpen;
        [SerializeField] private AudioClip defaultInventoryClose;

        [Header("Debug")]
        [SerializeField] private bool logAudioPlayback = false;

        #endregion

        #region Private Data

        private List<AudioSource> sfxSourcePool = new List<AudioSource>();
        private int currentPoolIndex = 0;

        // Volume PlayerPrefs keys
        private const string MASTER_VOLUME_KEY = "MasterVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string UI_VOLUME_KEY = "UIVolume";

        #endregion

        #region Initialization

        private void InitializeAudioManager()
        {
            // Create audio source pool for SFX
            CreateSFXPool();

            // Setup music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Setup UI source if not assigned
            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UISource");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.playOnAwake = false;
            }

            // Route to mixer groups
            RouteToMixer();

            // Load saved volumes
            LoadVolumeSettings();

            Debug.Log("[AudioManager] Initialized with mixer routing and sound pooling");
        }

        private void CreateSFXPool()
        {
            GameObject poolParent = new GameObject("SFX_Pool");
            poolParent.transform.SetParent(transform);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject sourceObj = new GameObject($"SFX_Source_{i}");
                sourceObj.transform.SetParent(poolParent.transform);

                AudioSource source = sourceObj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxSourcePool.Add(source);
            }

            Debug.Log($"[AudioManager] Created SFX pool with {poolSize} sources");
        }

        private void RouteToMixer()
        {
            if (audioMixer == null)
            {
                Debug.LogWarning("[AudioManager] No Audio Mixer assigned!");
                return;
            }

            // Route SFX pool
            foreach (var source in sfxSourcePool)
            {
                source.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
            }

            // Route music
            if (musicSource != null)
                musicSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Music")[0];

            // Route UI
            if (uiSource != null)
                uiSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("UI")[0];
        }

        #endregion

        #region SFX Playback (Pooled)

        /// <summary>
        /// Plays a sound effect using the audio source pool.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                Debug.LogWarning("[AudioManager] Attempted to play null AudioClip");
                return;
            }

            // Get next available source from pool
            AudioSource source = GetNextPooledSource();

            // Configure and play
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.Play();

            if (logAudioPlayback)
                Debug.Log($"[AudioManager] Playing SFX: {clip.name} (Volume: {volume}, Pitch: {pitch})");
        }

        /// <summary>
        /// Plays a sound effect at a specific world position.
        /// </summary>
        public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        /// <summary>
        /// Plays a one-shot sound on the UI channel.
        /// </summary>
        public void PlayUISound(AudioClip clip, float volume = 1f)
        {
            if (clip == null || uiSource == null) return;

            uiSource.PlayOneShot(clip, volume);
        }

        private AudioSource GetNextPooledSource()
        {
            // Simple round-robin pool selection
            AudioSource source = sfxSourcePool[currentPoolIndex];
            currentPoolIndex = (currentPoolIndex + 1) % sfxSourcePool.Count;

            // If source is busy, find a free one
            if (source.isPlaying)
            {
                foreach (var s in sfxSourcePool)
                {
                    if (!s.isPlaying)
                    {
                        return s;
                    }
                }
            }

            return source;
        }

        #endregion

        #region Music Playback

        /// <summary>
        /// Plays background music with optional crossfade.
        /// </summary>
        public void PlayMusic(AudioClip musicClip, float fadeInTime = 1f, bool loop = true)
        {
            if (musicSource == null || musicClip == null) return;

            musicSource.clip = musicClip;
            musicSource.loop = loop;
            musicSource.Play();

            // TODO: Implement fade in

            if (logAudioPlayback)
                Debug.Log($"[AudioManager] Playing music: {musicClip.name}");
        }

        /// <summary>
        /// Stops background music with optional fadeout.
        /// </summary>
        public void StopMusic(float fadeOutTime = 1f)
        {
            if (musicSource == null) return;

            musicSource.Stop();

            if (logAudioPlayback)
                Debug.Log("[AudioManager] Stopped music");
        }

        /// <summary>
        /// Sets music volume (0-1 range).
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            if (musicSource != null)
                musicSource.volume = volume;
        }

        #endregion

        #region Volume Control (with Mixer Integration)

        /// <summary>
        /// Sets the master volume via mixer.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            SetMixerVolume("MasterVolume", volume);
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volume);
        }

        /// <summary>
        /// Sets the SFX volume via mixer.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            SetMixerVolume("SFXVolume", volume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
        }

        /// <summary>
        /// Sets the music volume via mixer.
        /// </summary>
        public void SetMusicVolumeMixer(float volume)
        {
            SetMixerVolume("MusicVolume", volume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        }

        /// <summary>
        /// Sets the UI volume via mixer.
        /// </summary>
        public void SetUIVolume(float volume)
        {
            SetMixerVolume("UIVolume", volume);
            PlayerPrefs.SetFloat(UI_VOLUME_KEY, volume);
        }

        private void SetMixerVolume(string parameterName, float volume)
        {
            if (audioMixer == null) return;

            // Convert linear volume (0-1) to decibels (-80 to 0)
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            audioMixer.SetFloat(parameterName, dB);
        }

        private void LoadVolumeSettings()
        {
            float masterVol = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);
            float uiVol = PlayerPrefs.GetFloat(UI_VOLUME_KEY, defaultUIVolume);

            SetMasterVolume(masterVol);
            SetSFXVolume(sfxVol);
            SetMusicVolumeMixer(musicVol);
            SetUIVolume(uiVol);

            Debug.Log($"[AudioManager] Loaded volumes - Master: {masterVol}, SFX: {sfxVol}, Music: {musicVol}, UI: {uiVol}");
        }

        /// <summary>
        /// Saves all current volume settings.
        /// </summary>
        public void SaveVolumeSettings()
        {
            PlayerPrefs.Save();
            Debug.Log("[AudioManager] Volume settings saved");
        }

        #endregion

        #region Convenience Methods

        /// <summary>
        /// Plays the default pickup sound.
        /// </summary>
        public void PlayPickupSound()
        {
            PlaySFX(defaultPickupSound);
        }

        /// <summary>
        /// Plays the default damage sound.
        /// </summary>
        public void PlayDamageSound()
        {
            PlaySFX(defaultDamageSound);
        }

        /// <summary>
        /// Plays button click sound.
        /// </summary>
        public void PlayButtonClick()
        {
            PlayUISound(defaultButtonClick);
        }

        /// <summary>
        /// Plays inventory open sound.
        /// </summary>
        public void PlayInventoryOpen()
        {
            PlayUISound(defaultInventoryOpen);
        }

        /// <summary>
        /// Plays inventory close sound.
        /// </summary>
        public void PlayInventoryClose()
        {
            PlayUISound(defaultInventoryClose);
        }

        #endregion

        #region Public Properties

        public float MasterVolume => PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
        public float SFXVolume => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
        public float MusicVolume => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, defaultMusicVolume);

        #endregion
    }
}