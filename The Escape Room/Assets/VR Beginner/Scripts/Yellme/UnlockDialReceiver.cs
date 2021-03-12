using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Will Trigger the OnUnlockCollision when a gameObject with a tag "unlockdialkey" collide with the collider on which that script is
/// </summary>
public class UnlockDialReceiver : MonoBehaviour
{
    public UnityEvent OnUnlockCollision;
    //public bool DestroyedOnTriggered;

    void OnCollisionEnter(Collision other)
    {
        //var proj = other.rigidbody.GetComponent<MagicBallProjectile>();

        //if (proj != null)
        if(other.gameObject.tag == "unlockdialkey")
        {
            //Destroy(proj);
            OnUnlockCollision.Invoke();
            //if (DestroyedOnTriggered)
                //Destroy(this);
        }
    }
}