using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuUIScript : MonoBehaviour
{
    float loadingTimer = 0f;
    [SerializeField] GameObject bufferLogo;

    public Toggle MuteVolumeToggle;
    public Toggle NotifictationToggle;
    public Toggle LivesToggle;
    public Toggle DailyToggle;
    public Toggle EventsToggle;

    public AudioMixer mixer;
    public Slider musicSlider;
    public Slider soundSlider;

    private void Start()
    {
        // get mute toggle
        // MuteVolumeToggle = GameObject.FindWithTag("Mute").GetComponent<Toggle>();
        if (MuteVolumeToggle != null)
        {
            MuteVolumeToggle.isOn = false;
            MuteVolumeToggle.onValueChanged.AddListener((bool isOn) => { Mute(MuteVolumeToggle.isOn); });
        }

        // get notification toggles
        // NotifictationToggle = GameObject.FindWithTag("Notifications").GetComponent<Toggle>();
        if (NotifictationToggle != null)
        {
            NotifictationToggle.isOn = true;
            NotifictationToggle.onValueChanged.AddListener((bool isOn) => { EnableNotifications(NotifictationToggle.isOn); });
        }

        // LivesToggle = GameObject.FindWithTag("Lives").GetComponent<Toggle>();
        // DailyToggle = GameObject.FindWithTag("Daily").GetComponent<Toggle>();
        // EventsToggle = GameObject.FindWithTag("Events").GetComponent<Toggle>();
    }


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

    void Mute(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Volume Muted");
            musicSlider.interactable = false;
            soundSlider.interactable = false;
        }
        else
        {
            Debug.Log("Volume Unmuted");
            musicSlider.interactable = true;
            soundSlider.interactable = true;
        }
    }

    void EnableNotifications(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Notifs enabled");
            LivesToggle.interactable = true;
            DailyToggle.interactable = true;
            EventsToggle.interactable = true;
}
        else
        {
            Debug.Log("Notifs disabled");
            LivesToggle.interactable = false;
            DailyToggle.interactable = false;
            EventsToggle.interactable = false;
        }
    }

    public void SetMusicLevel(float sliderValue)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(sliderValue) * 20);
    }

    public void SetSoundLevel(float sliderValue)
    {
        mixer.SetFloat("SoundVolume", Mathf.Log10(sliderValue) * 20);
    }
}
