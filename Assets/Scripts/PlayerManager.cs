using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public SelectionRaycast selection;
    public List<ShipData> ships;
    private int currentShipIndex = 0;
    private GameObject currentShipInstance;
    public bool placementPhase { get; set; } = true;
    private bool phase2 = false;
    public bool playersTurn_ShootingPhase { get; set; } = true;
    public bool gameEnd = false;
    private Vector3 middlePosition;
    private Vector3 nextPosition;
    private Vector3 shotPosition;
    public GameLogic gameLogic;
    public GameManager gameManager;
    public UIManager uiManager;
    private bool isCoroutineRunning = false;

    void Start()
    {
        // Assign selectionRaycast at runtime
        if (selection == null)
        {
            selection = FindObjectOfType<SelectionRaycast>();
            if (selection == null)
            {
                Debug.LogError("Selection not found in the scene.");
            }
        }

        gameLogic = FindObjectOfType<GameLogic>();
        gameManager = FindObjectOfType<GameManager>();
        uiManager = FindObjectOfType<UIManager>();
    }

    void Update()
    {
        gameEnd = GameStateManager.Instance.GameEnd;

        if (!gameEnd)
        {
            placementPhase = GameStateManager.Instance.PlacementPhase;
            playersTurn_ShootingPhase = GameStateManager.Instance.ShootingPhase;

            if (playersTurn_ShootingPhase && !placementPhase)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    TakeShot();
                }
            }

            if (placementPhase) 
            {
                if (phase2 && Input.GetMouseButtonDown(1))
                {
                    // deselect --> return into phase1 without placing a ship
                    phase2 = false;
                    selection.DeselectingOrientationPlacement();
                }

                if (phase2 && Input.GetMouseButtonDown(0))
                {
                    PlaceShipPhase2();
                    //Debug.Log("Next ship index: " + currentShipIndex);
                }

                if (!phase2 && Input.GetMouseButtonDown(0) && currentShipIndex < ships.Count)
                {
                    PlaceShip();
                }

                // finished placing all ships
                if (!isCoroutineRunning && currentShipIndex >= ships.Count)
                {
                    StartCoroutine(MoveToShootingPhase());
                }
            }
        }
    }

    private IEnumerator MoveToShootingPhase()
    {
        isCoroutineRunning = true;

        //continue to show ships for additional 2 seconds
        Debug.Log("Testpoint - waiting for phase change to shooting.");
        yield return new WaitForSeconds(2.0f);

        Debug.Log("Placed all ships, moving to ShootingPhase");
        GameStateManager.Instance.StartShootingPhase();

        gameManager.setShipsToState(false);

        isCoroutineRunning = false;
    }

    // enters second phase of placing ships (e.g. based on middle position which direction should the ship face)
    void PlaceShip()
    {
        if (currentShipInstance == null && currentShipIndex < ships.Count)
        {
            middlePosition = selection.ShipPlacementHighlight(ships[currentShipIndex].size);
            Debug.Log("Middle Position: " + middlePosition);

            if (middlePosition != Vector3.negativeInfinity)
            //if (selection.lastSelection.IsSelected())
            {
                // enter second selection phase
                phase2 = true;
            } 
        }
        else if (currentShipInstance != null)
        {
            currentShipInstance.GetComponent<Ship>().isPlaced = true;
            currentShipInstance = null;
        }
    }

    // determines the orientation of the ship
    void PlaceShipPhase2()
    {
        //nextPosition = selection.ShipOrientationHighlight();
        nextPosition = selection.lastPosition;
        Debug.Log("Next position: " + nextPosition);

        //if (nextPosition != Vector3.negativeInfinity)
        if (selection.lastSelection != null && selection.lastSelection.IsSelected())
        {
            int offset = ships[currentShipIndex].size / 2;
            if (middlePosition.x + offset <= nextPosition.x || middlePosition.x - offset >= nextPosition.x
                || middlePosition.x + 1 <= nextPosition.x || middlePosition.x - 1 >= nextPosition.x)
            {
                // place ship horizontal
                ships[currentShipIndex].direction = PlacementDirection.Horizontal;
                ships[currentShipIndex].SetSpawnPosition(middlePosition);
                //Debug.Log("Ship Position: " + ships[currentShipIndex].GetPosition());
                currentShipInstance = ships[currentShipIndex].InstantiateShip(Quaternion.identity);
                bool evenShipLength = false;
                if (ships[currentShipIndex].size % 2 == 0)
                {
                    evenShipLength = true;
                }

                List<Vector3> positions = new List<Vector3>();
                Vector3 offsetVector = new Vector3(offset, 0, 0);
                Vector3 oneVector = new Vector3(1, 0, 0);

                if (offset > 1)
                {
                    // big ship
                    if (!evenShipLength)
                    {
                        positions.Add(middlePosition - offsetVector);
                    }

                    positions.Add(middlePosition);
                    positions.Add(middlePosition + oneVector);
                    positions.Add(middlePosition - oneVector);
                    positions.Add(middlePosition + offsetVector);
                }
                else
                {
                    // small ship
                    if (!evenShipLength)
                    {
                        positions.Add(middlePosition - oneVector);
                    }

                    positions.Add(middlePosition);
                    positions.Add(middlePosition + oneVector);
                }

                ships[currentShipIndex].SetPositions(positions);
                selection.MarkAsShip(positions);
                gameLogic.placeShip(positions, currentShipIndex);
            }
            else if (middlePosition.y + offset <= nextPosition.y || middlePosition.y - offset >= nextPosition.y
                || middlePosition.y + 1 <= nextPosition.y || middlePosition.y - 1 >= nextPosition.y)
            {
                // place ship vertical
                ships[currentShipIndex].direction = PlacementDirection.Vertical;
                ships[currentShipIndex].SetSpawnPosition(middlePosition);
                //Debug.Log("Ship Position: " + ships[currentShipIndex].GetPosition());
                currentShipInstance = ships[currentShipIndex].InstantiateShip(Quaternion.Euler(0, 0, 90));
                bool evenShipLength = false;
                if (ships[currentShipIndex].size % 2 == 0)
                {
                    evenShipLength = true;
                }

                List<Vector3> positions = new List<Vector3>();
                Vector3 offsetVector = new Vector3(0, offset, 0);
                Vector3 oneVector = new Vector3(0, 1, 0);

                if (offset > 1)
                {
                    // big ship
                    if (!evenShipLength)
                    {
                        positions.Add(middlePosition + offsetVector);
                    }

                    positions.Add(middlePosition);
                    positions.Add(middlePosition + oneVector);
                    positions.Add(middlePosition - oneVector);
                    positions.Add(middlePosition - offsetVector);
                }
                else
                {
                    // small ship
                    if (!evenShipLength)
                    {
                        positions.Add(middlePosition + oneVector);
                    }

                    positions.Add(middlePosition);
                    positions.Add(middlePosition - oneVector);
                }

                ships[currentShipIndex].SetPositions(positions);
                selection.MarkAsShip(positions);
                gameLogic.placeShip(positions, currentShipIndex);
            }

            selection.DeselectingOrientationPlacement();
            middlePosition = Vector3.negativeInfinity;
            phase2 = false;
            currentShipIndex++;

            if (currentShipIndex >= ships.Count)
            {
                GameStateManager.Instance.StartPausePhase();
            }
        }
    }

    void TakeShot()
    {
        Debug.Log($"Player taking shot.\n");

        shotPosition = selection.Selecting();
        if (shotPosition == Vector3.negativeInfinity)
        {
            Debug.Log("Player shot canceled.");
            return;
        }

        Debug.Log("ShotPosition: " + shotPosition);

        //TODO check if lastSelection.IsSelected() works better than current selection.Selecting()
        //if (selection.lastSelection.IsSelected())
        //{
        bool valid_shot = gameLogic.shootAtOpponentField(shotPosition);
        if (valid_shot)
        {
            GameStateManager.Instance.StartShootingPhaseComputer();
            Debug.Log("Computer is next");
        }
        //}
    }
}
