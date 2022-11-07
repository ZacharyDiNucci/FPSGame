using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class SingleShotGun : Gun
{
    [SerializeField] Camera cam;

    bool reloadStopped = false;
    bool canShoot = true;

    PhotonView pv;

    void Awake() {
        pv = GetComponent<PhotonView>();
    }
    public override void Use()
    {
        if(LastShootTime + ShootDelay < Time.time)
        {
            if(canShoot){
                if(CurrentAmmo <=0)
                {
                    canShoot = false;
                    pv.RPC("RPC_Reload", RpcTarget.All);
                    return;
                }
                CurrentAmmo--;
                UpdateUI();

                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                ray.origin = cam.transform.position;

                ray.direction += new Vector3(
                UnityEngine.Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                UnityEngine.Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                0

                );

                if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mask))
                {
                    madeImpact = true;
                    hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                    pv.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

                }
                else
                {
                    madeImpact = false;
                    pv.RPC("RPC_ShootMiss", RpcTarget.All, ray .direction* 100f, hit.normal);
                }
                LastShootTime = Time.time;
            }
        }
    }

    public override void UpdateUI()
    {
        UpdateAmmoUI();
    }

    public override void Reload()
    {
        if(CurrentAmmo == MaxMag)
        {
            return;
        }
        canShoot = false;
        pv.RPC("RPC_Reload", RpcTarget.All);
        return;
    }

    public override void StopReload()
    {
        if(canShoot)
        {
            return;
        }
        reloadStopped = true;
        StopCoroutine(ReloadGun());

    }

    public override void ResumeReload()
    {
        if(!reloadStopped)
        {
            return;
        }
        canShoot = false;
        pv.RPC("RPC_Reload", RpcTarget.All);
        return;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 direction = (hitPoint - trail.transform.position).normalized;
        Vector3 startPosition = trail.transform.position;

        float distance = Vector3.Distance(trail.transform.position, hitPoint);
        float startingDistance = distance;

        while(distance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (distance / startingDistance));
            distance -= Time.deltaTime * Speed;

            yield return null;
        }
        trail.transform.position = hitPoint;
        if(madeImpact)
        {
            Instantiate(hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
        }
        Destroy(trail.gameObject, trail.time);
    }

    [PunRPC]
    void RPC_Shoot( Vector3 hitPosition, Vector3 hitNormal)
    {
        TrailRenderer trail = Instantiate(bulletTrailPrefab, effectOrigin.position, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, hitPosition, hitNormal));
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if(colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 7.5f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }

    [PunRPC]
    void RPC_ShootMiss( Vector3 direction, Vector3 hitNormal)
    {
        TrailRenderer trail = Instantiate(bulletTrailPrefab, effectOrigin.position, Quaternion.identity);
        StartCoroutine(SpawnTrail(trail, direction * 100f, hitNormal));
    }

    
    [PunRPC]
    void RPC_Reload()
    {
        if(!pv.IsMine)
        {
            return;
        }
    
        StartCoroutine(ReloadGun());
        UpdateUI();
    }

    private IEnumerator ReloadGun()
    {
        yield return new WaitForSeconds(ReloadTime);

        if(MaxAmmo >= (MaxMag - CurrentAmmo))
        {
            MaxAmmo = MaxAmmo - (MaxMag - CurrentAmmo);
            CurrentAmmo = MaxMag;
        } else
        {
            CurrentAmmo = MaxAmmo;
            MaxAmmo = 0;
        }

        canShoot = true;
    }

    void UpdateAmmoUI()
    {
        ammoText.text = CurrentAmmo + " / " + MaxAmmo;
    }
}
