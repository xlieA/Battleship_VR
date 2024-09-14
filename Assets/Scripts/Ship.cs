using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    public int size;
    private Vector3 startPos;
    private Vector3 offset;
    public bool isPlaced = false;
    void OnMouseDown()
    {
        if (!isPlaced)
        {
            startPos = transform.position;
            offset = transform.position - GetMouseWorldPos();
        }
    }

    void OnMouseDrag()
    {
        if (!isPlaced)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        if (!isPlaced)
        {
            isPlaced = true;
        }
    }
    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = 10f; // Distance from the camera
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
}
