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

    public void Credits()
    {
        SceneManager.LoadScene("CreditsScene", LoadSceneMode.Single);
    }

    public void Rules()
    {
        SceneManager.LoadScene("RulesScene", LoadSceneMode.Single);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ContinueGame()
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().OnPause();
    }
}
