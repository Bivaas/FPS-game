using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Pickup : MonoBehaviourPun
{
    public Material highlightMaterial;
    public GameObject weaponPrefab;
    public float lookRange = 3f;

    private Material[] originalMaterials;
    private MeshRenderer[] meshRenderers;
    private bool isLookedAt = false;
    private Camera playerCam;
    private PlayerShooting player;

    void Start()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        originalMaterials = new Material[meshRenderers.Length];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            originalMaterials[i] = meshRenderers[i].material;
        }

        // Find only the LOCAL player
        foreach (PlayerShooting ps in FindObjectsByType<PlayerShooting>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (ps.photonView.IsMine)
            {
                player = ps;
                break;
            }
        }

        playerCam = player.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (player == null) return;

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, lookRange))
        {
            if (hit.collider.GetComponentInParent<Pickup>() == this)
            {
                SetLookedAt(true);
                return;
            }
        }

        SetLookedAt(false);
    }

    void SetLookedAt(bool lookedAt)
    {
        isLookedAt = lookedAt;

        if (lookedAt)
        {
            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.material = highlightMaterial;
            }
        }
        else
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].material = originalMaterials[i];
            }
        }
    }

    void OnPickup()
    {
        if (!isLookedAt) return;
        if (player == null) return;

       string weaponName = weaponPrefab.name.Contains("AR") ? "AR" : "Glock";
       player.EquipWeapon(weaponName);

        photonView.RPC(nameof(RPC_DestroyPickup), RpcTarget.MasterClient);
    }

    [PunRPC]
    void RPC_DestroyPickup()
    {
        if (PhotonNetwork.IsMasterClient && gameObject != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}