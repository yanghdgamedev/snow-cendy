using System;
using System.Collections;
using System.Collections.Generic;
using GameDevelopmentKit.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class AudioManager : SerializedMonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private Dictionary<SoundKey, SoundData> dicSounds;
    [SerializeField] private Dictionary<MusicKey, MusicData> dicMusics;
    
    [SerializeField] private MusicKey currentMusicKey = MusicKey.None;
    [SerializeField] private AudioSource currentMusic;
    [SerializeField] private AudioMixer audioMixer;
    
    private void Awake()
    {
        Instance = this;
    }

    public void PlayMusic(MusicKey key)
    {
        if (!GameData.MusicOn)
        {
            return;
        }
        
        if (dicMusics.TryGetValue(key, out var musicData))
        {
            if (musicData.audioSource == null)
            {
                musicData.audioSource = gameObject.AddComponent<AudioSource>();
                musicData.audioSource.playOnAwake = false;
                musicData.audioSource.loop = true;
                musicData.audioSource.clip = musicData.clip;
                musicData.audioSource.volume = musicData.volume;
                musicData.audioSource.pitch = musicData.pitch;
                musicData.audioSource.reverbZoneMix = 0;
                musicData.audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(musicData.audioMixGroupType.ToString())[0];
            }
            musicData.audioSource.Play();
            currentMusicKey = key;
            currentMusic = musicData.audioSource;
        }
    }
    
    public void StopMusic()
    {
        if (currentMusic != null)
        {
            currentMusicKey = MusicKey.None;
            currentMusic.Stop();
        }
    }
    
    public void MuteMusic()
    {
        if (currentMusic != null)
        {
            currentMusic.mute = true;
        }
    }
    
    public void UnmuteMusic()
    {
        if (currentMusic != null)
        {
            currentMusic.mute = false;
        }
    }

    public void PlaySound(SoundKey soundKey, float delay)
    {
        StartCoroutine(WaitPlaySound(soundKey, delay));
    }
    
    public IEnumerator WaitPlaySound(SoundKey soundKey, float delay)
    {
        yield return HelperScript.WaitForSeconds(delay);
        PlaySound(soundKey);
    }

    public void PlaySound(SoundKey soundKey)
    {
        if (!GameData.SoundOn)
        {
            return;
        }
        if (dicSounds.TryGetValue(soundKey, out var soundData))
        {
            if (Time.time - soundData.lastPlayTime < soundData.duration)
            {
                return;
            }
            if (soundData.audioSources == null)
            {
                soundData.audioSources = new List<AudioSource>();
            }
            
            foreach (var audioSource in soundData.audioSources)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    soundData.lastPlayTime = Time.time;
                    return;
                }
            }

            if (soundData.audioSources.Count < soundData.maxAudioSources)
            {
                var audioSound = gameObject.AddComponent<AudioSource>();
                audioSound.clip = soundData.clips[Random.Range(0, soundData.clips.Length)];
                audioSound.volume = soundData.volume;
                audioSound.pitch = soundData.pitch;
                audioSound.reverbZoneMix = 0;
                if (soundData.randomizePitch)
                {
                    audioSound.pitch = Random.Range(soundData.minPitch, soundData.maxPitch);
                }
                soundData.audioSources.Add(audioSound);
                audioSound.outputAudioMixerGroup = audioMixer.FindMatchingGroups(soundData.audioMixGroupType.ToString())[0];
                audioSound.Play();
                soundData.lastPlayTime = Time.time;
            }
            
        }
    }
}

[Serializable]
public class SoundData
{
    public AudioMixGroupType audioMixGroupType;
    public AudioClip[] clips;
    [Range(0,1)]
    public float volume = 1;
    public bool randomizePitch;
    [Range(-3,3)]
    [ShowIf("@randomizePitch == false")] public float pitch = 1;
    [Range(-3,3)]
    [ShowIf("randomizePitch")] public float minPitch = 0.8f;
    [Range(-3,3)]
    [ShowIf("randomizePitch")] public float maxPitch = 1.2f;
    public List<AudioSource> audioSources;
    public int maxAudioSources = 1;
    public float duration = Single.MinValue;
    public float lastPlayTime = -10;
    private bool NotRandomizePitch => !randomizePitch;
}
[Serializable]
public class MusicData
{
    public AudioMixGroupType audioMixGroupType;
    public AudioClip clip;
    [Range(0,1)]
    public float volume = 1;
    [Range(-3,3)]
    public float pitch = 1;
    public AudioSource audioSource;
}

public enum SoundKey {
    ButtonClick = 0,
    RollingBall = 1,
    Shoot = 2,
    InHole_1 = 3,
    InHole_2 = 4,
    Victory = 5,
    JumpBall = 6,
    LevelComplete = 7,
    IceBreak = 8,
    ShowColor = 9
}

public enum MusicKey
{
    None = -1,
    Default = 0,
}

public enum AudioMixGroupType
{
    Sound = 0,
    Music = 1
}