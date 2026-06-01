using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerLook : MonoBehaviourPun
{
    public float mouseSensitivity = 50f;
    public Transform cam;

    private float xRotation = 0f;
    private Vector2 lookInput;

    void Start()
    {
        if (!photonView.IsMine)
        {
            cam.gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        HandleMouseLook();
    }

    public void OnLook(InputValue value)
    {
        if (!photonView.IsMine) return;
        lookInput = value.Get<Vector2>();
    }

    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}