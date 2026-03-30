using UnityEngine;
using System.Collections.Generic;

public class Combat : MonoBehaviour
{
    public static Combat Instance;

    public int firingRange = 3;

    public GameObject projectilePrefab;

    void Awake()
    {
        Instance = this;
    }
    public void Fire(BoatController boat, FireCommand cmd)
{
    int dir = boat.FiringDirection(cmd.fireCommandType);
    if (dir == -1)
    {
        print("Something has gone horribly wrong...");
        return;
    }

    Vector3Int cell = boat.currentCell;
    var dirs = boat.GetDirs(boat.currentCell.y);

    BoatController hitBoat = null;

    // Loop up to firingRange
    for (int i = 1; i <= firingRange; i++)
    {
        cell += dirs[dir]; // move one cell in direction

        // Stop if cell is invalid
        if (!boat.tilemap.HasTile(cell)) break;

        // Check for a boat in this cell
        foreach (BoatController b in TurnManager.Instance.boats)
        {
            if (b.currentCell == cell)
            {
                hitBoat = b;
                break;
            }
        }

        if (hitBoat != null) break;
    }

    // Determine target position
    Vector3 targetPos;

    if (hitBoat != null)
    {
        targetPos = hitBoat.transform.position;
    }
    else
    {
        // Convert last cell to world position
        targetPos = boat.tilemap.GetCellCenterWorld(cell);
    }

    // Spawn projectile toward targetPos
    SpawnProjectile(boat, hitBoat, targetPos);
}

    void SpawnProjectile(BoatController shooter, BoatController target, Vector3 targetPos)
{
    Vector3 spawnPos = shooter.transform.position;
    spawnPos.z = 0f; // for 2D

    GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

    Projectile p = proj.GetComponent<Projectile>();
    p.Init(targetPos, target); // works even if target is null
}
}