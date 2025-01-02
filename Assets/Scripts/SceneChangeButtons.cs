using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButtons : MonoBehaviour
{
    public void Title()
    {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
