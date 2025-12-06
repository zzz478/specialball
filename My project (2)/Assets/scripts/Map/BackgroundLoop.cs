using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundLoop : MonoBehaviour
{
    [Header("背景适配参数")]
    public Camera mainCamera;
    public float backgroundSpeed = 1f;
    public float parallaxFactor = 0.8f;

    private SpriteRenderer sr;
    private float backgroundWidth;
    private float cameraHalfWidth;
    private Vector3 initialPos;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (mainCamera == null) mainCamera = Camera.main;

        // 1. 强制配置背景层级（优先级最高）
        sr.sortingLayerID = SortingLayer.NameToID("Background"); // 用ID替代名称，避免拼写错误
        sr.sortingOrder = 0;
        transform.position = new Vector3(transform.position.x, transform.position.y, 100f); // Z轴拉满，彻底底层

        // 2. 全屏适配
        FitBackgroundToScreen();

        // 3. 计算背景宽度
        float spriteOriginalWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
        backgroundWidth = spriteOriginalWidth * transform.localScale.x;
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        initialPos = transform.position;
    }

    void Update()
    {
        if (mainCamera == null || GameManager.Instance?.isGameOver == true) return;
        FollowCameraWithParallax();
        CheckAndLoopBackground();
    }

    void FitBackgroundToScreen()
    {
        if (sr == null || !mainCamera.orthographic) return;

        float spriteWidth = sr.sprite.bounds.size.x;
        float spriteHeight = sr.sprite.bounds.size.y;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float scaleX = (cameraWidth / spriteWidth) * 1.1f;
        float scaleY = (cameraHeight / spriteHeight) * 1.1f;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, 100f); // 锁定Z=100
    }

    void FollowCameraWithParallax()
    {
        float targetX = mainCamera.transform.position.x * parallaxFactor;
        float newX = Mathf.Lerp(transform.position.x, targetX, backgroundSpeed * Time.deltaTime);
        transform.position = new Vector3(newX, transform.position.y, 100f); // 强制Z轴不变
    }

    void CheckAndLoopBackground()
    {
        float backgroundLeftEdge = transform.position.x - backgroundWidth / 2;
        float cameraLeftEdge = mainCamera.transform.position.x - cameraHalfWidth;
        if (backgroundLeftEdge < cameraLeftEdge - backgroundWidth * 0.1f)
        {
            transform.position = new Vector3(transform.position.x + backgroundWidth, transform.position.y, 100f);
        }

        float backgroundRightEdge = transform.position.x + backgroundWidth / 2;
        float cameraRightEdge = mainCamera.transform.position.x + cameraHalfWidth;
        if (backgroundRightEdge > cameraRightEdge + backgroundWidth * 0.1f)
        {
            transform.position = new Vector3(transform.position.x - backgroundWidth, transform.position.y, 100f);
        }
    }

    void OnDrawGizmos()
    {
        if (sr == null) return;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position - new Vector3(backgroundWidth / 2, 0, 0), transform.position + new Vector3(backgroundWidth / 2, 0, 0));
    }
}