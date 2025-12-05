using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public float offsetX = 5f; // 相机在玩家前方

    void LateUpdate()
    {
        if (player == null || GameManager.Instance.isGameOver) return;
        Vector3 targetPos = new Vector3(player.position.x + offsetX, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}