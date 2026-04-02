using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public enum gameType
{
    Singleplayer,
    LocalMultiplayer, 
    LANMultiplayer 
}


public class TurnManager : MonoBehaviour
{
    public Tilemap tilemap;
    public static TurnManager Instance;
    private BoatActions actions;
    public gameType gameMode;
    public float pauseTime = 0.5f;

    public bool ordersOpen = true;

    public List<BoatController> boats = new List<BoatController>();
    public List<BoatController> goodBoats = new List<BoatController>();
    public List<BoatController> evilBoats = new List<BoatController>();
    public List<BoatController> deadBoats = new List<BoatController>();
    public GameObject deadBoatPrefab;
    public GameObject islandHex;

    void Awake()
    {
        Instance = this;

        actions = new BoatActions();
        actions.Movement.Execute.performed += ctx =>
        {
            callExecuteTurn();
        };
    }

    void Start()
    {
        foreach (Vector3Int island in Islandmaker.Instance.allIslands)
        {
            Vector3 spawn = tilemap.GetCellCenterWorld(Vector3Int.RoundToInt(island));
            GameObject proj = Instantiate(islandHex, spawn, Quaternion.identity);
        }
    }

    public void callExecuteTurn()
    {
        if (!ordersOpen){
            return;
        }
        
        if (gameMode == gameType.Singleplayer)
        {
            if (EnemyPathfinding.tilemap == null)
            {
                EnemyPathfinding.tilemap = boats[0].tilemap;
            }
            foreach (BoatController boat in goodBoats)
            {
                while (boat.commandQueue.Count < boat.maxCommands)
                {
                    boat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
                }
                while(boat.fireQueue.Count < boat.maxFireCommands)
                {
                    boat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
                }
            }
        } else {
            foreach (BoatController boat in boats)
            {
                while (boat.commandQueue.Count < boat.maxCommands)
                {
                    boat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
                }
                while(boat.fireQueue.Count < boat.maxFireCommands)
                {
                    boat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
                }
            }
        }
        StartCoroutine(ExecuteTurn());
    }

    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }

    private void shoot(BoatController boat)
    {
        if(boat.fireQueue.Count == 0)
        {
            return;
        }
        FireCommand fire = boat.fireQueue[0];
        if (fire.fireCommandType != FireCommandType.Nothing)
        {
            Combat.Instance.Fire(boat, fire);
        }
    }

    public System.Collections.IEnumerator ExecuteTurn()
    {
        if (gameMode == gameType.Singleplayer)
        {
            EnemyPathfinding.collectEnemyOrders(evilBoats);
        }
        ordersOpen = false;
        for (int i = 1; i <= 12; i++)
        {
            foreach (BoatController boat in boats)
            {
                switch (i)
                {
                    case 1:
                        if (boat.commandQueue[0].commandType == BoatCommandType.Forward)
                        {
                            boat.speed += 1;
                        }
                        else if (boat.commandQueue[0].commandType == BoatCommandType.Backward)
                        {
                            boat.speed -= 1;
                        }
                        break;
                    case 3:
                        if (boat.speed == 4)
                        {
                            boat.Forward();
                            shoot(boat);
                        }
                        break;
                    case 4:
                        if (boat.speed == 3)
                        {
                            boat.Forward();
                            shoot(boat);
                        }
                        break;
                    case 6:
                        if (boat.speed == 2 || boat.speed == 4)
                        {
                            boat.Forward();
                            shoot(boat);
                        }
                        else if (boat.speed == -2)
                        {
                            boat.Backward();
                            shoot(boat);
                        }
                        else if (boat.speed == 0 || boat.speed == -1 || boat.speed == 1)
                        {
                            shoot(boat);
                        }
                        break;
                    case 8:
                        if (boat.speed == 3)
                        {
                            boat.Forward();
                            shoot(boat);
                        }
                        break;
                    case 9:
                        if (boat.speed == 4)
                        {
                            boat.Forward();
                            shoot(boat);
                        }
                        break;
                    case 12:
                        if (boat.speed > 0)
                        {
                            boat.Forward();
                        }
                        else if (boat.speed < 0)
                        {
                            boat.Backward();
                        }
                        break;
                }
                if (!boat.hasCrashed && boat.CheckCollision())
                {
                    boat.takeDamage();
                    boat.hasCrashed = true;
                }
                if (boat.CheckIslandCollision())
                {
                    if(!boat.hasCrashed){
                        boat.takeDamage();
                    }
                    boat.hasCrashed = true;
                    boat.speed = 0;
                    boat.Backward();
                }
                if (i == 12)
                {
                    if (boat.commandQueue[0].commandType == BoatCommandType.RotateLeft)
                    {
                        boat.RotateLeft();
                    }
                    else if (boat.commandQueue[0].commandType == BoatCommandType.RotateRight)
                    {
                        boat.RotateRight();
                    }
                }
            }
            yield return new WaitForSeconds(pauseTime);
        }
        
        foreach (BoatController boat in boats)
        {

            boat.hasCrashed = false;
            if (boat.CheckCollision()) 
            {
                boat.takeDamage();
                boat.takeDamage();
            }
            shoot(boat);
            boat.commandQueue.RemoveAt(0);
            if (boat.fireQueue.Count > 0)
            {
                boat.fireQueue.RemoveAt(0);
            }
        }
                
        yield return new WaitForSeconds(pauseTime);

        foreach (BoatController boat in deadBoats)
        {
            if (BoatSelection.SelectedBoat == boat)
            {
                BoatSelection.SelectedBoat = null;
            }
            boats.Remove(boat);
            if (boat.isEvil){
                evilBoats.Remove(boat);
            } else {
                goodBoats.Remove(boat);
            }
            boat.destroyBoat();
        }
        deadBoats.Clear();
        CheckWinCondition();
        yield return new WaitForSeconds(pauseTime*4);
        ordersOpen = true;
        //print("Turn over");
        foreach (BoatController boat in goodBoats)
        {
            boat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
            boat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
        }
        if (gameMode != gameType.Singleplayer)
        {
            foreach (BoatController boat in evilBoats)
            {
                boat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
                boat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
            }
        }
    }
    private void CheckWinCondition()
    {
        if (goodBoats.Count == 0)
        {
            SceneManager.LoadScene("WinEvilBoat");
        }
        else if (evilBoats.Count == 0)
        {
            SceneManager.LoadScene("WinGoodBoat");
        }
    }
    public void killAllGood()
    {
        foreach (BoatController boat in goodBoats)
        {
            boat.takeDamage();
            boat.takeDamage();
        }
    }
    public void killAllEvil()
    {
        foreach (BoatController boat in evilBoats)
        {
            boat.takeDamage();
            boat.takeDamage();
        }
    }
}