using UnityEngine;
using System.Collections.Generic;
public class Combat : MonoBehaviour
{
    public static Combat Instance;

    public int firingRange = 3;

    void Awake()
    {
        Instance = this;
    }

    public void Fire(BoatController boat, FireCommand cmd)
    {
        int dir = boat.FiringDirection(cmd.fireCommandType);
        if (dir == -1)
        {
            print("something has gone horribly wrong...");
            return;
        }
        
        Vector3Int cell = boat.currentCell;
        for(int i = 0; i<firingRange; i++) // change this loop var to adjust range
        {
            var dirs = boat.GetDirs(cell.y);
            cell+=dirs[dir];


        }


        foreach (BoatController b in TurnManager.Instance.boats)
        {
            if (b.currentCell == cell)
            {
                b.takeDamage();
                return;
            }
        }
    }
    
}

