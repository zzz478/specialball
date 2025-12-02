using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCoinPickup : MonoBehaviour
{
    // 当有对象进入金币触发器时调用
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只有Player触碰才加分
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(10); // 调用加分接口，+10分
            Destroy(gameObject); // 收集后销毁金币
        }
    }
}
