using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerShooting : MonoBehaviourPun
{
    public Gun gun;
    public Transform gunHolder;

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
}