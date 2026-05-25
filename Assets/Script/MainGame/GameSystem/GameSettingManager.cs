using System;
using UnityEngine;
using UnityEngine.Audio;

// Model: 管理遊戲設置的數據和邏輯
public class GameSettingManager : MonoBehaviour
{
    public event Action<float> OnVolumeChanged;
    public event Action<int> OnQualityChanged;
    public event Action<int> OnResolutionChanged;
    public event Action<bool> OnFullscreenChanged;

    public float Volume { get; private set; } = 1f;
    public int QualityIndex { get; private set; } = 2;
    public int ResolutionIndex { get; private set; } = 0;
    public bool IsFullscreen { get; private set; } = true;

    public Resolution[] Resolutions { get; private set; }

    private readonly AudioMixer _audioMixer;


    public GameSettingManager(AudioMixer audioMixer)
    {
        _audioMixer = audioMixer;
    }
    private void Awake()
    {
        InitializeResolutions();
    }
    private void InitializeResolutions()
    {
        Resolutions = Screen.resolutions;
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutionIndex < 0 || resolutionIndex >= Resolutions.Length)
        {
            Debug.LogError("Invalid resolution index: " + resolutionIndex);
            return;
        }
        Resolution selectedResolution = Resolutions[resolutionIndex];
        Screen.SetResolution(
            selectedResolution.width, 
            selectedResolution.height, 
            Screen.fullScreen);

        OnResolutionChanged?.Invoke(resolutionIndex);
        Debug.Log(
            "Setting resolution to: " 
            + selectedResolution.width 
            + " x " 
            + selectedResolution.height);
    }

    public void SetVolume(float value)
    {
        Volume = value;
        _audioMixer.SetFloat("Volume", value);
        OnVolumeChanged?.Invoke(value);
        Debug.Log("Setting volume to: " + value);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        OnQualityChanged?.Invoke(qualityIndex);
        Debug.Log("Setting quality to: " + QualitySettings.names[qualityIndex]);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        OnFullscreenChanged?.Invoke(isFullscreen);
        Debug.Log("Setting fullscreen to: " + isFullscreen);
    }


    public int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < Resolutions.Length; i++)
        {
            if (Resolutions[i].width == Screen.currentResolution.width &&
                Resolutions[i].height == Screen.currentResolution.height)
            {
                return i;
            }
        }
        return 0; 
    }
}
