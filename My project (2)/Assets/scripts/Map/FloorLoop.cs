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

    // 全局保底计数（修复静态变量混乱问题）
    private static int _globalNoCoinCount = 0;
    [Header("金币保底配置（直接在编辑器调）")]
    public int maxNoCoinFloor = 2; // 连续N块无金币强制生成
    public float minCoinRate = 0.4f; // 最低生成概率
    #endregion

    #region 私有变量
    private SpriteRenderer sr;
    private float originalY;
    #endregion

    private float GetDynamicCoinRate()
    {
        if (GameManager.Instance == null) return coinSpawnRate;

        int currentScore = GameManager.Instance.currentScore;
        float currentRate = coinSpawnRate;

        // 平缓衰减：140分概率≈0.46，避免骤降
        if (currentScore < 100)
        {
            currentRate = Mathf.Lerp(coinSpawnRate, 0.55f, currentScore / 100f);
        }
        else if (currentScore < 200)
        {
            currentRate = Mathf.Lerp(0.55f, 0.4f, (currentScore - 100) / 100f);
        }
        else
        {
            currentRate = 0.4f;
        }
        return Mathf.Max(currentRate, minCoinRate);
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

    // 完全保留原有地图循环逻辑
    void CheckAndLoopFloor()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        float playerX = player.transform.position.x;
        if (transform.position.x < playerX - 25f)
        {
            float targetY = keepYPosition ? originalY : transform.position.y;
            float minOffset = floorWidth * 0.8f;
            float randomXOffset = Random.Range(minOffset, minOffset * 1.2f);
            Vector2 newPos = new Vector2(playerX + 20f + randomXOffset, targetY);

            while (IsPositionOverlap(newPos, minOffset))
            {
                randomXOffset += minOffset * 0.5f;
                newPos.x = playerX + 20f + randomXOffset;
            }
            transform.position = newPos;

            SpawnCoinOnLoop();
            if (randomColorOnLoop)
                RandomizeFloorColor();
        }
    }

    // 修复后的金币生成逻辑
    void SpawnCoinOnLoop()
    {
        if (currentCoin != null)
        {
            Destroy(currentCoin);
            currentCoin = null;
        }

        if (coinPrefab == null)
        {
            Debug.LogWarning($"地板{gameObject.name}未配置金币预制体");
            _globalNoCoinCount++;
            return;
        }

        float dynamicRate = GetDynamicCoinRate();
        bool forceSpawn = _globalNoCoinCount >= maxNoCoinFloor;
        bool shouldSpawn = Random.value <= dynamicRate || forceSpawn;

        // 调试日志，可查看生成状态
        Debug.Log($"【金币】分数:{GameManager.Instance.currentScore} | 概率:{dynamicRate:F2} | 连续无金币:{_globalNoCoinCount} | 强制生成:{forceSpawn}");

        if (shouldSpawn)
        {
            float coinX = transform.position.x + Random.Range(-coinXRange, coinXRange);
            float coinY = transform.position.y + Random.Range(coinYMin, coinYMax);
            currentCoin = Instantiate(coinPrefab, new Vector2(coinX, coinY), Quaternion.identity);
            currentCoin.transform.localScale = Vector3.one;
            currentCoin.transform.SetParent(null);

            SpriteRenderer coinSr = currentCoin.GetComponent<SpriteRenderer>();
            if (coinSr != null)
            {
                coinSr.sortingLayerName = "GamePlay";
                coinSr.sortingOrder = 2;
            }
            _globalNoCoinCount = 0;
        }
        else
        {
            _globalNoCoinCount++;
        }
    }

    public void BindCoin(GameObject coin)
    {
        currentCoin = coin;
        _globalNoCoinCount = 0;
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