using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerShooting : MonoBehaviourPun
{
    public Gun gun;
    public Transform gunHolder;
    public GameObject glockPrefab;
    public GameObject arPrefab;

    private bool isHoldingShoot;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }

    void OnShoot()
    {
        if (!photonView.IsMine) return;
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        if (!photonView.IsMine) return;
        isHoldingShoot = false;
    }

    void OnReload()
    {
        if (!photonView.IsMine) return;
        if (gun != null) gun.TryReload();
    }

    public void OnDrop()
    {
        if (!photonView.IsMine) return;
        if (gun != null)
        {
            gun.Drop();
            gun = null;
        }
    }

    public void EquipWeapon(string weaponName)
    {
        photonView.RPC(nameof(RPC_EquipWeapon), RpcTarget.AllBuffered, weaponName);
    }

    [PunRPC]
    void RPC_EquipWeapon(string weaponName)
    {
        if (gun != null)
        {
            Destroy(gun.gameObject);
            gun = null;
        }

        GameObject prefabToSpawn = null;
        if (weaponName == "Glock") prefabToSpawn = glockPrefab;
        else if (weaponName == "AR") prefabToSpawn = arPrefab;

        if (prefabToSpawn == null || gunHolder == null) return;

        GameObject newWeapon = Instantiate(prefabToSpawn, gunHolder);
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        gun = newWeapon.GetComponent<Gun>();
    }
}