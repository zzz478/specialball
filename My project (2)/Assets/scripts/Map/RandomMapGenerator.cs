using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMapGenerator : MonoBehaviour
{
    #region 地板参数
    public GameObject floorRedPrefab;
    public GameObject floorBluePrefab;
    public float startX = -40f;
    public int floorCount = 20;
    public float floorWidth = 5f;
    public float floorYMin = -5f;
    public float floorYMax = 1f;
    private float randomFloorY;
    private int lastColorType = -1;
    private int sameColorCount = 0;
    #endregion

    #region 金币参数
    public GameObject coinPrefab;
    [Range(0f, 1f)] public float coinSpawnRate = 0.7f;
    public float coinYMin = 0.5f;
    public float coinYMax = 1.5f;
    public float coinXRange = 1f;
    #endregion

    void Start()
    {
        GenerateRandomMap();
    }

    void GenerateRandomMap()
    {
        float currentX = startX;

        for (int i = 0; i < floorCount; i++)
        {
            // 1. 随机颜色（限制连续同色≤2块）
            int colorType = Random.Range(0, 2);
            if (colorType == lastColorType)
            {
                sameColorCount++;
                if (sameColorCount >= 2)
                {
                    colorType = colorType == 0 ? 1 : 0;
                    sameColorCount = 0;
                }
            }
            else
            {
                sameColorCount = 0;
                lastColorType = colorType;
            }

            // 2. 实例化地板
            GameObject floorPrefab = colorType == 0 ? floorRedPrefab : floorBluePrefab;
            float randomFloorY = Random.Range(floorYMin, floorYMax);
            GameObject floor = Instantiate(floorPrefab, transform);
            floor.transform.position = new Vector2(currentX, randomFloorY);

            // 关键：设置地板渲染层级（前景）
            SpriteRenderer floorSr = floor.GetComponent<SpriteRenderer>();
            if (floorSr != null)
            {
                floorSr.sortingLayerName = "GamePlay";
                floorSr.sortingOrder = 1;
            }

            // 3. 配置地板循环参数
            FloorLoop floorLoop = floor.GetComponent<FloorLoop>();
            if (floorLoop != null)
            {
                floorLoop.keepYPosition = false;
                floorLoop.floorWidth = floorWidth; // 同步地板宽度
            }

            // 4. 生成金币（提高初始概率到90%）
            SpawnCoin(floor, currentX, randomFloorY);

            // 5. 缩小X偏移（解决地图间隔）
            currentX += floorWidth * 0.6f; // 从0.8→0.6，减少地板间距
        }
    }

    // 优化金币生成（直接绑定，避免遍历）
    void SpawnCoin(GameObject floor, float floorX, float floorY)
    {
        // 初始阶段概率提高到90%，保证前期金币充足
        if (coinPrefab == null || Random.value > 0.9f) return;

        float coinX = floorX + Random.Range(-coinXRange, coinXRange);
        float coinY = floorY + Random.Range(coinYMin, coinYMax);

        GameObject coin = Instantiate(coinPrefab, new Vector2(coinX, coinY), Quaternion.identity);
        coin.transform.localScale = Vector3.one;
        coin.transform.SetParent(floor.transform);

        // 设置金币渲染层级
        SpriteRenderer coinSr = coin.GetComponent<SpriteRenderer>();
        if (coinSr != null)
        {
            coinSr.sortingLayerName = "GamePlay";
            coinSr.sortingOrder = 2;
        }

        FloorLoop floorLoop = floor.GetComponent<FloorLoop>();
        if (floorLoop != null)
        {
            floorLoop.BindCoin(coin);
        }
    }
}