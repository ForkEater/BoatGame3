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
    public static Tilemap tilemap = null;

    private static int updateSpeed(int phantomSpeed, BoatCommand command)
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

    private static (Vector3Int, int, int) getPhantomLocationFromQueue(BoatController boat, int moveCount)
    {
        if (boat.commandQueue.Count < moveCount)
        {
            print("Ya fucked up somewhere dimwit");
            return (boat.currentCell, 0, 0);
        }
        /*Find future location according to order queue*/
        int newFace = boat.GetFacing();
        Vector3Int phantomCell = boat.currentCell;
        int phantomSpeed = boat.speed;
        for (int i = 0; i<moveCount; i++)
        {
            // Modify speed
            phantomSpeed = updateSpeed(phantomSpeed, boat.commandQueue[i]);

            // Find new position based on first x commands
            for (int j = 0; j < phantomSpeed; j++) 
            {
                if (tilemap.HasTile(phantomCell + BoatController.GetDirs(phantomCell.y, newFace, phantomSpeed)) && !isIsland(phantomCell + BoatController.GetDirs(phantomCell.y, newFace, phantomSpeed)))
                {
                    phantomCell = phantomCell + BoatController.GetDirs(phantomCell.y, newFace, phantomSpeed);
                }
            }

            // Account for turning
            if (boat.commandQueue[i].commandType == BoatCommandType.RotateLeft)
            {
                newFace = (newFace + 5) % 6;
            } else if (boat.commandQueue[i].commandType == BoatCommandType.RotateRight)
            {
                newFace = (newFace + 1) % 6;
            }
        }
        return (phantomCell, newFace, phantomSpeed);
    }

    private static Vector3Int getPhantomLocation(Vector3Int pos, int facing, int phantomSpeed, BoatCommand command)
    {
        phantomSpeed = updateSpeed(phantomSpeed, command);
        Vector3Int newPos = pos;
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (!tilemap.HasTile(newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed)) && !isIsland(newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed)))
            {
                newPos = newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed);
            }
        }
        return newPos;
    }

    private static List<Vector3Int> getPhantomPath(Vector3Int pos, int facing, int phantomSpeed, BoatCommand command)
    {
        phantomSpeed = updateSpeed(phantomSpeed, command);
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int newPos = pos;
        int phanSpeed = phantomSpeed;
        if (phantomSpeed < 0)
        {
            phanSpeed = -phanSpeed;
        } else if (phantomSpeed == 0)
        {
            path.Add(pos);
            return path;
        }
        for (int i = 0; i < phanSpeed; i++)
        {
            if (!tilemap.HasTile(newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed)) && !isIsland(newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed)))
            {
                newPos = newPos + BoatController.GetDirs(newPos.y, facing, phantomSpeed);
            } 
            path.Add(newPos);
        }
        return path;
    }

    private static List<BoatController> willHitAlly(List<BoatController> boats)
    {
        List<Vector3Int[]> boatPaths = new List<Vector3Int[]>();
        foreach (BoatController b in boats)
        {
            (Vector3Int pos, int facing, int speed) = getPhantomLocationFromQueue(b, 3);
            List<Vector3Int> path = getPhantomPath(pos, facing, speed, new BoatCommand(BoatCommandType.Nothing));
            if (path.Count == 1)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0]};
                boatPaths.Add(path12);
            } else if (path.Count == 2)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[1], path[1]};
                boatPaths.Add(path12);
            } else if (path.Count == 3)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[2], path[2], path[2], path[2]};
                boatPaths.Add(path12);
            } else if (path.Count == 4)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[1], path[1], path[1], path[2], path[2], path[2], path[3], path[3], path[3]};
                boatPaths.Add(path12);
            } else
            {
                Vector3Int[] path12 = {};
                print("Ya fucked up somewhere dolt");
            }
        }
        for (int i = 0; i < 12; i++)
        {
            List<Vector3Int> positions = new List<Vector3Int>();
            foreach (Vector3Int[] pathe in boatPaths)
            {
                positions.Add(pathe[i]);
            }
            for (int j = 0; j < positions.Count; j++)
            {
                for (int k = j+1; k < positions.Count; k++)
                {
                    if (positions[j] == positions[k])
                    {
                        List<BoatController> crashes = new List<BoatController>();
                        crashes.Add(boats[j]);
                        crashes.Add(boats[k]);
                        print("Crashes will happen");
                        return crashes;
                    }
                }
            }
        }
        //print("No crashes"); 
        return new List<BoatController>();
    }

    private static List<bool> willHitIsland(Vector3Int pos, int facing, int phantomSpeed, BoatCommand command)
    {
        /*Check if will hit island in the next move, given a position and speed*/
        phantomSpeed = updateSpeed(phantomSpeed, command);
        List<bool> path = new List<bool>();
        for (int i = 0; i < phantomSpeed; i++)
        {
            path.Add(false);
        }
        Vector3Int currentPos = pos;
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (!tilemap.HasTile(currentPos + BoatController.GetDirs(currentPos.y, facing, phantomSpeed)) || isIsland(currentPos + BoatController.GetDirs(currentPos.y, facing, phantomSpeed)))
            {
                path[i] = true;
                currentPos = currentPos + BoatController.GetDirs(currentPos.y, facing, phantomSpeed);
            }
        }
        return path;
    }

    private static bool isIsland(Vector3Int cell)
    {
        foreach (Vector3Int island in Islandmaker.Instance.allIslands)
        {
            if (cell == island)
            {
                return true;
            }
        }
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

    private static int enemyHits(Vector3Int pos, int facing, int phantomSpeed, BoatCommand move, FireCommand shoot)
    {
        /*Returns how many enemy boats hit with current commands (minus hits to allies)*/
        List<List<Vector3Int>> targets = new List<List<Vector3Int>>();
        int firingDirection = firingDirectionFromPosition(shoot.fireCommandType, facing);
        Vector3Int currentBoatPosition = pos;
        phantomSpeed = updateSpeed(phantomSpeed, move);
        for (int i = 0; i < phantomSpeed; i++)
        {
            if (tilemap.HasTile(currentBoatPosition + BoatController.GetDirs(currentBoatPosition.y, facing, phantomSpeed)) && !isIsland(currentBoatPosition + BoatController.GetDirs(currentBoatPosition.y, facing, phantomSpeed)))
            {
                currentBoatPosition = currentBoatPosition + BoatController.GetDirs(currentBoatPosition.y, facing, phantomSpeed);
            }
            Vector3Int targetCell = currentBoatPosition;
            List<Vector3Int> round = new List<Vector3Int>();
            for (int j = 0; j < Combat.Instance.firingRange; j++)
            {
                targetCell = targetCell + BoatController.GetDirs(targetCell.y, firingDirection, 1);
                if (tilemap.HasTile(targetCell))
                {
                    round.Add(targetCell);
                }
            }
            //print(round.Count);
            targets.Add(round);
        }

        int hits = 0;
        List<Vector3Int[]> goodBoatPaths = new List<Vector3Int[]>();
        List<Vector3Int[]> evilBoatPaths = new List<Vector3Int[]>();

        foreach (BoatController b in TurnManager.Instance.boats)
        {
            (Vector3Int bpos, int bfacing, int bspeed) = getPhantomLocationFromQueue(b, b.commandQueue.Count);
            List<Vector3Int> path = getPhantomPath(bpos, bfacing, bspeed, new BoatCommand(BoatCommandType.Nothing));
            if (path.Count == 1)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0], path[0]};
                if (b.isEvil)
                {
                    evilBoatPaths.Add(path12);
                } else
                {
                    goodBoatPaths.Add(path12);
                }
            } else if (path.Count == 2)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[1], path[1]};
                if (b.isEvil)
                {
                    evilBoatPaths.Add(path12);
                } else
                {
                    goodBoatPaths.Add(path12);
                }
            } else if (path.Count == 3)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[0], path[1], path[1], path[1], path[1], path[2], path[2], path[2], path[2]};
                if (b.isEvil)
                {
                    evilBoatPaths.Add(path12);
                } else
                {
                    goodBoatPaths.Add(path12);
                }
            } else if (path.Count == 4)
            {
                Vector3Int[] path12 = {path[0], path[0], path[0], path[1], path[1], path[1], path[2], path[2], path[2], path[3], path[3], path[3]};
                if (b.isEvil)
                {
                    evilBoatPaths.Add(path12);
                } else
                {
                    goodBoatPaths.Add(path12);
                }
            } else
            {
                Vector3Int[] path12 = {};
                print("Ya fucked up somewhere");
                if (b.isEvil)
                {
                    evilBoatPaths.Add(path12);
                } else
                {
                    goodBoatPaths.Add(path12);
                }
            }
        }
        // 12-tick switch
        //print("t " + targets.Count);
        for (int i = 0; i < 12; i++) 
        {
            switch(i)
            {
                case 0: 
                    break;
                case 1: 
                    break;
                case 2: 
                    if (targets.Count == 4)
                    {
                        for (int j = 0; j < targets[0].Count; j++)
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
                case 3: 
                    if (targets.Count == 3)
                    {
                        for (int j = 0; j < targets[0].Count; j++)
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
                    break;
                case 5: 
                    if (targets.Count == 2)
                    {
                        for (int j = 0; j < targets[0].Count; j++)
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
                    if (targets.Count == 4)
                    {
                        for (int j = 0; j < targets[1].Count; j++)
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
                case 6: 
                    break;
                case 7: 
                    if (targets.Count == 3)
                    {
                        for (int j = 0; j < targets[1].Count; j++)
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
                case 8: 
                    if (targets.Count == 4)
                    {
                        for (int j = 0; j < targets[2].Count; j++)
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
                case 9: 
                    break;
                case 10: 
                    break;
                case 11: 
                    if (targets.Count == 4)
                    {
                        for (int j = 0; j < targets[3].Count; j++)
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
                    if (targets.Count == 3)
                    {
                        for (int j = 0; j < targets[2].Count; j++)
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
                    if (targets.Count == 2)
                    {
                        for (int j = 0; j < targets[1].Count; j++)
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

    public static void collectEnemyOrders(List<BoatController> boats)
    {
        List<(BoatController, BoatCommand)> scratches = new List<(BoatController, BoatCommand)>(); 
        bool safeMoves = false;
        while (!safeMoves)
        {
            for (int i = 0; i < boats.Count; i++)
            {
                BoatController boat = boats[i]; 
                while (boat.commandQueue.Count < boat.maxCommands)
                {
                    (BoatCommand move, FireCommand shoot) = chooseCommand(boat, scratches); 
                    boat.AddCommand(move);
                    boat.AddFireCommand(shoot); 
                }
            }
            // Check for crashes next round; if none break, else add to scratches and repeat (unless sratches is too long, then clear scratches and go and take best move for each boat)
            List<BoatController> crashes = willHitAlly(boats);
            if (crashes.Count == 0)
            {
                safeMoves = true; 
                //print("Safe moves found");
                break;
            } else
            {
                //print("Moves not safe");
                foreach (BoatController b in crashes)
                {
                    scratches.Add((b, b.commandQueue[b.commandQueue.Count - 1]));
                }
                if (scratches.Count >= 5)
                {
                    scratches.Clear();
                    foreach (BoatController b in boats)
                    {
                        (BoatCommand move, FireCommand shoot) = chooseCommand(b, scratches); 
                        b.AddCommand(move);
                        b.AddFireCommand(shoot); 
                    }
                    safeMoves = true; 
                }
            }
        }
    }

    public static (BoatCommand, FireCommand) chooseCommand(BoatController boat, List<(BoatController, BoatCommand)> scratches)
    {
        (Vector3Int pos, int facing, int phantomSpeed) = getPhantomLocationFromQueue(boat, boat.commandQueue.Count);

        BoatCommand[] moveOptions = {new BoatCommand(BoatCommandType.Forward), new BoatCommand(BoatCommandType.Backward), new BoatCommand(BoatCommandType.RotateLeft), new BoatCommand(BoatCommandType.RotateRight), new BoatCommand(BoatCommandType.Nothing)};
        FireCommand[] fireOptions = {new FireCommand(FireCommandType.FireFrontRight), new FireCommand(FireCommandType.FireFrontLeft), new FireCommand(FireCommandType.FireBackRight), new FireCommand(FireCommandType.FireBackLeft)};
        
        List<(BoatCommand, FireCommand, int)> orders = new List<(BoatCommand, FireCommand, int)>();

        for (int i = 0; i < moveOptions.Length*fireOptions.Length; i++)
        {
            orders.Add((null, null, 0));
        }

        int index = 0;
        
        bool willCrashNextTurn = false;

        for (int i = 0; i < moveOptions.Length; i++)
        {
            for (int j = 0; j < fireOptions.Length; j++)
            {
                // Ignore orders that have previously resulted in crashes
                bool skip = false;
                for (int k = 0; k < scratches.Count; k++)
                {
                    if (boat == scratches[k].Item1 && moveOptions[i] == scratches[k].Item2)
                    {
                        skip = true;
                    }
                }
                if (skip)
                {
                    continue;
                }

                BoatCommand move = moveOptions[i]; 
                FireCommand shoot = fireOptions[j];
                int value = 0;

                if (willCrashNextTurn && move.commandType != BoatCommandType.RotateLeft && move.commandType != BoatCommandType.RotateRight)
                {
                    value -= 30;
                } 
                List<bool> willCrash = willHitIsland(getPhantomLocation(pos, facing, phantomSpeed, move), facing, phantomSpeed+1, move);
                if (move.commandType == BoatCommandType.Forward && willCrash.Count > 0 && willCrash[willCrash.Count - 1] == true)
                {
                    value -= 50;
                }

                int hits = enemyHits(pos, facing, phantomSpeed, move, shoot);
                if (hits < 0)
                {
                    value -= 100;
                } else
                {
                    value += hits * 20;
                }

                orders[index] = (moveOptions[i], fireOptions[j], value);
                index += 1;
            }
        }
        int maxIndex = 0;
        for (int i = 1; i < orders.Count; i++)
        {
            if (orders[i].Item3 > orders[maxIndex].Item3)
            {
                maxIndex = i;
            }
        }
        if (orders[maxIndex].Item3 > 0)
        {
            return (orders[maxIndex].Item1, orders[maxIndex].Item2);
        }
        else
        {
            // Closest good ship
            BoatController closestGood = null;
            float minDist = float.MaxValue;
            for (int i = 0; i < TurnManager.Instance.goodBoats.Count; i++)
            {
                BoatController b = TurnManager.Instance.goodBoats[i];
                float dist = Vector3Int.Distance(pos, getPhantomLocationFromQueue(b, 3).Item1);
                if (dist < minDist)
                {
                    minDist = dist; 
                    closestGood = b;
                }
            }
            //print("cg" + closestGood);

            if (closestGood == null)
            {
                // Something weird happened
                print("Ya fucked up somewhere");
                return (new BoatCommand(BoatCommandType.Nothing), new FireCommand(FireCommandType.Nothing));
            }

            // Direction to closest good ship
            Vector3Int toTarget = getPhantomLocationFromQueue(closestGood, 3).Item1 - pos;
            int bestDir = -1;
            float bestDot = -1;
            Vector3 toTarget2D = new Vector2(toTarget.x, toTarget.y).normalized;
            //Vector3Int dirs = BoatController.GetDirs(pos.y, facing, phantomSpeed);
            for (int i = 0; i < 6; i++)
            {
                /*Vector3 dir2D = new Vector2(dirs.x, dirs.y).normalized;
                float dot = Vector3.Dot(dir2D, toTarget2D);*/
                Vector3Int dir = BoatController.GetDirs(pos.y, i, phantomSpeed);
                int dot = dir.x * toTarget.x + dir.y * toTarget.y + dir.z * toTarget.z;
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDir = i;
                }
            }
            //print("bd" + bestDir);

            BoatCommandType move = BoatCommandType.Nothing;
            // If not facing bestDir, turn towards it
            if (facing != bestDir)
            {
                int leftDist = (facing - bestDir + 6) % 6;
                int rightDist = (bestDir - facing + 6) % 6;
                if (leftDist <= rightDist)
                    move = BoatCommandType.RotateLeft;
                else
                    move = BoatCommandType.RotateRight;
            }
            else
            {
                // If facing target
                if (phantomSpeed == 4)
                {
                    move = BoatCommandType.Backward; 
                }
                else if (phantomSpeed == 0 ||phantomSpeed == 1 || phantomSpeed == 2)
                {
                    // Accelerate if safe
                    List<bool> willCrash = willHitIsland(pos, facing, phantomSpeed, new BoatCommand(BoatCommandType.Forward));
                    if (willCrash.Count > 0 && !willCrash[willCrash.Count - 1])
                        move = BoatCommandType.Forward;
                    else
                        move = BoatCommandType.Nothing;
                }
                else
                { 
                    move = BoatCommandType.Nothing;
                }
            }
            return (new BoatCommand(move), new FireCommand(FireCommandType.Nothing));
        }
    } 
}
