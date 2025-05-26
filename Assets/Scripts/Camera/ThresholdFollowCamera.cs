using UnityEngine;

public class ThresholdFollowCamera : MonoBehaviour
{
    [Header("目标摄像机")]
    public Transform targetCamera;

    [Header("位置偏移")]
    public Vector3 offsetPosition = Vector3.zero;
    [Header("旋转偏移")]
    public Quaternion offsetRotation = Quaternion.identity;

    [Header("阈值设置")]
    public float positionThreshold = 0.05f;
    public float rotationThreshold = 1f; // 角度阈值

    [Header("平滑时间")]
    public float smoothTime = 0.2f;

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // 计算目标位置和目标旋转
        Vector3 desiredPos = targetCamera.TransformPoint(offsetPosition);
        Quaternion desiredRot = targetCamera.rotation * offsetRotation;

        // 位置阈值检测
        if (Vector3.Distance(transform.position, desiredPos) > positionThreshold)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, desiredPos, ref _velocity, smoothTime);
        }

        // 旋转阈值检测
        float angleDiff = Quaternion.Angle(transform.rotation, desiredRot);
        if (angleDiff > rotationThreshold)
        {
            float t = Mathf.Clamp01(Time.deltaTime / smoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, t);
        }
    }
}