using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 新增分档速度参数（每100分一档）
    [Header("玩家移动速度分档（每100分一档）")]
    public float baseRunSpeed = 5f; // 初始速度（0-100分）
    public float runSpeed100 = 8f;  // 100分档位上限
    public float runSpeed200 = 12f; // 200分及以上上限（全局最大）
    public float maxRunSpeed = 15f; // 速度上限（避免过快）
    private float currentRunSpeed; // 当前实际速度
    [Header("速降速度分档（每100分一档）")]
    public float baseDescendGravity = 2f; // 初始速降重力（0-100分）
    public float descendGravity100 = 2.8f; // 100分档位上限
    public float descendGravity200 = 3.5f; // 200分及以上上限（全局最大）

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
        // 修复单例初始化逻辑（DontDestroyOnLoad放在正确位置）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 仅单例对象保留
            // 读取历史最高分
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        else
        {
            Destroy(gameObject);
        }
        currentRunSpeed = baseRunSpeed; // 初始化速度
    }

    //获取当前应有的移动速度
    public float GetCurrentRunSpeed()
    {
        if (currentScore < 100)
        {
            // 0-100分：从baseRunSpeed线性增长到runSpeed100
            currentRunSpeed = Mathf.Lerp(baseRunSpeed, runSpeed100, currentScore / 100f);
        }
        else if (currentScore < 200)
        {
            // 100-200分：从runSpeed100线性增长到runSpeed200
            currentRunSpeed = Mathf.Lerp(runSpeed100, runSpeed200, (currentScore - 100) / 100f);
        }
        else
        {
            // 200分及以上：锁定全局上限
            currentRunSpeed = runSpeed200;
        }
        // 最终限制不超过maxRunSpeed（兼容原有上限参数）
        currentRunSpeed = Mathf.Min(currentRunSpeed, maxRunSpeed);
        return currentRunSpeed;
    }

    // 新增：获取当前档位的速降重力
    public float GetCurrentDescendGravity()
    {
        float currentDescendGravity = baseDescendGravity;
        if (currentScore < 100)
        {
            currentDescendGravity = Mathf.Lerp(baseDescendGravity, descendGravity100, currentScore / 100f);
        }
        else if (currentScore < 200)
        {
            currentDescendGravity = Mathf.Lerp(descendGravity100, descendGravity200, (currentScore - 100) / 100f);
        }
        else
        {
            currentDescendGravity = descendGravity200;
        }
        return currentDescendGravity;
    }

    //游戏重置接口（Restart时调用，恢复初始状态）
    public void ResetGame()
    {
        isGameOver = false;
        isPaused = false;
        currentScore = 0;
        Time.timeScale = 1; // 强制恢复时间缩放，避免地板循环卡顿
        currentRunSpeed = baseRunSpeed; // 重置速度
        Debug.Log("游戏状态已重置！");
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
        Time.timeScale = 1;

        Debug.LogError("GameOver被触发！调用栈：" + System.Environment.StackTrace);

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
}
