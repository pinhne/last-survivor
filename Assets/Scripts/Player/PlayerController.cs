using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;      // Walk speed
    [SerializeField] private float _runSpeed = 9f;       // Hold Shift to run
    [SerializeField] private float _jumpHeight = 1.5f;
    [SerializeField] private float _gravity = -9.81f;

    [Header("Run")]
    [SerializeField] private KeyCode _runKey = KeyCode.LeftShift;
    [SerializeField] private bool _allowRightControlRun = true;

    [Header("Dodge")]
    [SerializeField] private KeyCode _dodgeLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode _dodgeRightKey = KeyCode.E;
    [SerializeField] private float _dodgeDistance = 4f;
    [SerializeField] private float _dodgeDuration = 0.18f;
    [SerializeField] private float _dodgeCooldown = 0.75f;

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

    private bool _isDodging;
    private float _lastDodgeTime = -999f;

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
        HandleDodgeInput();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        UpdateAnimation();
    }

    private void HandleGroundCheck()
    {
        if (_groundCheck == null)
        {
            _isGrounded = _controller.isGrounded;
        }
        else
        {
            _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundDistance, _groundMask);
        }

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

        // Khi đang dodge thì tạm khóa movement thường để cú né rõ ràng hơn
        if (_isDodging)
        {
            _moveX = 0f;
            _moveZ = 0f;
            return;
        }

        Vector3 move = transform.right * _moveX + transform.forward * _moveZ;

        // Chống chạy chéo nhanh hơn bình thường
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        float currentSpeed = IsRunKeyHeld() ? _runSpeed : _moveSpeed;
        _controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private bool IsRunKeyHeld()
    {
        if (Input.GetKey(_runKey))
            return true;

        return _allowRightControlRun && Input.GetKey(KeyCode.RightControl);
    }

    private void HandleDodgeInput()
    {
        if (_isDodging)
            return;

        if (!_isGrounded)
            return;

        if (Time.time < _lastDodgeTime + _dodgeCooldown)
            return;

        if (Input.GetKeyDown(_dodgeLeftKey))
        {
            StartCoroutine(Dodge(-transform.right));
        }
        else if (Input.GetKeyDown(_dodgeRightKey))
        {
            StartCoroutine(Dodge(transform.right));
        }
    }

    private IEnumerator Dodge(Vector3 direction)
    {
        _isDodging = true;
        _lastDodgeTime = Time.time;

        float elapsedTime = 0f;
        float dodgeSpeed = _dodgeDistance / _dodgeDuration;
        Vector3 dodgeDirection = direction.normalized;

        while (elapsedTime < _dodgeDuration)
        {
            _controller.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isDodging = false;
    }

    private void HandleJump()
    {
        if (_isDodging)
            return;

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
