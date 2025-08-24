using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static MainMenu Instance;


    public void Play()
    {
        AudioManager.instance.StopMusic();  
        AudioManager.instance.PlayGameBGM(); 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadLevel(int level)
    {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayGameBGM(); 
        Physics2D.gravity = Vector2.down * 9.81f; 
        SceneManager.LoadScene($"level{level}");
    }


    public void RestartGame()
    {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayGameBGM();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Retry()
    {
        AudioManager.instance.StopMusic();
        AudioManager.instance.PlayGameBGM();
        Physics2D.gravity = Vector2.down * 9.81f;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        AudioManager.instance.StopMusic(); 
        AudioManager.instance.PlayMainMenuMusic(); 
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Player quit");
    }

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnLoadButton()
    {
        
    }
}
