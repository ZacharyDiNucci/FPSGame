using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class AutoGun : Gun
{
    [SerializeField] Camera cam;

    private bool isFireing;


    PhotonView pv;

    void Awake() {
        pv = GetComponent<PhotonView>();
    }
    public override void Use()
    {
        isFireing = true;
        // while(isFireing && LastShootTime + ShootDelay < Time.time)
        // {
        //     Debug.Log("pew");

        // }
        if(LastShootTime + ShootDelay < Time.time)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;

            Vector3 direction = GetDirection();
            if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mask))
            {
                madeImpact = true;
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                pv.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);

            }
            else
            {
                madeImpact = false;
                pv.RPC("RPC_ShootMiss", RpcTarget.All, direction * 100f, hit.normal);
            }
            LastShootTime = Time.time;
        }

        
    }
    private void Update() {
        if(isFireing && LastShootTime + ShootDelay < Time.time)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = cam.transform.position;

            Vector3 direction = GetDirection();
            if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mask))
            {
                madeImpact = true;
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                pv.RPC("RPC_Shoot", RpcTarget.All, hit.point += direction, hit.normal);

            }
            else
            {
                madeImpact = false;
                pv.RPC("RPC_ShootMiss", RpcTarget.All, direction * 100f, hit.normal);
            }
            LastShootTime = Time.time;
        }
        if(Input.GetMouseButtonUp(0) && isFireing)
        {
            isFireing = false;
        }
    }

    private void StartFireing()
    {
        
        Debug.Log(isFireing);
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if(AddBulletSpread)
        {
            direction += new Vector3(
                UnityEngine.Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                UnityEngine.Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                UnityEngine.Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)

            );

            direction.Normalize();
        }
        return direction;
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
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.1f);
        Debug.Log(colliders.Length);
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
        StartCoroutine(SpawnTrail(trail, direction * 10f, hitNormal));
    }
}
