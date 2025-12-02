using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 单例：全员统一调用入口
    public static GameManager Instance;

    // 游戏状态（人员A读取：死亡/暂停时禁用输入）
    public bool isGameOver = false;
    public bool isPaused = false;

    // 计分数据（人员C读取：更新UI分数）
    public int currentScore = 0;
    public int highScore = 0;

    void Awake()
    {
        // 单例初始化（避免重复创建）
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject); // 场景切换不销毁

        // 读取历史最高分
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    // 1. 加分接口（自己用：金币收集时调用）
    public void AddScore(int value)
    {
        if (!isGameOver && !isPaused)
        {
            currentScore += value;
            Debug.Log("当前分数：" + currentScore); // 临时日志，后续对接人员C的UI
        }
    }

    // 2. 死亡接口（人员A调用：异色碰撞；自己调用：掉落死亡）
    public void GameOver()
    {
        if (isGameOver) return; // 防止重复触发
        isGameOver = true;

        // 更新最高分
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            Debug.Log("新最高分：" + highScore);
        }

        Debug.Log("游戏结束！"); // 临时日志，后续对接人员C的结算UI
    }

    // 3. 暂停/继续接口（人员C调用：Esc触发）
    public void PauseGame()
    {
        if (isGameOver) return;
        isPaused = true;
        Time.timeScale = 0; // 停止游戏时间
        Debug.Log("游戏暂停");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1; // 恢复游戏时间
        Debug.Log("游戏继续");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
