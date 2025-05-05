using UnityEngine;

public class WorldSpaceCameraController : MonoBehaviour
{
    private class CameraState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        // 依然对水平平移做旋转处理，y 坐标由跳跃逻辑单独处理
        public void Translate(Vector3 translation)
        {
            var rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;
            x += rotatedTranslation.x;
            z += rotatedTranslation.z;
        }
    }

    const float MouseSensitivityMultiplier = 0.03f;

    private CameraState _targetCameraState = new CameraState();

    [Header("移动设置")]
    [Tooltip("平移的指数加速因子，可通过鼠标滚轮控制")]
    public float boost = 0.1f;

    [Tooltip("水平移动速度")]
    public float moveSpeed = 2.0f;

    [Header("跳跃设置")]
    [Tooltip("跳跃时的向上初始速度")]
    public float jumpForce = 5.0f;

    [Tooltip("重力加速度度")]
    public float gravity = 9.8f;

    [Header("旋转设置")]
    [Tooltip("旋转灵敏度的乘数")]
    public float mouseSensitivity = 60.0f;

    [Tooltip("X = 鼠标位置变化。Y = 相机旋转的乘数")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(1f, 2.5f));

    [Tooltip("是否反转Y轴的鼠标输入")]
    public bool invertY = true;

    [Header("平滑设置")]
    [Tooltip("位置平滑时间，时间越长越平滑")]
    public float positionSmoothTime = 0.1f;

    [Tooltip("旋转平滑时间，时间越长越平滑")]
    public float rotationSmoothTime = 0.05f;

    private Vector3 _positionSmoothVelocity;
    private float _yawSmoothVelocity;
    private float _pitchSmoothVelocity;
    private float _rollSmoothVelocity;

    private bool _isJumping;
    private float _verticalVelocity;
    private float _groundY;

    void OnEnable()
    {
        _targetCameraState.SetFromTransform(transform);
        _groundY = transform.position.y;
    }

    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            direction += Vector3.forward * moveSpeed;
        if (Input.GetKey(KeyCode.S))
            direction += Vector3.back * moveSpeed;
        if (Input.GetKey(KeyCode.A))
            direction += Vector3.left * moveSpeed;
        if (Input.GetKey(KeyCode.D))
            direction += Vector3.right * moveSpeed;
        // 跳跃单独处理 y 坐标
        return direction;
    }

    Vector2 GetInputLookRotation()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    bool IsBoostPressed()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    bool IsCameraRotationAllowed()
    {
        return Input.GetMouseButton(1);
    }

    bool IsRightMouseButtonDown()
    {
        return Input.GetMouseButtonDown(1);
    }

    bool IsRightMouseButtonUp()
    {
        return Input.GetMouseButtonUp(1);
    }

    void Update()
    {
        // 右键按下时锁定光标
        if (IsRightMouseButtonDown())
            Cursor.lockState = CursorLockMode.Locked;

        // 右键释放时解锁光标
        if (IsRightMouseButtonUp())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        // 旋转处理
        if (IsCameraRotationAllowed())
        {
            var mouseMovement = GetInputLookRotation() * (MouseSensitivityMultiplier * mouseSensitivity);
            if (invertY)
                mouseMovement.y = -mouseMovement.y;
            var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
            _targetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            _targetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // 水平平移处理
        var translation = GetInputTranslationDirection() * Time.deltaTime;
        if (IsBoostPressed())
            translation *= 10.0f;
        boost += Input.mouseScrollDelta.y * 0.01f;
        translation *= Mathf.Pow(2.0f, boost);
        _targetCameraState.Translate(translation);

        // 跳跃处理
        float jumpTranslation = 0.0f;
        if (Input.GetKeyDown(KeyCode.Space) && !_isJumping)
        {
            _verticalVelocity = jumpForce;
            _isJumping = true;
        }
        if (_isJumping)
        {
            _verticalVelocity -= gravity * Time.deltaTime;
            jumpTranslation = _verticalVelocity * Time.deltaTime;
            if (_targetCameraState.y + jumpTranslation < _groundY)
            {
                jumpTranslation = _groundY - _targetCameraState.y;
                _verticalVelocity = 0;
                _isJumping = false;
            }
        }
        _targetCameraState.y += jumpTranslation;

        // 使用 SmoothDamp 平滑过渡位置
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(_targetCameraState.x, _targetCameraState.y, _targetCameraState.z);
        Vector3 smoothPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref _positionSmoothVelocity, positionSmoothTime);
        transform.position = smoothPosition;

        // 使用 SmoothDampAngle 平滑过渡旋转
        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetCameraState.yaw, ref _yawSmoothVelocity, rotationSmoothTime);
        float smoothPitch = Mathf.SmoothDampAngle(transform.eulerAngles.x, _targetCameraState.pitch, ref _pitchSmoothVelocity, rotationSmoothTime);
        float smoothRoll = Mathf.SmoothDampAngle(transform.eulerAngles.z, _targetCameraState.roll, ref _rollSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = new Vector3(smoothPitch, smoothYaw, smoothRoll);
    }
}