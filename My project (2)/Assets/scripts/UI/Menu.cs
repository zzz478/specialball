using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // 1. 加[SerializeField]，方便在Inspector赋值 + 空值提示
    [SerializeField] private GameObject menulist;
    [SerializeField] private GameObject deathmenu;

    void Update()
    {
        // 全局空值保护：先检查GameManager单例是否存在
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("Menu.cs：GameManager单例未初始化！请检查场景中是否挂载GameManager");
            return;
        }

        // Esc暂停/继续逻辑（加menulist空值保护）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isPaused)
            {
                if (menulist != null) menulist.SetActive(false);
                GameManager.Instance.ResumeGame();
            }
            else
            {
                if (menulist != null) menulist.SetActive(true);
                GameManager.Instance.PauseGame();
            }
        }

        // 死亡菜单逻辑（加deathmenu空值保护）→ 这是第29行核心修复！
        if (GameManager.Instance.isGameOver)
        {
            if (deathmenu != null) deathmenu.SetActive(true);
        }
        else
        {
            if (deathmenu != null) deathmenu.SetActive(false);
        }
    }

    public void Return()
    {
        if (menulist != null) menulist.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void Restart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame(); // 直接调用GameManager的重置方法，更规范
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            // 单例为空时的兜底重置
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Exit()
    {
        Debug.Log("强制执行退出！");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void Back()
    {
        // 加场景索引保护，避免越界
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentIndex > 0)
        {
            SceneManager.LoadScene(currentIndex - 1);
        }
        else
        {
            Debug.LogWarning("Menu.cs：已经是第一个场景，无法返回");
        }
    }

    public void Next()
    {
        // 加场景索引保护，避免越界
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentIndex < SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(currentIndex + 1);
        }
        else
        {
            Debug.LogWarning("Menu.cs：已经是最后一个场景，无法前进");
        }
    }
}