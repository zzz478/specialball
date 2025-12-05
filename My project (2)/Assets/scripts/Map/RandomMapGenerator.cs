using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 基础版随机地图生成：地板+金币
/// 挂载到空对象MapGenerator上
/// </summary>
public class RandomMapGenerator : MonoBehaviour
{
    #region 地板参数（Inspector配置）
    public GameObject floorRedPrefab; // 红地板预制体
    public GameObject floorBluePrefab; // 蓝地板预制体
    public int floorCount = 20; // 生成地板总数
    public float startX = 0f; // 地板起始X坐标
    public float floorWidth = 16f; // 单块地板宽度（匹配Scale.X=16）
    public float floorY = -3f; // 地板固定Y坐标
    private int lastColorType = -1; // 上一块颜色（0=红，1=蓝）
    private int sameColorCount = 0; // 连续同色计数（≤2块）
    #endregion

    #region 金币参数（Inspector配置）
    public GameObject coinPrefab; // 金币预制体
    [Range(0f, 1f)] public float coinSpawnRate = 0.7f; // 70%概率生成金币
    public float coinYMin = 0.5f; // 金币在地板上方最小高度
    public float coinYMax = 1.5f; // 金币在地板上方最大高度
    public float coinXRange = 1f; // 金币X轴偏移范围
    #endregion

    void Start()
    {
        GenerateRandomMap(); // 游戏开始生成地图
    }

    /// <summary>
    /// 生成随机地板+金币
    /// </summary>
    void GenerateRandomMap()
    {
        float currentX = startX; // 当前生成位置X

        for (int i = 0; i < floorCount; i++)
        {
            // 1. 随机选地板颜色（限制连续同色≤2块）
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

            // 2. 生成地板
            GameObject floorPrefab = colorType == 0 ? floorRedPrefab : floorBluePrefab;
            GameObject floor = Instantiate(floorPrefab, transform);
            floor.transform.position = new Vector2(currentX, floorY);

            // 3. 给地板赋值循环参数
            FloorLoop floorLoop = floor.GetComponent<FloorLoop>();
            if (floorLoop != null)
            {
                floorLoop.leftBoundary = -15f;
                floorLoop.rightBoundary = 15f;
                floorLoop.keepYPosition = true;
            }

            // 4. 随机生成金币
            SpawnCoin(currentX);

            // 5. 下一块地板X偏移
            currentX += floorWidth;
        }
    }

    /// <summary>
    /// 在指定地板位置生成金币
    /// </summary>
    void SpawnCoin(float floorX)
    {
        if (coinPrefab == null || Random.value > coinSpawnRate) return;

        float coinX = floorX + Random.Range(-coinXRange, coinXRange);
        float coinY = floorY + Random.Range(coinYMin, coinYMax);

        GameObject coin = Instantiate(coinPrefab, transform);
        coin.transform.position = new Vector2(coinX, coinY);

        // 金币随地板循环
        FloorLoop coinLoop = coin.AddComponent<FloorLoop>();
        coinLoop.leftBoundary = -15f;
        coinLoop.rightBoundary = 15f;
        coinLoop.keepYPosition = true;
        coinLoop.randomColorOnLoop = false;
    }
}