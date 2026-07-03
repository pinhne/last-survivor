using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Ground Check")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundDistance = 0.4f;
    [SerializeField] private LayerMask _groundMask;

    [Header("Animation")]
    [SerializeField] private float _animationSmoothTime = 0.1f;
    private Animator _animator;

    // Animation parameter IDs (dùng hash thay string để tối ưu performance)
    private static readonly int _animMoveX = Animator.StringToHash("MoveX");
    private static readonly int _animMoveZ = Animator.StringToHash("MoveZ");
    private static readonly int _animIsGrounded = Animator.StringToHash("IsGrounded");

    // Smoothed animation values
    private Vector2 _animVelocity;
    private Vector2 _animCurrentValue;

    private CharacterController _controller;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _moveX;
    private float _moveZ;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // Tìm Animator trên HumanM_Model (con của Player)
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
            Debug.LogWarning("[PlayerController] Không tìm thấy Animator! Kiểm tra HumanM_Model có Animator component không.");
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimation();
    }

    private void HandleGroundCheck()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);

        // Giữ player dính sàn, tránh velocity.y bị âm tích lũy khi đứng yên
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

    }

    private void HandleMovement()
    {
        _moveX = Input.GetAxis("Horizontal"); // A/D
        _moveZ = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * _moveX + transform.forward * _moveZ;
        _controller.Move(move * _moveSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            // Công thức vật lý: v = sqrt(h * -2 * g)
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        if (_animator == null) return;

        // Smooth dần giá trị animation để tránh giật cục khi chuyển hướng
        Vector2 targetValue = new Vector2(_moveX, _moveZ);
        _animCurrentValue = Vector2.SmoothDamp(
            _animCurrentValue,
            targetValue,
            ref _animVelocity,
            _animationSmoothTime
        );

        _animator.SetFloat(_animMoveX, _animCurrentValue.x);
        _animator.SetFloat(_animMoveZ, _animCurrentValue.y);
        _animator.SetBool(_animIsGrounded, _isGrounded);
    }
}