using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI placementPhase;
    public TextMeshProUGUI shootingPhase;
    public TextMeshProUGUI activePlayerText;
    public TextMeshProUGUI endingText;
    public TextMeshProUGUI infoText;
    public GameObject menuBackground;
    public Button reload;
    public Button info;
    public Button quit;
    public Button exitMenu;
    public Button exitInfo;
    public Button help;

    public ActivePlayer activePlayer;
    public PlayerManager playerManager;
    public List<Image> playerShips = new List<Image>();
    private int playerIndex = 0;
    public List<Image> computerShips = new List<Image>();
    private int computerIndex = 0;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void InitializeUI()
    {
        // assign playerManager at runtime
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
            if (playerManager == null)
            {
                Debug.LogError("PlayerManager not found in the scene.");
            }
        }

        infoText.text = "Placement Phase:\n" +
            "Place your ships using the QR Cube. " +
            "The currently selected cell is highlighted. " +
            "If you want to place your ship there, left-click. " +
            "The game will show you all the possible positions where your ship can be placed on that cell. " +
            "Select one of those cells or right-click to find another position.\n" +
            "\n" +
            "Shooting Phase:\n" +
            "Once you placed all your ships, you enter the Shooting Phase. " + 
            "You and the computer take turns shooting at your ships. " +
            "You can shoot a cell by selecting it with the QR Cube and then left-clicking.";

        // subscribe to game state changes
        GameStateManager.Instance.OnPlacementPhaseStarted += EnterPlacementPhase;
        GameStateManager.Instance.OnShootingPhaseStarted += EnterShootingPhase;

        // initial phase is placement
        EnterPlacementPhase();
    }

    private void EnterPlacementPhase()
    {
        placementPhase.text = "Placement Phase";
        placementPhase.gameObject.SetActive(true);

        shootingPhase.gameObject.SetActive(false);
        activePlayerText.gameObject.SetActive(false);
        endingText.gameObject.SetActive(false);
        menuBackground.SetActive(false);
        reload.gameObject.SetActive(false);
        info.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        quit.gameObject.SetActive(false);
        exitMenu.gameObject.SetActive(false);
        exitInfo.gameObject.SetActive(false);
        help.gameObject.SetActive(true);
    }

    private void EnterShootingPhase()
    {
        placementPhase.gameObject.SetActive(false);

        shootingPhase.gameObject.SetActive(true);
        shootingPhase.text = "Shooting Phase";

        activePlayerText.gameObject.SetActive(true);
        activePlayerText.text = "Your Turn";

        activePlayer = ActivePlayer.Player;
    }

    public void UpdateActivePlayerText(ActivePlayer activePlayer)
    {
        if (activePlayer == ActivePlayer.Player)
        {
            activePlayerText.text = "Your Turn";
        }
        else
        {
            activePlayerText.text = "Computer Turn";
        }
        activePlayerText.gameObject.SetActive(true);
    }

    public void ComputerHit()
    {
        if (computerIndex < computerShips.Count)
        {
            computerShips[computerIndex].color = Color.red;
            computerIndex++;
            Debug.Log("Computer Hit: " + computerIndex);
        }
        
        if (computerIndex >= computerShips.Count)
        {
            Debug.Log("Computer end");
            GameStateManager.Instance.EndGame();
            EndingScreen();
        }
    }

    public void PlayerHit()
    {
        if (playerIndex < playerShips.Count)
        {
            playerShips[playerIndex].color = Color.red;
            playerIndex++;
            Debug.Log("Player Hit: " + playerIndex);
        }

        if (playerIndex >= playerShips.Count)
        {
            Debug.Log("Player end");
            GameStateManager.Instance.EndGame();
            EndingScreen();
        }
    }

    public void EndingScreen()
    {
        if (computerIndex >= computerShips.Count)
        {
            endingText.text = "You win!";
            endingText.alignment = TextAlignmentOptions.Center;
        }

        if (playerIndex >= playerShips.Count)
        {
            endingText.text = "You lose!";
            endingText.alignment = TextAlignmentOptions.Center;
        }

        menuBackground.SetActive(true);
        endingText.gameObject.SetActive(true);
        reload.gameObject.SetActive(true);
        info.gameObject.SetActive(true);
        quit.gameObject.SetActive(true);
        exitMenu.gameObject.SetActive(true);
    }

    public void ToggleMenu()
    {
        AudioManager.Instance.Button();
        endingText.text = "Menu";
        endingText.alignment = TextAlignmentOptions.Center;
        menuBackground.SetActive(true);
        endingText.gameObject.SetActive(true);
        reload.gameObject.SetActive(true);
        info.gameObject.SetActive(true);
        quit.gameObject.SetActive(true);
        exitMenu.gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        AudioManager.Instance.Button();
        menuBackground.SetActive(false);
        endingText.gameObject.SetActive(false);
        reload.gameObject.SetActive(false);
        quit.gameObject.SetActive(false);
        info.gameObject.SetActive(false);
        exitMenu.gameObject.SetActive(false);
    }

    public void ToggleInfo()
    {
        AudioManager.Instance.Button();
        endingText.gameObject.SetActive(false);
        reload.gameObject.SetActive(false);
        quit.gameObject.SetActive(false);
        info.gameObject.SetActive(false);
        exitMenu.gameObject.SetActive(false);

        infoText.gameObject.SetActive(true);
        exitInfo.gameObject.SetActive(true);
    }

    public void CloseInfo()
    {
        AudioManager.Instance.Button();
        infoText.gameObject.SetActive(false);
        exitInfo.gameObject.SetActive(false);

        ToggleMenu();
    }

    public void QuitGame()
    {
        AudioManager.Instance.Button();
        Debug.Log("End game");
#if UNITY_EDITOR
        // Unity editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // build
        Application.Quit();
#endif
    }

    public void ReloadGame(string levelName)
    {
        AudioManager.Instance.Button();
        Debug.Log("Start Level " + levelName);

        // unsubscribe from events to prevent duplicate subscriptions
        GameStateManager.Instance.OnPlacementPhaseStarted -= EnterPlacementPhase;
        GameStateManager.Instance.OnShootingPhaseStarted -= EnterShootingPhase;

        // use the SceneManager to reload the scene
        StartCoroutine(ReloadScene(levelName));
    }

    private IEnumerator ReloadScene(string levelName)
    {
        // load the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);

        // wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // reinitialize the UI after the scene is loaded
        InitializeUI();
    }
}
