using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FloorLoop : MonoBehaviour
{
    #region 可配置参数
    [Header("循环边界参数")]
    public float leftBoundary = -20f;
    public float rightBoundary = 25f;
    [Header("地板尺寸参数")]
    public float floorWidth = 8f;
    public bool keepYPosition = true;
    public bool randomColorOnLoop = true;
    public Sprite floorRed;
    public Sprite floorBlue;
    #endregion

    #region 金币参数
    [Header("金币循环生成参数")]
    public GameObject coinPrefab;
    [Range(0f, 1f)] public float coinSpawnRate = 0.7f;
    public float coinRate100 = 0.5f;
    public float coinRate200 = 0.3f;
    public float coinYMin = 0.5f;
    public float coinYMax = 1.5f;
    public float coinXRange = 1f;
    private GameObject currentCoin;
    #endregion

    #region 私有变量
    private SpriteRenderer sr;
    private float originalY;
    #endregion

    private float GetDynamicCoinRate()
    {
        if (GameManager.Instance == null) return coinSpawnRate;

        float currentRate = coinSpawnRate;
        if (GameManager.Instance.currentScore < 100)
        {
            currentRate = coinSpawnRate;
        }
        else if (GameManager.Instance.currentScore < 200)
        {
            currentRate = coinRate100;
        }
        else
        {
            currentRate = coinRate200;
        }
        return currentRate;
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalY = transform.position.y;

        if (sr == null)
        {
            randomColorOnLoop = false;
            Debug.LogWarning($"地板{gameObject.name}缺少SpriteRenderer");
        }
    }

    void Update()
    {
        bool isPause = GameManager.Instance != null && GameManager.Instance.isPaused;
        if (isPause) return;
        CheckAndLoopFloor();
    }

    // 修复地板循环逻辑（减少随机偏移，避免间隔）
    void CheckAndLoopFloor()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        float playerX = player.transform.position.x;
        if (transform.position.x < playerX - 25f)
        {
            float targetY = keepYPosition ? originalY : transform.position.y;
            // 关键：缩小随机偏移范围（从0.8~1.6→0.8~1.2）
            float minOffset = floorWidth * 0.8f;
            float randomXOffset = Random.Range(minOffset, minOffset * 1.2f); // 减少偏移，避免间隔
            Vector2 newPos = new Vector2(playerX + 20f + randomXOffset, targetY); // 从30→20，缩短瞬移距离

            // 重叠检测
            while (IsPositionOverlap(newPos, minOffset))
            {
                randomXOffset += minOffset * 0.5f; // 减少叠加偏移
                newPos.x = playerX + 20f + randomXOffset;
            }
            transform.position = newPos;

            SpawnCoinOnLoop();
            if (randomColorOnLoop)
                RandomizeFloorColor();
        }
    }

    void SpawnCoinOnLoop()
    {
        if (currentCoin != null)
        {
            Destroy(currentCoin);
            currentCoin = null;
        }

        if (coinPrefab == null || Random.value > GetDynamicCoinRate()) return;

        float coinX = transform.position.x + Random.Range(-coinXRange, coinXRange);
        float coinY = transform.position.y + Random.Range(coinYMin, coinYMax);

        currentCoin = Instantiate(coinPrefab, new Vector2(coinX, coinY), Quaternion.identity);
        currentCoin.transform.localScale = Vector3.one;
        currentCoin.transform.SetParent(null);

        // 设置金币层级
        SpriteRenderer coinSr = currentCoin.GetComponent<SpriteRenderer>();
        if (coinSr != null)
        {
            coinSr.sortingLayerName = "GamePlay";
            coinSr.sortingOrder = 2;
        }
    }

    public void BindCoin(GameObject coin)
    {
        currentCoin = coin;
    }

    bool IsPositionOverlap(Vector2 targetPos, float minDistance)
    {
        FloorLoop[] allFloors = FindObjectsOfType<FloorLoop>();
        foreach (FloorLoop floor in allFloors)
        {
            if (floor == this) continue;
            float distance = Vector2.Distance(targetPos, floor.transform.position);
            if (distance < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    void RandomizeFloorColor()
    {
        if (floorRed == null || floorBlue == null)
        {
            Debug.LogWarning($"地板{gameObject.name}未配置红/蓝Sprite");
            return;
        }

        bool isRed = Random.Range(0, 2) == 0;
        sr.sprite = isRed ? floorRed : floorBlue;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector2(leftBoundary, -10f), new Vector2(leftBoundary, 10f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector2(rightBoundary, -10f), new Vector2(rightBoundary, 10f));
    }
}