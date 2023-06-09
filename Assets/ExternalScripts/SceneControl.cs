using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneControl : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quitting");
        Application.Quit();
    }

    public void ReplayGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
