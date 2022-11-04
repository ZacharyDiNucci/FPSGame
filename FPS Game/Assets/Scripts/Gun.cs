using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Gun : Item
{
    public abstract override void Use();
    public abstract override void UpdateUI();
    public abstract override void Reload();
    public abstract override void StopReload();
    public abstract override void ResumeReload();
    
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

    public int MaxAmmo;
    public int MaxMag;
    public int CurrentAmmo;
    public int ReloadTime;
    public TMP_Text ammoText;
    
}
