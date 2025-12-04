using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 地板循环脚本：超出左边界瞬移到右边界，实现伪无限跑酷
/// 挂载到每一块地板对象上
/// </summary>
[RequireComponent(typeof(BoxCollider2D))] // 确保地板有碰撞体
public class FloorLoop : MonoBehaviour
{
    #region 可配置参数（Inspector面板调整）
    [Header("循环边界参数")]
    [Tooltip("地板超出此X坐标则瞬移（左边界）")]
    public float leftBoundary = -15f;
    [Tooltip("瞬移后的目标X坐标（右边界）")]
    public float rightBoundary = 15f;
    [Tooltip("瞬移后Y轴是否保持原位置（默认true）")]
    public bool keepYPosition = true;
    [Tooltip("瞬移后随机切换地板颜色（红/蓝），可选功能")]
    public bool randomColorOnLoop = true;
    [Tooltip("红色地板Sprite（仅randomColorOnLoop=true时需要）")]
    public Sprite floorRed;
    [Tooltip("蓝色地板Sprite（仅randomColorOnLoop=true时需要）")]
    public Sprite floorBlue;
    #endregion

    #region 私有变量
    private SpriteRenderer sr; // 地板的SpriteRenderer组件
    private float originalY; // 地板初始Y坐标（用于保持高度）
    #endregion

    void Awake()
    {
        // 获取组件引用
        sr = GetComponent<SpriteRenderer>();
        // 记录初始Y坐标（避免瞬移后Y轴偏移）
        originalY = transform.position.y;

        // 容错：若没有SpriteRenderer，关闭随机颜色功能
        if (sr == null)
        {
            randomColorOnLoop = false;
            Debug.LogWarning($"地板{gameObject.name}缺少SpriteRenderer，关闭随机颜色功能");
        }
    }

    void Update()
    {
        // 游戏结束/暂停时停止循环逻辑
        if (GameManager.Instance.isGameOver || GameManager.Instance.isPaused)
            return;

        // 检测是否超出左边界
        CheckAndLoopFloor();
    }

    #region 核心循环逻辑
    /// <summary>
    /// 检测地板位置，超出左边界则瞬移到右边界
    /// </summary>
    void CheckAndLoopFloor()
    {
        if (transform.position.x < leftBoundary)
        {
            // 计算瞬移后的位置（保持Y轴/随机切换Y轴）
            Vector2 newPos = new Vector2(rightBoundary, keepYPosition ? originalY : Random.Range(-3f, -2f));
            transform.position = newPos;

            // 随机切换地板颜色（可选功能）
            if (randomColorOnLoop)
                RandomizeFloorColor();
        }
    }

    /// <summary>
    /// 随机切换地板红/蓝颜色
    /// </summary>
    void RandomizeFloorColor()
    {
        if (floorRed == null || floorBlue == null)
        {
            Debug.LogWarning($"地板{gameObject.name}未配置红/蓝Sprite，跳过颜色切换");
            return;
        }

        // 50%概率切换红/蓝
        bool isRed = Random.Range(0, 2) == 0;
        sr.sprite = isRed ? floorRed : floorBlue;
    }
    #endregion

    #region 辅助调试（可选）
    // 在Scene视图中绘制边界线，方便调试
    void OnDrawGizmos()
    {
        // 左边界（红色线）
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(leftBoundary, -10f), new Vector2(leftBoundary, 10f));

        // 右边界（绿色线）
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(rightBoundary, -10f), new Vector2(rightBoundary, 10f));
    }
    #endregion
}