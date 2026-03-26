using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

/*
Overview: 
Run when execute button is pressed; add enemy ai orders when players ahve been submitted

Check each possible command it can enter; 
- If it would hit an island or collide with another boat, ignore (if all commands would do so, have harm-minimization (if you have 1 hp & sinking 2hp enemy, do that, etc.))
- If a move would let you shoot an enemy boat, do that (bonus if you hit more enemy boats), unless you would hit one of your own boats (unless you are hitting more enemies than allies kind of thing)
- If can't hit any enemies, choose move that will face you towards the closest enemy boat (except straight-on; if you are close enough to the enemy we want to turn to broadside). 
  - If straight ahead, accelerate unless doing so would force you to crash 

*/

public class EnemyPathfinding: MonoBehaviour
{
    public Tilemap tilemap;

    private int updateSpeed(int phantomSpeed, BoatCommand command)
    {
        if (command.commandType == BoatCommandType.Forward)
        {
            phantomSpeed += 1;
        } else if (command.commandType == BoatCommandType.Backward && phantomSpeed > 0)
        {
            phantomSpeed -= 1;
        }
        return phantomSpeed;
    }

    private static (Vector3Int[], int, int) getPhantomLocationFromQueue(BoatController boat, int moveCount)
    {
        if (boat.commandQueue.Count < moveCount)
        {
            print("Ya fucked up somewhere");
            return (boat.currentCell, 0, 0);
        }
        /*Find future location according to order queue*/
        newFace = boat.GetFacing();
        phantomCell = boat.currentCell;
        phantomSpeed = boat.speed;
        for (int i = 0; i<moveCount; i++)
        {
            // Modify speed
            phantomSpeed = updateSpeed(phantomSpeed, boat.commandQueue[i]);

            // Find new position based on first x commands
            for (int j = 0; j < phantomSpeed; j++)
            {
                if (tilemap.HasTile(phantomCell + GetDirs(phantomCell.y, newFace, phantomSpeed) && !isIsland(phantomCell + GetDirs(phantomCell.y, newFace, phantomSpeed))))
                {
                    phantomCell = phantomCell + GetDirs(phantomCell.y, newFace, phantomSpeed);
                }
                
            }

            // Account for turning
            if (boat.commandQueue[i].commandType == BoatCommandType.Left)
            {
                newFace = (newFace + 5) % 6;
            } else if (boat.commandQueue[i].commandType == BoatCommandType.Right)
            {
                newFace = (newFace + 1) % 6;
            }
        }
        return (phantomCell, newFace, phantomSpeed);
    }

    private static Vector3Int[] getPhantomLocation(Vector3Int[] pos, int facing, int phantomSpeed, BoatCommand command)
    {
        phantomSpeed = updateSpeed(phantomSpeed, command);
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (!tilemap.HasTile(pos + GetDirs(pos.y, facing, phantomSpeed)) && !isIsland(pos + GetDirs(pos.y, facing, phantomSpeed)))
            {
                pos = pos + GetDirs(pos.y, facing, phantomSpeed);
            }
        }
        return pos;
    }

    private static Vector3Int[][] getPhantomPath(Vector3Int[] pos, int facing, int phantomSpeed, BoatCommand command)
    {
        phantomSpeed = updateSpeed(phantomSpeed, command);
        path = [];
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (!tilemap.HasTile(pos + GetDirs(pos.y, facing, phantomSpeed)) && !isIsland(pos + GetDirs(pos.y, facing, phantomSpeed)))
            {
                pos = pos + GetDirs(pos.y, facing, phantomSpeed);
            }
            path.append(pos);
        }
        return pos;
    }

    private static BoatController[] willHitAlly(BoatController[] boats)
    {
        //
        return [];
    }

    private static bool[] willHitIsland(Vector3Int[] pos, int facing, int phantomSpeed, BoatCommand command)
    {
        /*Check if will hit island in the next move, given a position and speed*/
        phantomSpeed = updateSpeed(phantomSpeed, command);
        path = [false]*phantomSpeed;
        currentPos = pos;
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (!tilemap.HasTile(currentPos + GetDirs(currentPos.y, facing, phantomSpeed)) || isIsland(currentPos + GetDirs(currentPos.y, facing, phantomSpeed)))
            {
                path[i] = true;
                currentPos = currentPos + GetDirs(currentPos.y, facing, phantomSpeed);
            }
        }
        return path;
    }

    private static bool isIsland(Vector3Int cell)
    {
        return false;
    }

    public static int firingDirectionFromPosition(FireCommandType cmd, int facing)
    {
        switch (cmd)
        {
            case FireCommandType.FireFrontLeft:
                return (facing+5)%6;
            case FireCommandType.FireFrontRight:
                return (facing+1)%6;
            case FireCommandType.FireBackRight:
                return (facing+2)%6;
            case FireCommandType.FireBackLeft:
                return (facing+4)%6;  
        }   
        return -1;
    }

    private static int enemyHits(Vector3Int[] pos, int facing, int phantomSpeed, BoatCommand move, FireCommand shoot)
    {
        /*Returns how many enemy boats hit with current commands (minus hits to allies)*/
        targets = [];
        firingDirection = firingDirectionFromPosition(shoot.fireCommandType, facing);
        currentBoatPosition = pos;
        phantomSpeed = updateSpeed(phantomSpeed, move);
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (tilemap.HasTile(currentBoatPosition + GetDirs(cureentBoatPosition.y, facing, phantomSpeed)) && !isIsland(currentBoatPosition + GetDirs(currentBoatPosition.y, facing, phantomSpeed)))
            {
                currentBoatPosition = currentBoatPosition + GetDirs(currentBoatPosition.y, facing, phantomSpeed);
            }
            targetCell = currentBoatPosition;
            round = [];
            for (int j = 0; j < Combat.Instance.firingRange; j++)
            {
                targetCell = targetCell + GetDirs(targetCell.y, firingDirection, 1);
                if (tilemap.HasTile(targetCell))
                {
                    round.append(targetCell);
                }
            }
            targets.append(round);
        }

        hits = 0;
        goodBoatPaths = [];
        evilBoatPaths = [];

        foreach (BoatController b in TurnManager.Instance.boats)
        {
            (pos, facing, speed) = getPhantomLocationFromQueue(b, 2);
            path = getPhantomPath(pos, facing, speed, b.commandQueue[-1]);
            if (path.length == 1)
            {
                path = [path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0]];
            } else if (path.length == 2)
            {
                path = [path[0], path[0], path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[1], path[1]];
            } else if (path.length == 3)
            {
                path = [path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[2], path[2], path[2], path[2]];
            } else if (path.length == 4)
            {
                path = [path[0], path[0], path[0], path[1], path[1], path[1], path[2], path[2], path[2], path[3], path[3], path[3]];
            }
            if (b.isEvil)
            {
                evilBoatPaths.append(path);
            } else
            {
                goodBoatPaths.append(path);
            }
            
        }
        // 12-tick switch
        for (int i = 1; i <= 12; i++)
        {
            switch(i)
            {
                case 1: 
                    break;
                case 2: 
                    break;
                case 3: 
                    if (targets.length == 4)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
                case 4: 
                    if (targets.length == 3)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
                case 5: 
                    break;
                case 6: 
                    if (targets.length == 2)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[0][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    if (targets.length == 4)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
                case 7: 
                    break;
                case 8: 
                    if (targets.length == 3)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
                case 9: 
                    if (targets.length == 4)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[2][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[2][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
                case 10: 
                    break;
                case 11: 
                    break;
                case 12: 
                    if (targets.length == 4)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[3][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[3][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    if (targets.length == 3)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[2][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[2][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    if (targets.length == 2)
                    {
                        for (int j = 0; j < Combat.Instance.firingRange; j++)
                        {
                            foreach (Vector3Int[] traj in goodBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits++;
                                }
                            }
                            foreach (Vector3Int[] traj in evilBoatPaths)
                            {
                                if (traj[i] == targets[1][j])
                                {
                                    hits--;
                                }
                            }
                        }
                    }
                    break;
            }
        }

        return hits;
    }

    public static void collectEnemyOrders(BoatController[] boats)
    {
        scratches = []; // Orders that cannot be allowed (format (boat, move))
        foreach (BoatCommand boat in boats)
        {
            (BoatCommand move, FireCommand shoot) = EnemyPathfinding.chooseCommand(boat, scratches); 
            boat.AddCommand(move);
            boat.AddFireCommand(shoot); 
        }
        /* Check for crash between evil boats, if yes add to scratch list */
        /* while crashes, choose new orders */
        /* If all orders crash, something went wrong, just take highest value using empty scratch list*/
    }

    public static (BoatCommand, FireCommand) chooseCommand(BoatController boat, (BoatController, BoatCommand)[] scratches)
    {
        (Vector3Int[] pos, int facing, int phantomSpeed) = getPhantomLocationFromQueue(boat, 2);

        moveOptions = [new BoatCommand(BoatCommandType.Forward), new BoatCommand(BoatCommandType.Backward), new BoatCommand(BoatCommandType.RotateLeft), new BoatCommand(BoatCommandType.RotateRight), new BoatCommand(BoatCommandType.Nothing)];
        fireOptions = [ new FireCommand(FireCommandType.FireFrontRight), new FireCommand(FireCommandType.FireFrontLeft), new FireCommand(FireCommandType.FireBackRight), new FireCommand(FireCommandType.FireBackLeft), new FireCommand(FireCommandType.Nothing)];
        orders = [(null, null, 0)]*moveOptions.length*fireOptions.length;
        index = 0;
        willCrashNextTurn = false;

        for (int i = 0; i < moveOptions.length; i++)
        {
            for (int j = 0; j < fireOptions.length; j++)
            {
                /* Ignore orders that have previously resulted in crashes*/
                skip = false;
                for (int k = 0; k < scratches.length; k++)
                {
                    if (boat == scratches[k][0] && moveOptions[i] == scratches[k][1])
                    {
                        skip = true;
                    }
                }
                if (skip)
                {
                    continue;
                }

                // Consider options
                move = moveOptions[i];
                shoot = fireOptions[j];
                value = 0;

                if (willCrashNextTurn && move.commandType != BoatCommandType.RotateLeft && move.commandType != BoatCommandType.RotateRight)
                {
                    value-=30;
                } 
                if (move.commandType == BoatCommandType.Forward && willHitIsland(getPhantomLocation(pos, facing, phantomSpeed, move), facing, phantomSpeed+1, move)[-1] == true)
                {
                    value -= 50;
                }

                hits = enemyHits();
                if (hits < 0)
                {
                    value -= 100;
                } else
                {
                    value += hits*20;
                }
                


                orders[index] = (moveOptions[i], fireOptions[j], value);
                index += 1;
            }
        }

        
        return (new BoatCommand(BoatCommandType.Nothing), new FireCommand(FireCommandType.Nothing));
    } 
}

/* Have check at end that if boat will crash into allied boat on next round, reconsider ordeers and the boats that would crash must choose a different previous command*/