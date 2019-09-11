using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public bool printDebug = false;
    void OnCollisionEnter(Collision c) {
        if(printDebug) {
            Debug.Log(c.gameObject.name + " hit me!");
        }
    }
}
