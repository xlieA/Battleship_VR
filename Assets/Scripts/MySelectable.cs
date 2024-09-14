using UnityEngine;

public class MySelectable : MonoBehaviour {

    public Material highlightMaterial;
    public Material highlightPlacementMaterial;
    public Material hitMaterial;
    public Material missMaterial;
    public Material defaultMaterial;
    public Material flashMaterial;
    private Material oldMaterial;

    private bool activeState = false;
    private bool isHit = false;
    private bool isMiss = false;
    public bool hasShip { get; set; } = false;
    private bool placing = false;

    // Use this for initialization
    void Start ()
    {
        defaultMaterial = new Material(GetComponent<Renderer>().material);
        highlightPlacementMaterial = new Material(GetComponent<Renderer>().material);
        highlightPlacementMaterial.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }
    
    // Update is called once per frame
    void Update ()
    {
        // TODO: check if that was responsible for "water shoot" during placement phase
        // --> nope :(
        /*
        if( activeState && Input.GetKeyDown(KeyCode.Mouse0))
        {
            // remove using System; entry that is auto generated
            //defaultMaterial.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
        */
        
    }

    internal void Select()
    {
        if (!isHit && !isMiss && !hasShip)
        {
            activeState = true;
            GetComponent<Renderer>().material = highlightMaterial;
        }
    }

    internal void Select2()
    {
        if (!isHit && !isMiss)
        {
            activeState = true;
            GetComponent<Renderer>().material = highlightMaterial;
        }
    }

    internal void DeSelect()
    {
        if (!isHit && !isMiss)
        {
            activeState = false;
            GetComponent<Renderer>().material = defaultMaterial;
        }
    }

    internal void SelectPlacement()
    {
        if (!hasShip)
        {
            activeState = true;
            GetComponent<Renderer>().material = highlightPlacementMaterial;
            placing = true;
        }
    }

    internal void DeselectPlacement()
    {
        activeState = false;
        GetComponent<Renderer>().material = defaultMaterial;
        placing = false;
    }

    internal void SelectPlacement2()
    {
        if (placing && !hasShip)
        {
            activeState = true;
            GetComponent<Renderer>().material = highlightMaterial;
        }
    }

    internal void DeselectPlacement2()
    {
        activeState = false;
        GetComponent<Renderer>().material = highlightPlacementMaterial;
    }

    internal void HitCell()
    {
        if (!isHit && !isMiss)
        {
            isHit = true;
            GetComponent<Renderer>().material = hitMaterial;
        }
    }

    internal void MissCell()
    {
        if (!isHit && !isMiss)
        {
            isMiss = true;
            GetComponent<Renderer>().material = missMaterial;
        }
    }

    // call when opponent shoots at some cell
    internal void flashCell()
    {
        oldMaterial = GetComponent<Renderer>().material;
        GetComponent<Renderer>().material = flashMaterial;
    }

    internal void flashCellDeselect()
    {
        if (isHit)
            GetComponent<Renderer>().material = hitMaterial;
        else if (isMiss)
            GetComponent<Renderer>().material = missMaterial;
        else 
            GetComponent<Renderer>().material = oldMaterial;
    }

    public bool IsSelected()
    {
        return activeState;
    }
}
