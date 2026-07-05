using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] private float _mouseSensitivity = 200f;
    [SerializeField] private Transform _playerBody;

    private float _xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        // Xoay trục dọc (lên/xuống) - chỉ áp dụng cho Camera, clamp -80 -> 80 độ
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Xoay trục ngang (trái/phải) - áp dụng cho cả Player body (để di chuyển đúng hướng nhìn)
        _playerBody.Rotate(Vector3.up * mouseX);
    }
}