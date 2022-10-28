using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    PlayerController playerController;

    void Awake() {
        playerController = GetComponentInParent<PlayerController>();
    }
    private void OnTriggerEnter(Collider other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(true);    
        }
    }
    void OnTriggerExit(Collider other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(false);
        }
    }
    void OnTriggerStay(Collider other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(true);
        }
    }
    void OnCollisionEnter(Collision other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(true);    
        }
    }

    void OnCollisionExit(Collision other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(false);
        }
    }

    void OnCollisionStay(Collision other) {
        if(other.gameObject != playerController.gameObject)
        {
            playerController.SetGroundedState(true);
        }
    }
}
