using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManager playerManager;
    public GameLogic gameLogic;
    public UIManager uiManager;
    public bool placementPhase = false;
    public bool playersTurn_ShootingPhase = false;
    public bool shootingPhaseComputer = false;
    public bool pausePhase = false;
    private bool isCoroutineRunning = false;
    public bool gameEnd = false;

    void Start()
    {
        // Initialize the game
        gameLogic = FindObjectOfType<GameLogic>();
        playerManager = FindObjectOfType<PlayerManager>();
        uiManager = FindObjectOfType<UIManager>();

        placementPhase = GameStateManager.Instance.PlacementPhase;
        playersTurn_ShootingPhase = GameStateManager.Instance.ShootingPhase;
        shootingPhaseComputer = GameStateManager.Instance.ShootingPhaseComputer;
        gameEnd = GameStateManager.Instance.GameEnd;

        GameStateManager.Instance.StartPlacementPhase();
    }

    void Update()
    {
        gameEnd = GameStateManager.Instance.GameEnd;

        // Game update logic
        if (!gameEnd)
        {
            placementPhase = GameStateManager.Instance.PlacementPhase;
            playersTurn_ShootingPhase = GameStateManager.Instance.ShootingPhase;
            pausePhase = GameStateManager.Instance.PausePhase;
            shootingPhaseComputer = GameStateManager.Instance.ShootingPhaseComputer;

            if (!isCoroutineRunning && shootingPhaseComputer)
            {
                StartCoroutine(gameTick());
            }
        }
    }

    private IEnumerator gameTick()
    {
        isCoroutineRunning = true;

        yield return new WaitForSeconds(2.0f);

        if (gameEnd)
        {
            isCoroutineRunning = false;
            yield break;
        }

        // Player waiting for shot from opponent -> shoot & set turn to player
        setShipsToState(true);

        yield return new WaitForSeconds(2.0f);

        if (gameEnd)
        {
            isCoroutineRunning = false;
            yield break;
        }

        gameLogic.shootAtPlayerField();
        yield return new WaitForSeconds(2.0f);

        setShipsToState(false);

        isCoroutineRunning = false;
        GameStateManager.Instance.StartShootingPhase();
    }

    public void setShipsToState(bool to_active)
    {
        playerManager.ships.ForEach(ship_element =>
        {
            if (ship_element.ship != null)
                ship_element.ship.SetActive(to_active);
            if (ship_element.shipInstance != null)
                ship_element.shipInstance.SetActive(to_active);
            if (ship_element.ship == null && ship_element.shipInstance == null)
                print($"Warning: Ship (size {ship_element.size}) has neither .ship nor .shipInstance to set active status to {to_active}.");
        });
    }
}
