using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviourPun
{
    public int maxHealth = 100;
    public float respawnDelay = 5f;
    public Material hitMat;
    public Slider healthBar;

    private int currentHealth;
    private Renderer rend;
    private Material originalMaterial;

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

        if (currentHealth <= 0)
        {
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
        gameObject.SetActive(false);
        if (photonView.IsMine)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
    }

    void Respawn()
    {
        Transform spawnPoint = GameManager.Instance.GetRandomSpawnPoint();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        currentHealth = maxHealth;
        gameObject.SetActive(true);

        if (healthBar != null) healthBar.value = maxHealth;
    }
}