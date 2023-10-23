using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using static UnityEngine.UI.CanvasScaler;
using System.IO;

public class MenuUIScript : MonoBehaviour
{
    float loadingTimer = 0f;
    [SerializeField] GameObject bufferLogo;

    public Toggle muteVolumeToggle;
    public Toggle notifictationToggle;
    public Toggle livesToggle;
    public Toggle dailyToggle;
    public Toggle eventsToggle;
    public Toggle hintsToggle;
    public Toggle colorBlindToggle;

    public AudioMixer mixer;
    public Slider musicSlider;
    public Slider soundSlider;

    public TMP_Dropdown graphicsDropdown;

    private float currentMusicVolume = 0.0f;
    private float currentMusicValue = 1.0f;

    private float currentSoundVolume = 0.0f;
    private float currentSoundValue = 1.0f;

    private bool isTutorialComplete = false;
    private string TUTORIAL_FILENAME = "tutorialData.dat";

    private void Start()
    {
        // LOAD SETTINGS
        LoadSettings();

        // get mute toggle
        if (muteVolumeToggle != null)
        {
            muteVolumeToggle.onValueChanged.AddListener((bool isOn) => { Mute(muteVolumeToggle.isOn); });
        }

        // get slider
        if (soundSlider != null)
        {
            soundSlider.onValueChanged.AddListener((float value) => { SetSoundLevel(soundSlider.value); } );
        }

        // get notification toggles
        if (notifictationToggle != null)
        {
            notifictationToggle.onValueChanged.AddListener((bool isOn) => { EnableNotifications(notifictationToggle.isOn); });
        }

        // get hints toggle
        if (hintsToggle != null)
        {
            hintsToggle.onValueChanged.AddListener((bool isOn) => { EnableHints(hintsToggle.isOn); });
        }

        // get quality dropdowns
        if (graphicsDropdown != null)
        {
            graphicsDropdown.onValueChanged.AddListener((int qualityIndex) => { SetQualityLevel(graphicsDropdown.value); }); 
        }

        // set colorblind toggle
        if (colorBlindToggle != null)
        {
            colorBlindToggle.onValueChanged.AddListener((bool isOn) => { SetColorBlindMode(colorBlindToggle.isOn); });
        }
    }

	private void FixedUpdate()
	{
        SaveSettings();
    }

	// load the game
	public void StartLoad(){
        StartCoroutine(AsyncLoadIntoGame());
    }

    private IEnumerator AsyncLoadIntoGame(){
        AsyncOperation asyncLoad;
        if (isTutorialComplete)
        {
            asyncLoad = SceneManager.LoadSceneAsync("Map_t");
        } else
        {
            asyncLoad = SceneManager.LoadSceneAsync("Tutorial_Entry");
        }
        
        while(!asyncLoad.isDone){
            if(loadingTimer <= 0.75f){
                loadingTimer += Time.deltaTime;
            }else{
                bufferLogo.SetActive(true);
                bufferLogo.transform.GetChild(0).GetComponent<ConstantForce2D>().torque = 15;
            }
            yield return null;
        }
    }

    // mute or set all volume based on toggle value
    void Mute(bool isOn)
    {
        if (isOn) // is muted
        {
            Debug.Log("Volume Muted");
            musicSlider.interactable = false;
            soundSlider.interactable = false;

            musicSlider.value = 0.0001f;
            soundSlider.value = 0.0001f;

            mixer.SetFloat("MusicVolume", -80.0f);
            mixer.SetFloat("SoundVolume", -80.0f);
        }
        else
        {
            Debug.Log("Volume Unmuted");
            musicSlider.interactable = true;
            soundSlider.interactable = true;

            musicSlider.value = currentMusicValue;
            soundSlider.value = currentSoundValue;
            Debug.Log(currentMusicValue);

            mixer.SetFloat("MusicVolume", currentMusicVolume);
            mixer.SetFloat("SoundVolume", currentSoundVolume);
        }
    }

    // enable notifications based on toggle, disable other notification settings
    void EnableNotifications(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Notifs enabled");
            livesToggle.isOn = true;
            dailyToggle.isOn = true;
            eventsToggle.isOn = true;

            livesToggle.interactable = true;
            dailyToggle.interactable = true;
            eventsToggle.interactable = true;
}
        else
        {
            Debug.Log("Notifs disabled");
            livesToggle.isOn = false;
            dailyToggle.isOn = false;
            eventsToggle.isOn = false; 

            livesToggle.interactable = false;
            dailyToggle.interactable = false;
            eventsToggle.interactable = false;
        }
    }

    // enable hints feature based on toggle
    public void EnableHints(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Hints Enabled");
        }
        else
        {
            Debug.Log("Hints Disabled");
        }
    }

    // set quality level in project settings based on dropdown
    public void SetQualityLevel(int qualityIndex)
    {
        Debug.Log(qualityIndex);
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    // control music and sound levels with sliders
    public void SetMusicLevel(float sliderValue)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
        currentMusicVolume = Mathf.Log10(sliderValue) * 20;
        currentMusicValue = sliderValue;
    }

    public void SetSoundLevel(float sliderValue)
    {
        mixer.SetFloat("SoundVolume", Mathf.Log10(sliderValue) * 20);
        currentSoundVolume = Mathf.Log10(sliderValue) * 20;
        currentSoundValue = sliderValue;
    }

    // set colorblind graphics based on toggle
    public void SetColorBlindMode(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("ColorBlind Mode Enabled");
        }
        else
        {
            Debug.Log("ColorBlind Mode Disabled");
        }
    }

    // saving settings
    public void SaveSettings()
    {
        // Sound
        PlayerPrefs.SetInt("MuteVolumePreference", (muteVolumeToggle.isOn ? 1 : 0));
        PlayerPrefs.SetFloat("MusicVolume", currentMusicVolume);
        PlayerPrefs.SetFloat("SoundVolume", currentSoundVolume);
        PlayerPrefs.SetFloat("MusicValue", currentMusicValue);
        PlayerPrefs.SetFloat("SoundValue", currentSoundValue);

        // Notifications
        PlayerPrefs.SetInt("NotificationEnabledPreference", (notifictationToggle.isOn ? 1 : 0));
        PlayerPrefs.SetInt("LivesReplenishedEnabledPreference", (livesToggle.isOn ? 1 : 0));
        PlayerPrefs.SetInt("DailyRewardsEnabledPreference", (dailyToggle.isOn ? 1 : 0));
        PlayerPrefs.SetInt("EventsEnabledPreference", (eventsToggle.isOn ? 1 : 0));

        // Hints
        PlayerPrefs.SetInt("HintsEnabledPreference", (hintsToggle.isOn ? 1 : 0));

        // Graphics
        PlayerPrefs.SetInt("QualitySettingPreference", graphicsDropdown.value);
        PlayerPrefs.SetInt("ColorBlindEnabledPreference", (colorBlindToggle.isOn ? 1 : 0));

        // save
        PlayerPrefs.Save();

        // Tutorial
        string path = Path.Combine(Application.persistentDataPath, TUTORIAL_FILENAME);
        File.WriteAllText(path, isTutorialComplete.ToString());
    }

    public void LoadSettings()
    {
        // load sound preferences/default values
        if (PlayerPrefs.HasKey("MuteVolumePreference"))
        {
            muteVolumeToggle.isOn = PlayerPrefs.GetInt("MuteVolumePreference") == 1;
        }
        else
        {
            muteVolumeToggle.isOn = false;
        }
        if (PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("MusicValue"))
        {
            currentMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
            currentMusicValue = PlayerPrefs.GetFloat("MusicValue");

            if (muteVolumeToggle.isOn)
            {
                mixer.SetFloat("MusicVolume", -80.0f);
                musicSlider.interactable = false;
                musicSlider.value = 0.0f;
            }
            else
            {
                mixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
                musicSlider.interactable = true;
                musicSlider.value = PlayerPrefs.GetFloat("MusicValue");
            }
        }
        else
        {
            if (muteVolumeToggle.isOn)
            {
                mixer.SetFloat("MusicVolume", -80.0f);
                musicSlider.interactable = false;
                musicSlider.value = 0.0f;
            }
            else
            {
                mixer.SetFloat("MusicVolume", 0.0f);
                musicSlider.interactable = true;
                musicSlider.value = 1.0f;
            }
            
        }
        if (PlayerPrefs.HasKey("SoundVolume") && PlayerPrefs.HasKey("SoundValue"))
        {
            currentSoundVolume = PlayerPrefs.GetFloat("SoundVolume");
            currentSoundValue = PlayerPrefs.GetFloat("SoundValue");


            if (muteVolumeToggle.isOn)
            {
                mixer.SetFloat("SoundVolume", -80.0f);
                soundSlider.interactable = false;
                soundSlider.value = 0.0f;
            }
            else
            {
                mixer.SetFloat("SoundVolume", PlayerPrefs.GetFloat("SoundVolume"));
                soundSlider.interactable = true;
                soundSlider.value = PlayerPrefs.GetFloat("SoundValue");
            }
        }
        else
        {
            // default
            if (muteVolumeToggle.isOn)
            {
                mixer.SetFloat("SoundVolume", -80.0f);
                soundSlider.interactable = false;
                soundSlider.value = 0.0f;
            }
            else
            {
                mixer.SetFloat("SoundVolume", 0.0f);
                soundSlider.interactable = true;
                soundSlider.value = 1.0f;
            }
        }

        // notifications
        if (PlayerPrefs.HasKey("NotificationEnabledPreference"))
        {
            notifictationToggle.isOn = PlayerPrefs.GetInt("NotificationEnabledPreference") == 1;
        }
        else
        {
            notifictationToggle.isOn = true;
        }

        //
        EnableNotifications(notifictationToggle.isOn);

        if (PlayerPrefs.HasKey("LivesReplenishedEnabledPreference"))
        {
            livesToggle.isOn = PlayerPrefs.GetInt("LivesReplenishedEnabledPreference") == 1;
        }
        else
        {
            livesToggle.isOn = true;
        }
        if (PlayerPrefs.HasKey("DailyRewardsEnabledPreference"))
        {
            dailyToggle.isOn = PlayerPrefs.GetInt("DailyRewardsEnabledPreference") == 1;
        }
        else
        {
            dailyToggle.isOn = true;
        }
        if (PlayerPrefs.HasKey("EventsEnabledPreference"))
        {
            eventsToggle.isOn = PlayerPrefs.GetInt("EventsEnabledPreference") == 1;
        }
        else
        {
            eventsToggle.isOn = true;
        }


        // hints
        if (PlayerPrefs.HasKey("HintsEnabledPreference"))
        {
            hintsToggle.isOn = PlayerPrefs.GetInt("HintsEnabledPreference") == 1;
        }
        else
        {
            hintsToggle.isOn = true;
        }


        // graphics
        if (PlayerPrefs.HasKey("QualitySettingPreference"))
        {
            graphicsDropdown.value = PlayerPrefs.GetInt("QualitySettingPreference");
        }
        else
        {
            graphicsDropdown.value = 0; // default is high quality
        }

        if (PlayerPrefs.HasKey("ColorBlindEnabledPreference"))
        {
            colorBlindToggle.isOn = PlayerPrefs.GetInt("ColorBlindEnabledPreference") == 1;
        }
        else
        {
            colorBlindToggle.isOn = false;
        }

        // Tutorial
        string path = Path.Combine(Application.persistentDataPath, TUTORIAL_FILENAME);
        if (File.Exists(path))
        {
          string content = File.ReadAllText(path);
          if (bool.TryParse(content, out bool loadedTutorialState))
          {
            isTutorialComplete = loadedTutorialState;
          }
          else
          {
            Debug.LogError("Failed to parse saved tutorial data.");
          }
        }
     }
}

