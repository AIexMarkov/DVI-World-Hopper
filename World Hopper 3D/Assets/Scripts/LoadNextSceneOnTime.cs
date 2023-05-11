using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextSceneOnTime : MonoBehaviour
{
    [SerializeField]
    private float SecondsForMenuLoad = 5f;

    void Start()
    {
        StartCoroutine(LoadSceneAfterSeconds());
    }

    public void SkipIntro()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(1);
    }
    
    IEnumerator LoadSceneAfterSeconds()
    {
        yield return new WaitForSeconds(SecondsForMenuLoad);
        SceneManager.LoadScene(1);
    }
}
