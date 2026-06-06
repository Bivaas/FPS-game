using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

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

        int uniqueSeed = System.Guid.NewGuid().GetHashCode() ^ (PhotonNetwork.LocalPlayer.ActorNumber * 7919);
        Random.InitState(uniqueSeed);

        Transform spawnPoint = GetInitialSpawnPoint();
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

        PlayerShooting playerShooting = player.GetComponent<PlayerShooting>();
        if (playerShooting != null)
        {
            playerShooting.EquipWeapon("Glock");
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

    public Transform GetInitialSpawnPoint()
    {
        HashSet<int> claimedIndices = new HashSet<int>();

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsLocal) continue;
            if (p.CustomProperties.ContainsKey("SpawnIndex"))
            {
                claimedIndices.Add((int)p.CustomProperties["SpawnIndex"]);
            }
        }

        List<int> available = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!claimedIndices.Contains(i)) available.Add(i);
        }

        if (available.Count == 0)
        {
            Debug.LogWarning("No available spawn indices, falling back to random");
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        int chosenIndex = available[Random.Range(0, available.Count)];

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "SpawnIndex", chosenIndex } });

        return spawnPoints[chosenIndex];
    }

    public Transform GetRandomSpawnPoint()
    {
        List<Transform> shuffledSpawns = new List<Transform>(spawnPoints);
        for (int i = shuffledSpawns.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = shuffledSpawns[i];
            shuffledSpawns[i] = shuffledSpawns[j];
            shuffledSpawns[j] = temp;
        }

        PlayerHealth[] allPlayers = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        Transform bestSpawn = null;
        float bestMinDistance = -1f;

        foreach (Transform candidate in shuffledSpawns)
        {
            float minDistToAnyPlayer = float.MaxValue;

            foreach (PlayerHealth player in allPlayers)
            {
                if (player == null || !player.gameObject.activeInHierarchy) continue;

                float dist = Vector3.Distance(candidate.position, player.transform.position);
                if (dist < minDistToAnyPlayer)
                {
                    minDistToAnyPlayer = dist;
                }
            }

            if (minDistToAnyPlayer == float.MaxValue)
            {
                return candidate;
            }

            if (minDistToAnyPlayer > bestMinDistance)
            {
                bestMinDistance = minDistToAnyPlayer;
                bestSpawn = candidate;
            }
        }

        if (bestSpawn == null)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        return bestSpawn;
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