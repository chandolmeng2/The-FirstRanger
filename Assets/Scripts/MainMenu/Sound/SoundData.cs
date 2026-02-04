using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Sound/SoundData")]
public class SoundData : ScriptableObject
{
    public SoundKey soundKey;
    public AudioClip clip;
    public SoundCategory category; // BGM, SFX
    public SceneType scene;        // 어떤 씬에서 사용하는 사운드인지
}
