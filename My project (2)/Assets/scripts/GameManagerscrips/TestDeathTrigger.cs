using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDeathTrigger : MonoBehaviour
{
    // 当有对象进入触发器时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有Player掉落才触发死亡
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.GameOver(); // 调用死亡接口
        }
    }
}
