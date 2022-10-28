using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Gun : Item
{
    public abstract override void Use();
    
    public GameObject bulletImpactPrefab;
    public TrailRenderer bulletTrailPrefab;
    public ParticleSystem hitEffect;

    public Transform effectOrigin;

    public bool AddBulletSpread = true;
    public Vector3 BulletSpreadVariance = new Vector3(0.1f,0.1f,0);

    public float ShootDelay = 0.5f;

    public LayerMask mask;

    public float LastShootTime;
    public float Speed;
    public bool madeImpact;
}
