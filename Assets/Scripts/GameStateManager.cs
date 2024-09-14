using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public bool PlacementPhase { get; private set; } = true;
    public bool ShootingPhase { get; private set; } = false;
    public bool ShootingPhaseComputer { get; private set; } = false;
    public bool PausePhase { get; private set; } = false;
    public ActivePlayer ActivePlayer { get; private set; } = ActivePlayer.Player;
    public bool GameEnd { get; private set; } = false;

    public event Action OnPlacementPhaseStarted;
    public event Action OnShootingPhaseStarted;
    public event Action OnPausePhaseStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartPlacementPhase()
    {
        PausePhase = false;
        PlacementPhase = true;
        ShootingPhase = false;
        ShootingPhaseComputer = false;
        OnPlacementPhaseStarted?.Invoke();
        Debug.Log("Placement phase started");
    }

    public void StartShootingPhase()
    {
        PausePhase = false;
        PlacementPhase = false;
        ShootingPhase = true;
        ShootingPhaseComputer = false;
        OnShootingPhaseStarted?.Invoke();
        Debug.Log("Shooting phase started");

        ActivePlayer = ActivePlayer.Computer;
        ChangeTurn();
    }

    public void StartShootingPhaseComputer()
    {
        PausePhase = true;
        PlacementPhase = false;
        ShootingPhase = false;
        ShootingPhaseComputer = true;
        OnShootingPhaseStarted?.Invoke();
        Debug.Log("Shooting phase (computer) started");

        ActivePlayer = ActivePlayer.Player;
        ChangeTurn();
    }

    public void StartPausePhase()
    {
        PausePhase = true;
        PlacementPhase = false;
        ShootingPhase = false;
        ShootingPhaseComputer = false;
        OnPausePhaseStarted?.Invoke();
        Debug.Log("Pause phase started");
    }

    public void ChangeTurn()
    {
        if (ActivePlayer == ActivePlayer.Player)
        {
            ActivePlayer = ActivePlayer.Computer;
        }
        else
        {
            ActivePlayer = ActivePlayer.Player;
        }

        Debug.Log("Changing turn to: " + ActivePlayer);
        UIManager.Instance.UpdateActivePlayerText(ActivePlayer);
    }

    public void EndGame()
    {
        GameEnd = true;
        Debug.Log("Game ended");
    }
}

