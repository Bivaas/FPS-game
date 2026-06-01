using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Playermovement : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundmask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        CheckGround();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        MovePlayer();
    }

    void OnMovement(InputValue value)
    {
        if (!photonView.IsMine) return;
        moveInput = value.Get<Vector2>();
    }

    void OnJump()
    {
        if (!photonView.IsMine) return;
        if (isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }

    void MovePlayer()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y, direction.z * moveSpeed);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundmask);
    }
}