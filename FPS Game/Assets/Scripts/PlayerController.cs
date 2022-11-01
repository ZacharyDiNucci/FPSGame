using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] Image healthBarImage;
    [SerializeField] GameObject ui;
    [SerializeField] float mouseSensitivity;
    [SerializeField] float walkSpeed;
    [SerializeField] float sprintSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float smoothTime;

    [SerializeField] Item[] items;


    int itemIndex;
    int previousItemIndex = -1;
    public float nextTimeToFire = 0f;

    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    Rigidbody rb;
    float verticalLookRotation;

    const float MaxHealth = 100f;
    float currentHealth = MaxHealth;

    PlayerManager playerManager;
    PhotonView pv;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }
    void Start() {
        if(pv.IsMine)
        {
            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void Update()
    {
        if(!pv.IsMine){
            return;
            
        }

        LookAround();
        MoveAround();
        JumpAround();

        for(int i = 0; i < items.Length; i++)
        {
            if(Input.GetKeyDown((i +1).ToString())){
                EquipItem(i);
                break;
            }
        }

        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if(itemIndex >= items.Length - 1)
            {
                EquipItem(0);
            }
            else
            EquipItem(itemIndex + 1);
        } else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if(itemIndex <= 0)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
            
        }
        if(pv.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        if(((GunInfo)items[itemIndex].itemInfo).isAutomatic)
        {
            while(Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
            {
                Shoot();
            }
        }
        else if(Input.GetMouseButtonDown(0))
        {
            Debug.Log(((GunInfo)items[itemIndex].itemInfo).isAutomatic);
            Shoot();
        }
        

        if (transform.position.y <= -10f)
        {
            Die();
        }
    }
    private void Shoot()
    {
        nextTimeToFire = Time.time + 1f / ((GunInfo)items[itemIndex].itemInfo).fireRate;
        items[itemIndex].Use();    
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(!pv.IsMine && targetPlayer == pv.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }
    
    void LookAround()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 90f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void MoveAround()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }
    void JumpAround()
    {
        if(Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    void FixedUpdate() {
        if(pv.IsMine){
            rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        }
    }

    void EquipItem(int _index)
    {
        if(_index == previousItemIndex )
        {
            return;
        }
        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true);

        if(previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }
        previousItemIndex = itemIndex;
    }

    public void TakeDamage(float damage)
    {
        pv.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }
    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if(!pv.IsMine)
        {
            return;
        }

        currentHealth -= damage;
        healthBarImage.fillAmount = currentHealth / MaxHealth;
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        playerManager.Die();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "HealthPack")
        {
            if(currentHealth <= MaxHealth + 25.0f)
            {
                Destroy(other);
                pv.RPC("RPC_Heal", RpcTarget.All, 25.0f);
            }
            else if(currentHealth < MaxHealth)
            {
                Destroy(other);
                pv.RPC("RPC_Heal", RpcTarget.All, MaxHealth);
            }
        }
    }

    [PunRPC]
    void RPC_Heal(float heal)
    {
        if(!pv.IsMine)
        {
            return;
        }
        if(heal != MaxHealth)
        {
            currentHealth += heal;
        }
        else currentHealth = MaxHealth;
        healthBarImage.fillAmount = currentHealth / MaxHealth;
    }
}
