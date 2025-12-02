using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Update()
    {
        // 按T键：手动触发死亡接口（测试GameOver()）
        if (Input.GetKeyDown(KeyCode.T))
        {
            GameManager.Instance.GameOver();
        }

        // 按P键：手动触发加分接口（测试AddScore()）
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameManager.Instance.AddScore(10); // 每次加10分，和金币一致
        }

        // 按Esc键：测试暂停/继续接口
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isPaused)
                GameManager.Instance.ResumeGame();
            else
                GameManager.Instance.PauseGame();
        }
    }
}
