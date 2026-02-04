using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("오디오 소스")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("걷기 전용 사운드 채널")]
    public AudioSource walkingSource; // 걷기 루프용

    [Header("볼륨 설정(실외 걷기, 실내 걷기, 뛰기, 글로벌)")]
    [Range(0f, 1f)]
    [SerializeField] private float walkingOutdoorVolumeMultiplier = 0.2f; // 실외 걷기 (더 작게)
    [Range(0f, 1f)]
    [SerializeField] private float walkingIndoorVolumeMultiplier = 0.3f;  // 실내 걷기 (더 크게)
    [Range(0f, 1f)]
    [SerializeField] private float runningVolumeMultiplier = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float globalWalkingVolume = 0.5f;

    [Header("사운드 데이터")]
    public SoundData[] soundDataList; // Inspector에 등록하거나 Resources.LoadAll()로 자동 로딩

    private Dictionary<SoundKey, SoundData> soundDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitSoundDict();

            // 씬이 로드될 때마다 OnSceneLoaded 호출
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bgmSource == null) return;

        if (scene.name == "MainMenu")
        {
            Play(SoundKey.BGM_MainMenu);
        }
        else
        {
            StopBGM(); // 다른 씬 들어가면 BGM 끔
        }
    }

    private void InitSoundDict()
    {
        soundDict = new Dictionary<SoundKey, SoundData>();
        foreach (var data in soundDataList)
        {
            if (!soundDict.ContainsKey(data.soundKey))
                soundDict[data.soundKey] = data;
        }
    }

    public void Play(SoundKey key)
    {
        if (soundDict.TryGetValue(key, out var sound))
        {
            Debug.Log($"[SoundManager] 재생 요청: {key}, clip: {sound.clip}");
            switch (sound.category)
            {
                case SoundCategory.BGM:
                    bgmSource.clip = sound.clip;
                    bgmSource.loop = true;
                    bgmSource.volume = 1f; // 항상 원래 볼륨부터 시작
                    bgmSource.Play();
                    break;
                default:
                    sfxSource.PlayOneShot(sound.clip);
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"[SoundManager] {key} 사운드를 찾을 수 없음.");
        }
    }

    public void StopBGM() => bgmSource.Stop();

    public void StopBGM_Fade(float fadeTime = 1f)
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.DOFade(0f, fadeTime).OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.volume = 1f; // 볼륨 초기화
            });
        }
    }

    public void SetVolume(SoundCategory category, float volume)
    {
        switch (category)
        {
            case SoundCategory.BGM:
                bgmSource.volume = volume;
                break;
            case SoundCategory.SFX:
                sfxSource.volume = volume;
                break;
        }
    }

    //걷기 전용
    public void PlayWalkingLoop(AudioClip clip)
    {
        if (walkingSource.isPlaying && walkingSource.clip == clip) return;

        walkingSource.clip = clip;
        walkingSource.loop = true;
        walkingSource.volume = globalWalkingVolume; // 마스터 볼륨 적용
        walkingSource.Play();
    }

    public void StopWalkingLoop(float fadeTime = 0.3f)
    {
        if (walkingSource.isPlaying)
        {
            walkingSource.DOFade(0f, fadeTime).OnComplete(() =>
            {
                walkingSource.Stop();
                walkingSource.clip = null;
                walkingSource.volume = globalWalkingVolume; // 초기화 시에도 마스터 볼륨 적용
            });
        }
    }

    // 오버로드된 메서드 - 걷기/뛰기 구분하여 볼륨 적용
    public void SwitchWalkingLoop(AudioClip newClip, float fadeTime = 0.2f, bool isRunning = false, bool isIndoor = false)
    {
        if (walkingSource.isPlaying && walkingSource.clip == newClip)
            return;

        // 걷기/뛰기, 실내/실외에 따라 다른 볼륨 적용
        float volumeMultiplier = isRunning ? runningVolumeMultiplier
                               : (isIndoor ? walkingIndoorVolumeMultiplier : walkingOutdoorVolumeMultiplier);

        float targetVolume = globalWalkingVolume * volumeMultiplier;

        if (walkingSource.isPlaying)
        {
            walkingSource.DOFade(0f, fadeTime).OnComplete(() =>
            {
                walkingSource.clip = newClip;
                walkingSource.volume = targetVolume;
                walkingSource.loop = true;
                walkingSource.Play();
            });
        }
        else
        {
            walkingSource.clip = newClip;
            walkingSource.volume = targetVolume;
            walkingSource.loop = true;
            walkingSource.Play();
        }
    }

    // 기존 메서드 호환성 유지
    public void SwitchWalkingLoop(AudioClip newClip, float fadeTime)
    {
        SwitchWalkingLoop(newClip, fadeTime, false);
    }

    public AudioClip GetClip(SoundKey key)
    {
        if (soundDict.TryGetValue(key, out var data))
            return data.clip;

        Debug.LogWarning($"[SoundManager] {key} 사운드 클립을 찾을 수 없습니다.");
        return null;
    }

    // 걷기 소리 볼륨 실시간 조절 메서드들
    public void SetWalkingMasterVolume(float volume)
    {
        globalWalkingVolume = Mathf.Clamp01(volume);
        if (walkingSource.isPlaying)
        {
            walkingSource.volume = globalWalkingVolume;
        }
    }

    public void SetRunningVolumeMultiplier(float multiplier)
    {
        runningVolumeMultiplier = Mathf.Clamp01(multiplier);
    }
}
