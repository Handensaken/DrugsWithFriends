using UnityEngine;
using UnityEngine.Audio;

public class SetVolume : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioMixerGroup audioGroup;
    
    public void SetAudioVolume(float volume)
    {
        audioMixer.SetFloat(audioGroup.ToString(), Mathf.Log10(volume) * 20);
    }
}
