using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

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

    public AudioMixer mixer;
    public Slider musicSlider;
    public Slider soundSlider;

    public TMP_Dropdown graphicsDropdown;

    private float currentMusicVolume;
    private float currentSoundVolume;

    private void Start()
    {
        // get mute toggle
        if (muteVolumeToggle != null)
        {
            muteVolumeToggle.isOn = false;
            muteVolumeToggle.onValueChanged.AddListener((bool isOn) => { Mute(muteVolumeToggle.isOn); });
        }

        // get notification toggles
        if (notifictationToggle != null)
        {
            notifictationToggle.isOn = true;
            notifictationToggle.onValueChanged.AddListener((bool isOn) => { EnableNotifications(notifictationToggle.isOn); });
        }

        // get hints toggle
        if (hintsToggle != null)
        {
            hintsToggle.isOn = false;
            hintsToggle.onValueChanged.AddListener((bool isOn) => { EnableHints(hintsToggle.isOn); });
        }

        // get quality dropdown

        if (graphicsDropdown != null)
        {
            graphicsDropdown.onValueChanged.AddListener((int qualityIndex) => { SetQualityLevel(graphicsDropdown.value); }); 
        }
    }

    // load the game
    public void StartLoad(){
        StartCoroutine(AsyncLoadIntoGame());
    }

    private IEnumerator AsyncLoadIntoGame(){
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Map");
        
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
        if (isOn)
        {
            Debug.Log("Volume Muted");
            musicSlider.interactable = false;
            soundSlider.interactable = false;

            mixer.SetFloat("MusicVolume", -80.0f);
            mixer.SetFloat("SoundVolume", -80.0f);
        }
        else
        {
            Debug.Log("Volume Unmuted");
            musicSlider.interactable = true;
            soundSlider.interactable = true;

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
            livesToggle.interactable = true;
            dailyToggle.interactable = true;
            eventsToggle.interactable = true;
}
        else
        {
            Debug.Log("Notifs disabled");
            livesToggle.interactable = false;
            dailyToggle.interactable = false;
            eventsToggle.interactable = false;
        }
    }

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
    }

    public void SetSoundLevel(float sliderValue)
    {
        mixer.SetFloat("SoundVolume", Mathf.Log10(sliderValue) * 20);
        currentSoundVolume = Mathf.Log10(sliderValue) * 20;
    }
}
