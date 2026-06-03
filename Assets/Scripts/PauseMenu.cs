using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Photon.Pun;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Button resumeButton;
    public Button leaveButton;

    [Header("Sensitivity Range")]
    public float minSensitivity = 10f;
    public float maxSensitivity = 200f;

    private PlayerLook localPlayerLook;
    private bool isPaused = false;

    void Start()
    {
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        leaveButton.onClick.AddListener(LeaveRoom);

        sensitivitySlider.minValue = minSensitivity;
        sensitivitySlider.maxValue = maxSensitivity;
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 50f);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        UpdateSensitivityLabel(sensitivitySlider.value);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SetLocalPlayerLookPaused(true);
    }

    void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetLocalPlayerLookPaused(false);
    }

    void OnSensitivityChanged(float value)
    {
        UpdateSensitivityLabel(value);
        FindLocalPlayerLook();
        if (localPlayerLook != null)
        {
            localPlayerLook.SetSensitivity(value);
        }
    }

    void UpdateSensitivityLabel(float value)
    {
        if (sensitivityValueText != null)
            sensitivityValueText.text = Mathf.RoundToInt(value).ToString();
    }

    void SetLocalPlayerLookPaused(bool paused)
    {
        FindLocalPlayerLook();
        if (localPlayerLook != null)
            localPlayerLook.SetPaused(paused);
    }

    void FindLocalPlayerLook()
    {
        if (localPlayerLook != null) return;

        PlayerLook[] allLooks = FindObjectsByType<PlayerLook>(FindObjectsSortMode.None);
        foreach (PlayerLook pl in allLooks)
        {
            if (pl.photonView.IsMine)
            {
                localPlayerLook = pl;
                return;
            }
        }
    }

    void LeaveRoom()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PhotonNetwork.LeaveRoom();
    }
}