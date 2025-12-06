using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject menulist;
    public GameObject deathmenu;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isPaused)
            {
                menulist.SetActive(false);
                GameManager.Instance.ResumeGame();
            }

            else
            {
                menulist.SetActive(true);
                GameManager.Instance.PauseGame();
            }
        }
        if (GameManager.Instance.isGameOver)
        {
            deathmenu.SetActive(true);
        }
        else
        {
            deathmenu.SetActive(false);
        }
    }
    public void Return()
    {
        menulist.SetActive(false);
        GameManager.Instance.ResumeGame();
    }
    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameOver = false;
            GameManager.Instance.isPaused = false;
            GameManager.Instance.currentScore = 0;
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
    public void Exit()
    {
        Application.Quit();
    }
    public void Back()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void Next()
    {
        Restart();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }
}

