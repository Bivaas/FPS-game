using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public Gun gun;
    public Transform gunHolder;

    private bool isHoldingShoot;

    void Update()
    {
        if (isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }

    void OnShoot()
    {
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        isHoldingShoot = false;
    }

    void OnReload()
    {
        if (gun != null)
        {
            gun.TryReload();
        }
    }

    public void OnDrop()
    {
        if (gun != null)
        {
            gun.Drop();
            gun = null;
        }
    }
}