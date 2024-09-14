using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ship")
        {
            Debug.Log("Hit!");
            // TODO add hit logic
        }
        Destroy(gameObject);
    }
}
