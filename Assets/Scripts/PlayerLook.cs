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
            Camera camComponent = cam.GetComponent<Camera>();
            if (camComponent != null) camComponent.enabled = false;

            AudioListener audio = cam.GetComponent<AudioListener>();
            if (audio != null) audio.enabled = false;

            return;
        }

        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50f);

        SetLayerForLocalBody();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void SetLayerForLocalBody()
    {
        int localBodyLayer = LayerMask.NameToLayer("LocalPlayerBody");
        if (localBodyLayer < 0) return;

        foreach (Transform child in transform)
        {
            if (child.name == "Eyes" || child.name == "Mouth")
            {
                SetLayerRecursive(child.gameObject, localBodyLayer);
            }
        }
    }

    void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach  (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
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