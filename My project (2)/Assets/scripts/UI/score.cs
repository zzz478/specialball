using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Score : MonoBehaviour
{
    public Text scoreText;
    public Text highscoreText;

    // Start 在场景开始时自动调用
    void Start()
    {
        // 立即更新一次
        UpdateScoreUI();
    }

    // Update 每帧调用
    void Update()
    {
        // 每帧都更新（简单粗暴，适合测试）
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"当前分数：{GameManager.Instance.currentScore}";
        }

        if (highscoreText != null)
        {
            highscoreText.text = $"最高得分：{GameManager.Instance.highScore}";  // 修正这里
        }
    }
}
