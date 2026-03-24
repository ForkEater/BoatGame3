using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/*
Overview: 
Run when execute button is pressed; add enemy ai orders when players ahve been submitted

Check each possible command it can enter; 
- If it would hit an island or collide with another boat, continue (if all commands would do so, have harm-minimization (if you have 1 hp & sinking 2hp enemy, do that, etc.))
- If a move would let you shoot an enemy boat, do that (bonus if you hit more enemy boats), unless you would hit one of your own boats (unless you are hitting more enemies than allies kind of thing)
- If can't hit any enemies, choose move that will face you towards the closest enemy boat (except straight-on; if you are close enough to the enemy we want to turn to broadside). 
  - If straight ahead, accelerate unless doing so would force you to crash 

*/

public class EnemyPathfinding: MonoBehaviour
{
    public Tilemap tilemap;

    private Vector3Int[] getPhantomLocation()
    {
        /*Find future location according to order queue, update phantomCell*/
        return null;
    }

    private bool willHitIsland(BoatCommand command)
    {
        /*Check if will hit island with current speed and facing if it doesn't turn with its next action*/
        return false;
    }

    private int enemyHits(BoatCommand move, FireCommand shoot)
    {
        /*Returns how many enemy boats hit with current commands (minus hits to allies)*/
        return 0;
    }

    public static (BoatCommand, FireCommand) chooseCommand(BoatController boat)
    {
        /**/
        return (new BoatCommand(BoatCommandType.Nothing), new FireCommand(FireCommandType.Nothing));
    }
}
