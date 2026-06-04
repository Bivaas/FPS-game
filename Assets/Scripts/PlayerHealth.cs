using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    public float respawnDelay = 5f;
    public Material hitMat;
    public Slider healthBar;

    private int currentHealth;
    private Renderer rend;
    private Material originalMaterial;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponentInChildren<Renderer>();
        originalMaterial = rend.material;

        if (photonView.IsMine)
        {
            healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        }
    }

    [PunRPC]
    public void RPC_TakeDamage(int damage, PhotonMessageInfo info)
    {
        if (!photonView.IsMine) return;

        currentHealth -= damage;
        healthBar.value = currentHealth;

        photonView.RPC("RPC_Blink", RpcTarget.All);

        if (currentHealth <= 0 && !isDead)
        {
            isDead= true;

            Player shooter = info.Sender;
            if (shooter != null)
            {
                int shooterKills = (int)shooter.CustomProperties["Kills"];
                shooter.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                {
                    { "Kills", shooterKills + 1 }
                });
            }

            int myDeaths = (int)PhotonNetwork.LocalPlayer.CustomProperties["Deaths"];
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "Deaths", myDeaths + 1 }
            });

            Die();
        }
    }

    [PunRPC]
    void RPC_Blink()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Blink());        
        }
    }

    IEnumerator Blink()
    {
        rend.material = hitMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = originalMaterial;
    }

    void Die()
    {
        if (photonView.IsMine)
        {
            DropARIfHolding();
            EquipDefaultGlock();
            GameManager.Instance.StartCoroutine(RespawnCountdown());
        }

        gameObject.SetActive(false);
    }

    IEnumerator RespawnCountdown()
    {
        TextMeshProUGUI respawnText = GameManager.Instance.respawnText;

        if (respawnText != null)
        {
            respawnText.gameObject.SetActive(true);

            float timeLeft = respawnDelay;
            while (timeLeft > 0)
            {
                respawnText.text = "Respawning in " + Mathf.CeilToInt(timeLeft) + "...";
                yield return new WaitForSeconds(1f);
                timeLeft -= 1f;
            }

            respawnText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        Respawn();
    }

    void DropARIfHolding()
    {
        PlayerShooting ps = GetComponent<PlayerShooting>();
        if (ps == null || ps.gun == null) return;

        if (ps.gun.gameObject.name.Contains("AR"))
        {
            Vector3 dropPos = transform.position + Vector3.up * 0.5f;
            Quaternion dropRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            string prefabName = GameManager.Instance.arPickupPrefab.name;
            PhotonNetwork.Instantiate(prefabName, dropPos, dropRot);
        }
    }

    void EquipDefaultGlock()
    {
        PlayerShooting ps = GetComponent<PlayerShooting>();
        if (ps == null) return;
        ps.EquipWeapon("Glock");
    }

    void Respawn()
    {
        Transform spawnPoint = GameManager.Instance.GetRandomSpawnPoint();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        currentHealth = maxHealth;
        gameObject.SetActive(true);

        if (healthBar != null) healthBar.value = maxHealth;

        isDead = false; 
    }
}