using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Text;
using System.Collections.Generic;
using System;
using System.Collections;
using UnityEditor;


public class GameLogic : MonoBehaviour
{
    public GridManager gridManager;
    public PlayerManager playerManager;
    public UIManager uiManager;

    private int board_width;
    private int board_height;
    private int[,] playerField;
    private int[,] opponentField;
    private (int, int)[] shipOptions;
    private int[,] computerHitMemory;
    private Tuple<int, int> lastHit; // last position where computer hit player ship

    // public GameObject gameOverScreen;
    // public GameObject gameWinScreen;
    public bool gameIsOver = false;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        uiManager = FindObjectOfType<UIManager>();

        loadInfos();
        initializeOpponentField();
    }

    private void loadInfos()
    {
        //load board size
        board_width = gridManager.GetGridWidth();
        board_height = gridManager.GetGridHeight();
        playerField = new int[board_width, board_height];
        opponentField = new int[board_width, board_height];
        computerHitMemory = new int[board_width, board_height];


        // load ShipData info, i.e. number of ships & their length + health
        List<ShipData> ships = playerManager.ships;
        shipOptions = new (int, int)[ships.Count];
        for (int i = 0; i < ships.Count; i++)
        {
            shipOptions[i] = (ships[i].size, ships[i].health);
        }
    }

    private void initializeOpponentField()
    {
        for (int ship_index = 0; ship_index < shipOptions.Length; ship_index++)
        {
            int ship_len = shipOptions[ship_index].Item1;
            while (true)
            {
                bool orientation = Random.Range(0, 2) == 0;
                int x;
                int y;
                if (orientation)
                {
                    x = Random.Range(0, board_width - ship_len + 1);
                    y = Random.Range(0, board_height);
                }
                else
                {
                    x = Random.Range(0, board_width);
                    y = Random.Range(0, board_height - ship_len + 1);
                }

                // check ship space // "Ships can touch each other, but they can't occupy the same grid space."
                bool occupied = false;
                for (int i = 0; i < ship_len && !occupied; i++)
                {
                    int x_i = orientation ? i : 0;
                    int y_i = !orientation ? i : 0;
                    if (opponentField[x + x_i, y + y_i] > 0)
                    {
                        // found overlap, just retry (via continue)
                        occupied = true;
                    }
                }
                if (!occupied)
                {
                    //location is free, place ship variant
                    for (int i = 0; i < ship_len; i++)
                    {
                        int x_i = orientation ? i : 0;
                        int y_i = !orientation ? i : 0;
                        if (opponentField[x + x_i, y + y_i] != 0)
                            print($"Warning: Overwriting opponentField value {opponentField[x + x_i, y + y_i]}, at position {x + x_i}, {y + y_i}.");
                        opponentField[x + x_i, y + y_i] = shipOptions.Length - ship_index;
                    }
                    break;
                }
                
            }
        }
        
        //logField("opponentField", opponentField);
    }

    private void logField(string name, int[,] field)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{name}=\n[");
        for (int i = 0; i < field.GetLength(0); i++)
        {
            for (int j = 0; j < field.GetLength(1); j++)
            {
                int fixed_x = j;
                int fixed_y = field.GetLength(0) - 1 - i;
                sb.Append(field[fixed_x, fixed_y]);
                if (j < field.GetLength(1) - 1)
                    sb.Append(" ");
                else if (i < field.GetLength(0) - 1)
                    sb.Append(",\n");
            }
        }
        sb.Append("]");
        print(sb.ToString());
    }

    public void placeShip(List<Vector3> positions, int ship_index)
    {
        if (ship_index >= shipOptions.Length)
        {
            print("Invalid index for ship placement: " + ship_index);
            return;
        }

        int ship_len = shipOptions[ship_index].Item1;

        //get positions in array
        int[,] coordinates = new int[positions.Count, 2];
        for (int i = 0; i < positions.Count; i++)
        {
            coordinates[i, 0] = (int)(positions[i].x - gridManager.xOffset);
            coordinates[i, 1] = (int)(positions[i].y - gridManager.yOffset);
            print($"Placing ship at coordinates {coordinates[i, 0]} {coordinates[i, 1]} with positions[i]={positions[i]} and ship_len={ship_len}.");
        }

        try
        {
            //place ship variant
            for (int i = 0; i < positions.Count; i++)
            {
                playerField[coordinates[i, 0], coordinates[i, 1]] = shipOptions.Length - ship_index;
            }
        } catch (IndexOutOfRangeException e)
        {
            print($"Ship placement failed: positions.Count={positions.Count}, exception: " + e.Message);
        }



        logField("playerField", playerField);
    }

    public bool shootAtOpponentField(Vector3 shotPosition)
    {
        //Input vector carries Grid-coordinates, i.e. x & y have to be transformed for array access
        int x = (int)shotPosition.x - gridManager.xOffset;
        int y = (int)shotPosition.y - gridManager.yOffset;

        if (x >= board_width || y >=  board_height || x < 0 || y < 0)
        {
            print($"Shot out of bounds: {x}, {y}");
            return false;
        }

        // highlight via MySelectable
        GameObject targeted_cell = GameObject.Find($"Cell_{shotPosition.x}_{shotPosition.y}");
        MySelectable cell_selectable = targeted_cell.transform.gameObject.GetComponent<MySelectable>();
        if (opponentField[x, y] > 0)
        {
            cell_selectable.HitCell();
            AudioManager.Instance.Hit();
        }
        else
        {
            cell_selectable.MissCell();
            AudioManager.Instance.Miss();
        }

        if (opponentField[x, y] > 0)
        {
            int ship_index = opponentField[x, y];
            opponentField[x, y] = -ship_index;
            if (!containsMatch(opponentField, ship_index, false)) {
                //ship destroyed, increase score
                print($"Destroyed ship with index {ship_index}.");
                uiManager.ComputerHit();
            }
        }
        return true;
    }

    public void shootAtPlayerField()
    {
        Tuple<int, int> shotPosition = nextShotPos();
        Debug.Log("Info - ComputerTargetting chose shotPosition: " + shotPosition);
        int x = shotPosition != null ? shotPosition.Item1 : Random.Range(0, board_width);
        int y = shotPosition != null ? shotPosition.Item2 : Random.Range(0, board_height);

        shootAtSpecificPlayerField(x, y);
    }

    private Tuple<int, int> nextShotPos()
    {
        // check last successful hit's adjacent cells, if available & any are viable
        if (lastHit != null)
        {
            Tuple<int, int> follow_up = validNeightbourCell(lastHit.Item1, lastHit.Item2);
            if (follow_up != null)
                return follow_up;
        }

        //find unchecked cell next to location of any successful (& undestroyed) hit
        for (int i = 0; i < board_width; i++)
        {
            for (int j = 0; j < board_height; j++)
            {
                if (computerHitMemory[i, j] > 0)
                {
                    Tuple<int, int> res = validNeightbourCell(i, j);
                    if (res != null)
                        return res;
                }
            }
        }

        //no lead found, default to random coordinate options ("+" shape), but with preference for cells which have not been hit
        int x_random = Random.Range(0, board_width);
        int y_random = Random.Range(0, board_height);
        if (computerHitMemory[x_random, y_random] == 0)
            return new Tuple<int, int>(x_random, y_random);
        else 
            return validNeightbourCell(x_random, y_random);
    }

    public void shootAtSpecificPlayerField(int x, int y)
    {
        int ship_index = playerField[x, y];
        if (ship_index > 0)
        {
            print($"Opponent hit at {x} {y}.");
            AudioManager.Instance.Hit();
            computerHitMemory[x, y] = 1;
            lastHit = new Tuple<int, int>(x, y);

            //visualize damage for player
            ShipData ship = getShipAt(x, y);
            ship.OnDamage();

            playerField[x, y] = -ship_index;
            if (!containsMatch(playerField, ship_index, false))
            {
                //ship destroyed, increase score
                print($"Opponent destroyed ship with index {ship_index}.");
                uiManager.PlayerHit();

                cascadeShipDestructionInMemory(x, y);
                lastHit = null;
            }
        } else if (ship_index < 0) {
            print($"Opponent missed at {x} {y}. This location holds a destroyed player ship component.");
            AudioManager.Instance.Miss();
        } else {
            print($"Opponent missed at {x} {y}.");
            computerHitMemory[x, y] = -1;
            AudioManager.Instance.Miss();
        }

        // highlight via MySelectable
        GameObject targeted_cell = GameObject.Find($"Cell_{x + gridManager.xOffset}_{y + gridManager.yOffset}");
        MySelectable cell_selectable = targeted_cell.transform.gameObject.GetComponent<MySelectable>();
        cell_selectable.flashCell();

        StartCoroutine(DeselectCellAfterDelay(cell_selectable, 2f));
    }

    private void cascadeShipDestructionInMemory(int x, int y)
    {
        // when computer destroys a player ship, computerHitMemory array is updated to no longer attempt to shoot at the same ship
        // this basic cascade doesnt check for plausible orientation etc, i.e. can also "forget" other hits, in contact with destroyed ship

        if (!coordsInBounds(x, y))
        {
            print($"Warning: Memory cascade reached out of bounds coordinates {x} {y}.");
            return;
        }

        computerHitMemory[x, y] = -2;

        (int, int)[] options = surroundingCoords(x, y);
        foreach ((int, int) pair in options)
        {
            if (coordsInBounds(pair.Item1, pair.Item2) && computerHitMemory[pair.Item1, pair.Item2] > 0)
                cascadeShipDestructionInMemory(pair.Item1, pair.Item2);
        }
    }

    private (int, int)[] surroundingCoords(int x, int y)
    {
        (int, int)[] options = {(x-1, y), (x+1, y), (x, y-1), (x, y+1)};
        return options;
    }

    private bool coordsInBounds(int x, int y)
    {
        return (x >= 0 && x < board_width && y >= 0 && y < board_height);
    }

    private Tuple<int, int> validNeightbourCell(int x, int y)
    {
        (int, int)[] options = surroundingCoords(x, y);
        foreach ((int, int) pair in options)
        {
            if (coordsInBounds(pair.Item1, pair.Item2) && computerHitMemory[pair.Item1, pair.Item2] == 0)
                return new Tuple<int, int>(pair.Item1, pair.Item2);
        }
        return null;
    }

    private bool containsMatch(int[,] field, int check_val, bool check_val_as_lower_bound)
    {
        foreach (int val in field)
        {
            if (check_val_as_lower_bound && val >= check_val)
                return true;
            if (val == check_val)
                return true;
        }
        return false;
    }

    private ShipData getShipAt(int x, int y)
    {
        int fixed_x = x + gridManager.xOffset;
        int fixed_y = y + gridManager.yOffset;
        List<ShipData> ship_candidate_pos_lists = playerManager.ships;
        foreach (ShipData candidate in ship_candidate_pos_lists)
        {
            foreach (Vector3 position in candidate.positions)
            {
                if (position.x == fixed_x && position.y == fixed_y)
                    return candidate;
            }
        }
        print($"Warning: No ship found at {x} {y}.");
        return null;
    }

    private IEnumerator DeselectCellAfterDelay(MySelectable cell, float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Deselect the cell
        cell.flashCellDeselect();
    }

    [ContextMenu("Log Opponent Positions")]
    public void logOpponentPositions()
    {
        logField("opponentField", opponentField);
    }

    [ContextMenu("Log Player Positions")]
    public void logPlayerPositions()
    {
        logField("playerField", playerField);
    }

    [ContextMenu("Trigger Game Over")]
    public void gameOver()
    {
        // gameOverScreen.SetActive(true);
        print("Testpoint - gameOver");
        gameIsOver = true;
    }

    [ContextMenu("Trigger Win")]
    public void gameWin()
    {
        // gameWinScreen.SetActive(true);
        print("Testpoint - gameWin");
        gameIsOver = true;
    }

    [ContextMenu("Quit Game (Ingame)")]
    public void ingameQuitGame()
    {
        Application.Quit();
    }

    [ContextMenu("Test-Shots //at Opponent")]
    public void testShots()
    {
        //helper function for debugging win condition
        for (int i = 0; i < board_width; i++)
        {
            for (int j = 0; j < board_height; j++)
            {
                shootAtOpponentField(new Vector3(i, j, -1));
            }
        }
    }

    [ContextMenu("Test-Shots //at Player")]
    public void testOpposingShots()
    {
        //helper function for debugging situation where player ships are hit
        for (int i = 0; i < board_width; i++)
        {
            for (int j = 0; j < board_height / 2; j++)
            {
                shootAtSpecificPlayerField(i, j);
            }
        }
    }
}

