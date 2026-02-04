using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance; // 싱글턴 패턴

    public AudioSource audioSource;
    public Slider volumeSlider;
    public Dropdown qualityDropdown;

    private void Awake()
    {
        // 싱글턴 패턴 (게임 내에서 하나만 존재)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // UI 요소 초기화
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f); // 저장된 볼륨 값 불러오기
            volumeSlider.onValueChanged.AddListener(SetVolume);
            SetVolume(volumeSlider.value); // 초기 볼륨 설정
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = PlayerPrefs.GetInt("Quality", 2); // 저장된 품질 설정 불러오기
            qualityDropdown.onValueChanged.AddListener(SetQuality);
            SetQuality(qualityDropdown.value); // 초기 품질 설정
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume); // 설정 저장
    }

    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index); // 설정 저장
    }
}
