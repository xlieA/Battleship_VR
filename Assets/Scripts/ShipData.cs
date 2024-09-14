using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipData : MonoBehaviour
{
    public GameObject ship;
    public GameObject shipInstance;
    public int size;
    public int health;
    private Vector3 spawnPosition;
    public List<Vector3> positions = new List<Vector3>();      // contains all the cells that are occupied by ship
    public PlacementDirection direction;
    private Color startColor;
    private int maxHealth;

    public void Start()
    {
        maxHealth = health;
    }

    public void SetSpawnPosition(Vector3 p)
    {
        if (size % 2 == 0)
        {
            if (direction == PlacementDirection.Horizontal)
            {
                // Horizontal
                spawnPosition = p;
                spawnPosition.x = p.x + 0.5f;
            }
            else
            {
                // Vertical
                spawnPosition = p;
                spawnPosition.y = p.y - 0.5f;
            }
        }
        else
        {
            spawnPosition = p;
        }
    }

    public void SetPositions(List<Vector3> pos)
    {
        foreach (Vector3 p in pos)
        {
            positions.Add(p);
        }
    }

    public GameObject InstantiateShip(Quaternion rotation)
    {
        AudioManager.Instance.Button();
        shipInstance = Instantiate(ship, spawnPosition, rotation);
        startColor = shipInstance.GetComponent<MeshRenderer>().sharedMaterial.color;
        shipInstance.SetActive(true);
        return shipInstance;
    }
       
    public void OnDamage()
    { 
        if (health <= 0) return; //TODO: destroy when dead
        health--;
        shipInstance.GetComponent<MeshRenderer>().material.color = Color.Lerp(Color.red, startColor, (float)health / (float)maxHealth);

    }
}
