using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public float offsetX = 5f; // 相机在玩家前方的偏移量
    public float fixedY = 0f; // 相机固定Y轴高度（避免上下晃动）

    private Camera mainCamera;
    private float cameraHalfWidth; // 相机视野半宽（关键：用于判断地板是否超出视野）

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        // 提前计算相机视野半宽（正交相机）
        if (mainCamera.orthographic)
        {
            cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        }
    }

    void LateUpdate()
    {
        if (player == null || GameManager.Instance?.isGameOver == true) return;

        // 优化跟随逻辑：Y轴固定，X轴在玩家前方offsetX位置
        Vector3 targetPos = new Vector3(player.position.x + offsetX, fixedY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }

    #region 供地图循环调用的方法（核心补充）
    /// <summary>
    /// 判断地板是否超出相机左视野（超出则需要瞬移复用）
    /// </summary>
    public bool IsFloorOutOfLeftView(Vector2 floorPos)
    {
        // 缓冲值2f：避免地板刚出视野就瞬移，视觉更自然
        return floorPos.x < transform.position.x - cameraHalfWidth - 2f;
    }

    /// <summary>
    /// 获取地板瞬移的目标位置（相机右视野外）
    /// </summary>
    public Vector2 GetFloorTeleportPos(float floorWidth)
    {
        // 瞬移到相机右视野外，且X轴随机偏移（避免地板排列太规律）
        float targetX = transform.position.x + cameraHalfWidth + floorWidth * Random.Range(0.8f, 1.2f);
        // Y轴随机（和你原有地图逻辑一致，可根据需要调整范围）
        float randomY = Random.Range(-2f, 2f);
        return new Vector2(targetX, randomY);
    }
    #endregion

    // 可选：编辑器可视化相机视野（方便调试）
    void OnDrawGizmos()
    {
        if (!mainCamera) mainCamera = GetComponent<Camera>();
        if (!mainCamera.orthographic) return;

        // 绘制相机左/右视野边界（红色=左，绿色=右）
        float halfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(transform.position.x - halfWidth, transform.position.y - 5f),
                        new Vector3(transform.position.x - halfWidth, transform.position.y + 5f));
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(transform.position.x + halfWidth, transform.position.y - 5f),
                        new Vector3(transform.position.x + halfWidth, transform.position.y + 5f));
    }
}