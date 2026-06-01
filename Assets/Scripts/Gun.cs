using UnityEngine;
using System.Collections;
using Photon.Pun;

public class Gun : MonoBehaviourPun
{
    public float reloadTime = 1f;
    public float fireRate = 0.15f;
    public int magSize = 20;
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 15f;

    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public GameObject droppedWeapon;

    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 reloadRotationOffset = new Vector3(66, 50, 50);
    private Camera cam;

    void Start()
    {
        currentAmmo = magSize;
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
        cam = GetComponentInParent<PlayerShooting>().GetComponentInChildren<Camera>();
    }

    public void Shoot()
    {
        if (isReloading) return;
        if (Time.time < nextTimeToFire) return;

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        StopCoroutine(nameof(Recoil));
        StartCoroutine(nameof(Recoil));

        nextTimeToFire = Time.time + fireRate;
        currentAmmo--;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
    
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            PlayerHealth health = hit.collider.GetComponentInParent<PlayerHealth>();
            if (health != null)
            {
                health.photonView.RPC("RPC_TakeDamage", RpcTarget.AllViaServer, 10);
            }
        }
        
        Vector3 targetPoint = ray.GetPoint(100f);
        if (Physics.Raycast(ray, out RaycastHit visualHit, 100f))
        {
            targetPoint = visualHit.point;
        }

        Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;
        Quaternion bulletRotation = Quaternion.LookRotation(direction);
        Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        GameObject bulletObj = Instantiate(bullet, bulletSpawnPoint.position, bulletRotation);
        Rigidbody bulletRb = bulletObj.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * 30f;
        }
    }

    public void TryReload()
    {
        if (isReloading) return;
        if (currentAmmo == magSize) return;
        StartCoroutine(Reload());
    }

    public void Drop()
    {
        Instantiate(droppedWeapon, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    IEnumerator Reload()
    {
        isReloading = true;

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRotationOffset);
        float halfReload = reloadTime / 2f;
        float t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReload);
            yield return null;
        }

        t = 0f;

        while (t < halfReload)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReload);
            yield return null;
        }

        currentAmmo = magSize;
        isReloading = false;
    }

    IEnumerator Recoil()
    {
        Vector3 recoilTarget = initialPosition + new Vector3(recoilDistance, 0, 0);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(initialPosition, recoilTarget, t);
            yield return null;
        }

        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(recoilTarget, initialPosition, t);
            yield return null;
        }

        transform.localPosition = initialPosition;
    }
}