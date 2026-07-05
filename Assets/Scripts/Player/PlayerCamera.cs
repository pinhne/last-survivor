using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] private float _mouseSensitivity = 200f;
    [SerializeField] private Transform _playerBody;

    [Header("FOV / Zoom")]
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private Camera _armsCamera;
    [SerializeField] private float _defaultFOV = 60f;
    [SerializeField] private float _defaultArmsFOV = 55f;
    [SerializeField] private float _fovChangeSpeed = 12f;

    [Header("Camera Recoil Limit")]
    [SerializeField] private float _maxVerticalRecoil = 12f;
    [SerializeField] private float _maxHorizontalRecoil = 4f;

    private float _xRotation = 0f;

    private float _recoilX = 0f;
    private float _recoilY = 0f;
    private float _activeRecoilRecoverSpeed = 8f;

    private float _targetFOV;

    private void Awake()
    {
        if (_targetCamera == null)
            _targetCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (_targetCamera != null)
        {
            _defaultFOV = _targetCamera.fieldOfView;
            _targetFOV = _defaultFOV;
        }

        if (_armsCamera != null)
        {
            _defaultArmsFOV = _armsCamera.fieldOfView;
        }
    }

    private void Update()
    {
        HandleMouseLook();
        RecoverRecoil();
        UpdateFOV();
        ApplyFinalRotation();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        if (_playerBody != null)
            _playerBody.Rotate(Vector3.up * mouseX);
    }

    private void RecoverRecoil()
    {
        _recoilX = Mathf.MoveTowards(
            _recoilX,
            0f,
            _activeRecoilRecoverSpeed * Time.deltaTime
        );

        _recoilY = Mathf.MoveTowards(
            _recoilY,
            0f,
            _activeRecoilRecoverSpeed * Time.deltaTime
        );
    }

    private void UpdateFOV()
    {
        if (_targetCamera != null)
        {
            _targetCamera.fieldOfView = Mathf.Lerp(
                _targetCamera.fieldOfView,
                _targetFOV,
                _fovChangeSpeed * Time.deltaTime
            );
        }

        // ArmsCamera giữ FOV cố định để súng không bị phóng to / lệch / crop khi zoom.
        if (_armsCamera != null)
        {
            _armsCamera.fieldOfView = _defaultArmsFOV;
        }
    }

    private void ApplyFinalRotation()
    {
        float finalX = _xRotation - _recoilX;
        float finalY = _recoilY;

        transform.localRotation = Quaternion.Euler(finalX, finalY, 0f);
    }

    public void AddRecoil(float verticalAmount, float horizontalAmount, float recoverSpeed)
    {
        if (verticalAmount <= 0f && horizontalAmount <= 0f) return;

        _recoilX = Mathf.Clamp(
            _recoilX + verticalAmount,
            0f,
            _maxVerticalRecoil
        );

        if (horizontalAmount > 0f)
        {
            _recoilY = Mathf.Clamp(
                _recoilY + Random.Range(-horizontalAmount, horizontalAmount),
                -_maxHorizontalRecoil,
                _maxHorizontalRecoil
            );
        }

        if (recoverSpeed > 0f)
            _activeRecoilRecoverSpeed = recoverSpeed;
    }

    public void SetZoom(bool isZooming, float zoomFOV)
    {
        _targetFOV = isZooming ? zoomFOV : _defaultFOV;
    }

    public void ResetCameraEffects()
    {
        _recoilX = 0f;
        _recoilY = 0f;
        _targetFOV = _defaultFOV;

        if (_targetCamera != null)
            _targetCamera.fieldOfView = _defaultFOV;

        if (_armsCamera != null)
            _armsCamera.fieldOfView = _defaultArmsFOV;
    }
}