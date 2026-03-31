using UnityEngine;
using System.Collections.Generic;
public class Combat : MonoBehaviour
{
    public static Combat Instance;

    public GameObject cannonballPrefab;

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
        for(int i = 0; i<firingRange; i++) 
        {
            Vector3Int dirs = BoatController.GetDirs(cell.y, dir, 1);
            cell+=dirs;
            //print("boat " + boat.name + "is firing to cell" + cell);
            foreach (BoatController b in TurnManager.Instance.boats)
            {
                if (b.currentCell == cell)
                {
                    print("boat " +b.name + " has been hit :(");
                    b.takeDamage();
                    SpawnProjectile(boat, cell);
                    return;
                }
            }
        } 
        SpawnProjectile(boat, cell);   
    }

    void SpawnProjectile(BoatController boat, Vector3 targetPos)
    {
        Vector3 spawn = boat.transform.position;
        spawn.z = 0f;
        Vector3 worldTarget = boat.tilemap.GetCellCenterWorld(Vector3Int.RoundToInt(targetPos));
        GameObject proj = Instantiate(cannonballPrefab, spawn, Quaternion.identity);
        Projectile projScript = proj.GetComponent<Projectile>();
        projScript.Init(worldTarget);
    }
}
