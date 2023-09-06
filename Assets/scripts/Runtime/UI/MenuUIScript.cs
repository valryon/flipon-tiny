using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIScript : MonoBehaviour
{
    float loadingTimer = 0f;
    [SerializeField] GameObject bufferLogo;

    public Toggle MuteVolumeToggle;
    public Toggle NotifictationToggle;
    public Toggle LivesToggle;
    public Toggle DailyToggle;
    public Toggle EventsToggle;

    private void Start()
    {
        // get mute toggle
        MuteVolumeToggle = GameObject.FindWithTag("Mute").GetComponent<Toggle>();
        if (MuteVolumeToggle != null)
        {
            MuteVolumeToggle.isOn = false;
            MuteVolumeToggle.onValueChanged.AddListener((bool isOn) => { Mute(MuteVolumeToggle.isOn); });
        }

        // get notification toggles
        NotifictationToggle = GameObject.FindWithTag("Notifications").GetComponent<Toggle>();
        if (NotifictationToggle != null)
        {
            NotifictationToggle.isOn = true;
            NotifictationToggle.onValueChanged.AddListener((bool isOn) => { EnableNotifications(NotifictationToggle.isOn); });
        }

        LivesToggle = GameObject.FindWithTag("Lives").GetComponent<Toggle>();
        DailyToggle = GameObject.FindWithTag("Daily").GetComponent<Toggle>();
        EventsToggle = GameObject.FindWithTag("Events").GetComponent<Toggle>();
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
        }
        else
        {
            Debug.Log("Volume Unmuted");
        }
    }

    void EnableNotifications(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Notifs enabled");
            LivesToggle.enabled = true;
            DailyToggle.enabled = true;
            EventsToggle.enabled = true;
}
        else
        {
            Debug.Log("Notifs disabled");
            LivesToggle.enabled = false;
            DailyToggle.enabled = false;
            EventsToggle.enabled = false;
        }
    }
}
