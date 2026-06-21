using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class OptionsManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;
    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;

    private const string MasterParam = "MasterVol";
    private const string MusicParam = "MusicVol";
    private const string SFXParam = "EffectsVol";

    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private DropdownField fpsDropdown;
    private Toggle fullscreenToggle;
    private Button applyButton;
    private Button revertButton;
    private List<Resolution> uniqueResolutions;

    private int selectedResolution;
    private int selectedQuality;
    private int selectedFPS;
    private bool selectedFullscreen;

    private int savedResolution;
    private int savedQuality;
    private int savedFPS;
    private bool savedFullscreen;

    private readonly List<int> fpsOptions = new()
    {
        30,
        60,
        120,
        144,
        240,
        -1
    };

    private void Awake()
    {
        VisualElement root = uiDocument.rootVisualElement;

        resolutionDropdown = root.Q<DropdownField>("resolutionDropdown");
        qualityDropdown = root.Q<DropdownField>("qualityDropdown");
        fpsDropdown = root.Q<DropdownField>("fpsDropdown");
        fullscreenToggle = root.Q<Toggle>("fullscreenToggle");
        applyButton = root.Q<Button>("applyButton");
        revertButton = root.Q<Button>("revertButton");

        masterVolumeSlider = root.Q<Slider>("masterSlider");
        musicVolumeSlider = root.Q<Slider>("musicSlider");
        sfxVolumeSlider = root.Q<Slider>("effectsSlider");

        applyButton.clicked += ApplySettings;
        revertButton.clicked += CancelChanges;

        InitializeResolutions();
        InitializeQuality();
        InitializeFPS();

        LoadSettings();

        float savedMaster = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        ConfigurarSlider(masterVolumeSlider, savedMaster, MasterParam, "MasterVolume");
        ConfigurarSlider(musicVolumeSlider, savedMusic, MusicParam, "MusicVolume");
        ConfigurarSlider(sfxVolumeSlider, savedSFX, SFXParam, "SFXVolume");

    }

    private void ConfigurarSlider(Slider slider, float valorInicial, string parametroMixer, string chavePrefs)
    {
        if (slider == null) return;

        // Garante os limites corretos para a conversão matemática logarítmica
        slider.lowValue = 0.0001f;
        slider.highValue = 1f;
        slider.value = valorInicial;

        // Aplica o volume inicial logo no carregamento
        AtualizarVolumeNoMixer(parametroMixer, valorInicial);

        // Escuta as mudanças do jogador em tempo real
        slider.RegisterValueChangedCallback(evt =>
        {
            float novoValor = evt.newValue;
            AtualizarVolumeNoMixer(parametroMixer, novoValor);

            // Salva a preferência do jogador automaticamente
            PlayerPrefs.SetFloat(chavePrefs, novoValor);
            PlayerPrefs.Save();
        });
    }

    private void AtualizarVolumeNoMixer(string nomeParametro, float valorLinear)
    {
        // A Mágica do Logaritmo: Converte a escala linear (0.0001 a 1) na escala decibéis (-80dB a 0dB)
        float decibeis = Mathf.Log10(valorLinear) * 20;

        audioMixer.SetFloat(nomeParametro, decibeis);
    }

    private static readonly List<Vector2Int> commonResolutions = new()
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    public void InitializeResolutions()
    {
        uniqueResolutions = new List<Resolution>();
        List<string> options = new();

        foreach (Vector2Int res in commonResolutions)
        {
            Resolution resolution = new Resolution
            {
                width = res.x,
                height = res.y
            };

            uniqueResolutions.Add(resolution);
            options.Add($"{res.x} x {res.y}");
        }

        resolutionDropdown.choices = options;

        resolutionDropdown.RegisterValueChangedCallback(evt =>
        {
            selectedResolution =
                resolutionDropdown.choices.IndexOf(evt.newValue);
        });
    }

    public void InitializeQuality()
    {
        List<string> qualityOptions = new(QualitySettings.names);
        if (qualityOptions.Count == 0)
        {
            qualityOptions.Add("Default");
        }

        qualityDropdown.choices = qualityOptions;

        qualityDropdown.RegisterValueChangedCallback(evt =>
        {
            selectedQuality =
                qualityDropdown.choices.IndexOf(evt.newValue);
        });

    }

    public void InitializeFPS()
    {
        List<string> fpsStrings = new();

        foreach (int fps in fpsOptions)
        {
            fpsStrings.Add(fps == -1 ? "Unlimited" : fps.ToString());
        }

        fpsDropdown.choices = fpsStrings;

        fpsDropdown.RegisterValueChangedCallback(evt =>
        {
            selectedFPS =
                fpsOptions[fpsDropdown.choices.IndexOf(evt.newValue)];
        });

        fullscreenToggle.RegisterValueChangedCallback(evt =>
        {
            selectedFullscreen = evt.newValue;
        });

    }

    private void ApplySettings()
    {
        Resolution resolution = uniqueResolutions[selectedResolution];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            selectedFullscreen);

        QualitySettings.SetQualityLevel(selectedQuality);

        Application.targetFrameRate = selectedFPS;

        savedResolution = selectedResolution;
        savedQuality = selectedQuality;
        savedFPS = selectedFPS;
        savedFullscreen = selectedFullscreen;

        SaveSettings();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("Resolution", selectedResolution);
        PlayerPrefs.SetInt("Quality", selectedQuality);
        PlayerPrefs.SetInt("Fullscreen", selectedFullscreen ? 1 : 0);
        PlayerPrefs.SetInt("FPS", selectedFPS);

        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        savedResolution = PlayerPrefs.GetInt(
            "Resolution",
            GetCurrentResolutionIndex());

        savedQuality = PlayerPrefs.GetInt(
            "Quality",
            QualitySettings.GetQualityLevel());

        savedFullscreen = PlayerPrefs.GetInt(
            "Fullscreen",
            Screen.fullScreen ? 1 : 0) == 1;

        savedFPS = PlayerPrefs.GetInt(
            "FPS",
            60);

        selectedResolution = savedResolution;
        selectedQuality = savedQuality;
        selectedFullscreen = savedFullscreen;
        selectedFPS = savedFPS;

        UpdateUI();
        ApplyCurrentSavedSettings();
    }

    private int GetCurrentResolutionIndex()
    {
        Resolution current = Screen.currentResolution;

        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            if (uniqueResolutions[i].width == current.width &&
                uniqueResolutions[i].height == current.height)
            {
                return i;
            }
        }

        return 0;
    }
    private void ApplyCurrentSavedSettings()
    {
        if (uniqueResolutions == null || uniqueResolutions.Count == 0)
            return;

        int resIndex = Mathf.Clamp(savedResolution, 0, uniqueResolutions.Count - 1);
        Resolution resolution = uniqueResolutions[resIndex];

        Screen.SetResolution(
            resolution.width,
            resolution.height,
            savedFullscreen);

        QualitySettings.SetQualityLevel(savedQuality);

        Application.targetFrameRate = savedFPS;
    }

    private void UpdateUI()
    {
        resolutionDropdown.index = Mathf.Clamp(
            selectedResolution,
            0,
            resolutionDropdown.choices.Count - 1);

        qualityDropdown.index = Mathf.Clamp(
            selectedQuality,
            0,
            qualityDropdown.choices.Count - 1);

        fullscreenToggle.value = selectedFullscreen;

        int fpsIndex = fpsOptions.IndexOf(selectedFPS);

        if (fpsIndex < 0)
            fpsIndex = 1;

        fpsDropdown.index = Mathf.Clamp(
            fpsIndex,
            0,
            fpsDropdown.choices.Count - 1);
    }

    private void CancelChanges()
    {
        selectedResolution = savedResolution;
        selectedQuality = savedQuality;
        selectedFullscreen = savedFullscreen;
        selectedFPS = savedFPS;

        UpdateUI();
    }

}