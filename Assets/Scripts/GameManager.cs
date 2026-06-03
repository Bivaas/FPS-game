using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Spawning")]
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    public GameObject glockPrefab;
    public Transform glockSpawnPoint;
    public Transform arSpawnPoint;
    public GameObject arPickupPrefab;

    [Header("UI")]
    public GameObject scoreboardPanel;
    public Transform scoreboardContent;
    public GameObject scoreboardRowPrefab;
    public TextMeshProUGUI respawnText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Hashtable initialProps = new Hashtable
        {
            { "Kills", 0 },
            { "Deaths", 0 }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);

        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        if (playerShooting != null && playerShooting.gunHolder != null)
        {
            GameObject glock = Instantiate(glockPrefab, playerShooting.gunHolder);
            glock.transform.localPosition = Vector3.zero;
            glock.transform.localRotation = Quaternion.identity;
            playerShooting.gun = glock.GetComponent<Gun>();
        }
        

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(arPickupPrefab.name, arSpawnPoint.position, arSpawnPoint.rotation);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreboardPanel.SetActive(true);
            UpdateScoreboard();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            scoreboardPanel.SetActive(false);
        }
    }

    public Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    void UpdateScoreboard()
    {
        foreach (Transform child in scoreboardContent)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject row = Instantiate(scoreboardRowPrefab, scoreboardContent);
            row.transform.localScale = Vector3.one;
            TextMeshProUGUI[] texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            int kills = player.CustomProperties.ContainsKey("Kills") ? (int)player.CustomProperties["Kills"] : 0;
            int deaths = player.CustomProperties.ContainsKey("Deaths") ? (int)player.CustomProperties["Deaths"] : 0;

            texts[0].text = player.NickName;
            texts[1].text = kills.ToString();
            texts[2].text = deaths.ToString();
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(scoreboardContent as RectTransform);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
    }
}