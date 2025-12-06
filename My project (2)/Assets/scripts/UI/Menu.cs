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
        if (GameManager.Instance == null) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isPaused)
            {
                menulist?.SetActive(false);
                GameManager.Instance.ResumeGame();
            }

            else
            {
                menulist?.SetActive(true);
                GameManager.Instance.PauseGame();
            }
        }
        if (GameManager.Instance.isGameOver)
        {
            if (deathmenu != null) // 新增空判断
            {
                deathmenu.SetActive(true);
            }
        }
        else
        {
            if (deathmenu != null) // 新增空判断
            {
                deathmenu.SetActive(false);
            }
        }
    }
    public void Return()
    {
        menulist?.SetActive(false); // 加?容错
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
            //GameManager.Instance.isGameOver = false;
            //GameManager.Instance.isPaused = false;
            //GameManager.Instance.currentScore = 0;
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                // 调用PlayerController中新增的ResetPlayerState方法
                player.ResetPlayerState();
            }
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
    public void Exit()
    {
        // 彻底独立执行退出，不依赖任何其他脚本/状态
        Debug.Log("强制执行退出！");
        Application.Quit();
        // 编辑器中强制停止播放
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#endif
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

