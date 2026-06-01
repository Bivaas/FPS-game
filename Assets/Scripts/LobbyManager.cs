using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_InputField roomInput;
    public TextMeshProUGUI statusText;
    public GameObject buttonsPanel;

    void Start()
    {
        buttonsPanel.SetActive(false);
        statusText.text = "Connecting...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        buttonsPanel.SetActive(true);
        statusText.text = "Connected. Enter your name and a room.";
    }

    public void OnClickCreateRoom()
    {
        if (!ValidateInputs()) return;

        PhotonNetwork.NickName = nameInput.text;

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;

        statusText.text = "Creating room...";
        buttonsPanel.SetActive(false);

        PhotonNetwork.CreateRoom(roomInput.text, options);
    }

    public void OnClickJoinRoom()
    {
        if (!ValidateInputs()) return;

        PhotonNetwork.NickName = nameInput.text;

        statusText.text = "Joining room...";
        buttonsPanel.SetActive(false);

        PhotonNetwork.JoinRoom(roomInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "Failed to create room: " + message;
        buttonsPanel.SetActive(true);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "Room not found or full.";
        buttonsPanel.SetActive(true);
    }

    bool ValidateInputs()
    {
        if (string.IsNullOrEmpty(nameInput.text))
        {
            statusText.text = "Please enter your name.";
            return false;
        }
        if (string.IsNullOrEmpty(roomInput.text))
        {
            statusText.text = "Please enter a room name.";
            return false;
        }
        return true;
    }
}