using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void StartButton()
    {
        SceneManager.LoadScene(2);
    }

    public void OptionButton()
    {

    }

    public void QuitButton()
    {
        Application.Quit();
    }
}