using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIScript : MonoBehaviour
{
    float loadingTimer = 0f;
    [SerializeField] GameObject bufferLogo;

    public void StartLoad(){
        StartCoroutine(AsyncLoadIntoGame());
    }

    private IEnumerator AsyncLoadIntoGame(){
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Game");
        
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
}
