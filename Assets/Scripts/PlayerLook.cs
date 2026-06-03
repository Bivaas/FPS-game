using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerLook : MonoBehaviourPun
{
    public float mouseSensitivity = 50f;
    public Transform cam;

    private float xRotation = 0f;
    private Vector2 lookInput;
    private bool isPaused = false;

    void Start()
    {
        if (!photonView.IsMine)
        {
            cam.gameObject.SetActive(false);
            return;
        }

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (isPaused) return;
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
        public void SetSensitivity(float value)
    {
        mouseSensitivity = value;
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
    }

    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
}