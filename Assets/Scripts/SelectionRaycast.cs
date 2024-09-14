using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionRaycast : MonoBehaviour
{
    public GridManager gridManager;
    public float checkRadius = 0.1f;
    public bool placementPhase = false;
    private bool orientationPhase = false;
    public bool shootingPhase = false;
    public bool pausePhase = false;
    readonly Vector3[] neighbourOffsets = new Vector3[]
        {
            new Vector3(1, 0, 0),    // right
            new Vector3(-1, 0, 0),   // left
            new Vector3(0, 1, 0),    // top
            new Vector3(0, -1, 0),   // bottom
        };

    internal MySelectable lastSelection = null;
    internal List<MySelectable> lastSelectionPlacement = new List<MySelectable>();
    internal MySelectable middleSelectionPlacement = null;
    // internal MySelectable shotSelection = null;

    public Vector3 lastPosition;

    private void Start()
    {
        // assign the gridManager at runtime
        if (gridManager == null)
        {
            gridManager = FindObjectOfType<GridManager>();
            if (gridManager == null)
            {
                Debug.LogError("GridManager not found in the scene.");
            }
        }

        GameStateManager.Instance.OnPlacementPhaseStarted += HandlePlacementPhaseStarted;
        GameStateManager.Instance.OnShootingPhaseStarted += HandleShootingPhaseStarted;
        GameStateManager.Instance.OnPausePhaseStarted += HandlePausePhaseStarted;


        GameStateManager.Instance.StartPlacementPhase();
    }

    private void FixedUpdate()
    {
        placementPhase = GameStateManager.Instance.PlacementPhase;
        shootingPhase = GameStateManager.Instance.ShootingPhase;
        pausePhase = GameStateManager.Instance.PausePhase;

        if (!pausePhase && shootingPhase)
        {
            Selecting();
            Debug.Log("In Shooting Phase");
        }
        else if (!pausePhase && orientationPhase)
        {
            ShipOrientationHighlight();
            Debug.Log("In Orientation Phase");
        }
        else if (!pausePhase && placementPhase)
        {
            Selecting();
            Debug.Log("In Placing Phase");
        }
    }

    private void HandlePlacementPhaseStarted()
    {
        pausePhase = false;
        placementPhase = true;
        shootingPhase = false;
    }

    private void HandleShootingPhaseStarted()
    {
        pausePhase = false;
        placementPhase = false;
        shootingPhase = true;
    }

    private void HandlePausePhaseStarted()
    {
        pausePhase = true;
        placementPhase = false;
        shootingPhase = false;
    }

    // highlights selected grid tile based on position of qr cube
    public Vector3 Selecting()
    {
        RaycastHit hitInfo;
        // position offset avoids self collision with non centered colliders
        bool intersects = Physics.Raycast(transform.position + transform.up * 0.1f, transform.up, out hitInfo);
        if (intersects)
        {
            var newSelection = hitInfo.transform.gameObject.GetComponent<MySelectable>();
            // only activate new selectable if previous one was deselected
            if(lastSelection != null && newSelection != lastSelection)
            {
                lastSelection.DeSelect();
                lastSelection = null;
            }
            if (newSelection != null && !shootingPhase)
            {
                newSelection.Select();
                lastSelection = newSelection;
            }
            if (newSelection != null && shootingPhase)
            {
                newSelection.Select2();
                lastSelection = newSelection;
            }

            //Debug.Log("Selected tile position: " + hitInfo.collider.gameObject.transform.position);
            return hitInfo.collider.gameObject.transform.position;
        }
        else
        {
            if (lastSelection != null)
            {
                lastSelection.DeSelect();
                lastSelection = null;
            }
        }
        return Vector3.negativeInfinity;
    }

    // highlights neighbouring grid tiles at given position to indicate valid placements for ships
    public Vector3 ShipPlacementHighlight(int shipSize)
    {
        Vector3 position = Selecting();

        if (position != Vector3.negativeInfinity)
        {
            GameObject middleObject = GetObjectAtPosition(position, checkRadius);
            if (middleObject != null)
            {
                middleSelectionPlacement =  middleObject.GetComponent<MySelectable>();
            }

            orientationPhase = true;
            int offset = shipSize/2;

            bool evenShipLength = false;
            if (shipSize % 2 == 0)
            {
                evenShipLength = true;
            }

            // check if tile is inside grid
            List<GameObject> cellToHighlight = new List<GameObject>();
            int size;

            // Horizontal
            Vector3 neighbourPosition = position + neighbourOffsets[0] * offset;
            //Debug.Log("Neighbour coords: " + neighbourPosition);
            GameObject hitObject1 = GetObjectAtPosition(neighbourPosition, checkRadius);
            neighbourPosition = position + neighbourOffsets[1] * offset;
            GameObject hitObject2 = GetObjectAtPosition(neighbourPosition, checkRadius);

            if (offset > 1)
            {
                // big ship
                neighbourPosition = position + neighbourOffsets[0];
                GameObject hitObject3 = GetObjectAtPosition(neighbourPosition, checkRadius);
                neighbourPosition = position + neighbourOffsets[1];
                GameObject hitObject4 = GetObjectAtPosition(neighbourPosition, checkRadius);
                size = 3;

                if  (!evenShipLength)
                {
                    cellToHighlight.Add(hitObject2);
                    size = 4;
                }

                cellToHighlight.Add(hitObject1);
                cellToHighlight.Add(hitObject3);
                cellToHighlight.Add(hitObject4);
                DoubleHighlight(cellToHighlight, size);
                cellToHighlight.Clear();
            }
            else
            {
                size = 1;

                // small ship
                if  (!evenShipLength)
                {
                    cellToHighlight.Add(hitObject2);
                    size = 2;
                }

                cellToHighlight.Add(hitObject1);
                DoubleHighlight(cellToHighlight, size);
                cellToHighlight.Clear();
            }

            // Vertical
            neighbourPosition = position + neighbourOffsets[2] * offset;
            //Debug.Log("Neighbour coords: " + neighbourPosition);
            hitObject1 = GetObjectAtPosition(neighbourPosition, checkRadius);
            neighbourPosition = position + neighbourOffsets[3] * offset;
            hitObject2 = GetObjectAtPosition(neighbourPosition, checkRadius);

            if (offset > 1)
            {
                // big ship
                neighbourPosition = position + neighbourOffsets[2];
                GameObject hitObject3 = GetObjectAtPosition(neighbourPosition, checkRadius);
                neighbourPosition = position + neighbourOffsets[3];
                GameObject hitObject4 = GetObjectAtPosition(neighbourPosition, checkRadius);
                size = 3;

                if  (!evenShipLength)
                {
                    cellToHighlight.Add(hitObject1);
                    size = 4;
                }

                cellToHighlight.Add(hitObject2);
                cellToHighlight.Add(hitObject3);
                cellToHighlight.Add(hitObject4);
                DoubleHighlight(cellToHighlight, size);
                cellToHighlight.Clear();
            }
            else
            {
                size = 1;

                // small ship
                if  (!evenShipLength)
                {
                    cellToHighlight.Add(hitObject1);
                    size = 2;
                }

                cellToHighlight.Add(hitObject2);
                DoubleHighlight(cellToHighlight, size);
                cellToHighlight.Clear();
            }
        }
        return position;
    }

    // only highlights tile if the whole ship would be able to fit on it
    private void DoubleHighlight(List<GameObject> hitObject, int size)
    {
        if (hitObject != null && hitObject.Count == size)
        {
            List<MySelectable> selections = new List<MySelectable>();
            bool empty = false;
            Debug.Log("Position List Length: " + hitObject.Count);

            // get objects in list
            foreach (GameObject o in hitObject)
            {
                if (o.GetComponent<MySelectable>() != null && !o.GetComponent<MySelectable>().hasShip)
                {
                    Debug.Log("Position: " + o.transform.position);
                    selections.Add(o.GetComponent<MySelectable>());
                }
                else
                {
                    Debug.Log(o.GetComponent<MySelectable>());
                    empty = true;
                }
            }

            if(!empty && selections.Count == size)
            {
                foreach (MySelectable o in selections)
                {
                    o.SelectPlacement();
                    lastSelectionPlacement.Add(o);
                }
            }
        }
    }

    // only highlight cells in given list (e.g. the tile that highlight the possible placement options)
    public Vector3 ShipOrientationHighlight()
    {
        RaycastHit hitInfo;
        // position offset avoids self collision with non centered colliders
        bool intersects = Physics.Raycast(transform.position + transform.up * 0.1f, transform.up, out hitInfo);
        if (intersects)
        {
            var newSelection = hitInfo.transform.gameObject.GetComponent<MySelectable>();
            if (ContainsElement(newSelection))
            {
                // only activate new selectable if previous one was deselected
                if(lastSelection != null && newSelection != lastSelection)
                {
                    lastSelection.DeselectPlacement2();
                    lastSelection = null;
                }
                if (newSelection != null)
                {
                    newSelection.SelectPlacement2();
                    lastSelection = newSelection;
                }

                lastPosition = hitInfo.collider.gameObject.transform.position;
                //Debug.Log("Selected tile position: " + hitInfo.collider.gameObject.transform.position);
                return hitInfo.collider.gameObject.transform.position;
            }  
        }
        else
        {
            if (lastSelection != null)
            {
                lastSelection.DeselectPlacement2();
                lastSelection = null;
            }
        }
        return Vector3.negativeInfinity;
    }

    // deletes the highlights for valid placement positions
    public void DeselectingOrientationPlacement()
    {
        foreach (MySelectable entry in lastSelectionPlacement)
        {
            entry.DeselectPlacement();
        }

        if (middleSelectionPlacement != null)
        {
            middleSelectionPlacement.DeselectPlacement();
        }
        
        lastSelectionPlacement.Clear();

        orientationPhase = false;
    }

    // returns the object at a given position
    private GameObject GetObjectAtPosition(Vector3 position, float checkRadius)
    {
        Collider[] colliders = Physics.OverlapSphere(position, checkRadius);

        if (colliders.Length > 0)
        {
            return colliders[0].gameObject;
        }

        return null;
    }

    // marks the cell as occupied (can't place any more ships there)
    public void MarkAsShip(List<Vector3> positions)
    {
        foreach (Vector3 p in positions)
        {
            GameObject hitObject = GetObjectAtPosition(p, checkRadius);

            if (hitObject.GetComponent<MySelectable>() != null)
            {
                hitObject.GetComponent<MySelectable>().hasShip = true;

                Debug.Log("Ship length: " + positions.Count());

                // debugging
                /*
                Material highlightPlacementMaterial =  hitObject.GetComponent<MySelectable>().highlightMaterial;
                hitObject.GetComponent<MySelectable>().GetComponent<Renderer>().material = highlightPlacementMaterial;
                Debug.Log("Ship position: " + p);
                Debug.Log("Has ship: " + hitObject.GetComponent<MySelectable>().hasShip);
                */
            }
        }
    }

    // shoots a ship at a given position
    public void Shoot(Vector3 position)
    {
        GameObject hitObject = GetObjectAtPosition(position, checkRadius);
        hitObject.GetComponent<MySelectable>().HitCell();
    }

    // misses a ship at a given position
    public void Miss(Vector3 position)
    {
        GameObject hitObject = GetObjectAtPosition(position, checkRadius);
        hitObject.GetComponent<MySelectable>().MissCell();
    }

    // checks if a list contains a certain element
    private bool ContainsElement(MySelectable search)
    {
        return lastSelectionPlacement.Contains(search);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.up * 0.1f, transform.up);
    }
}
