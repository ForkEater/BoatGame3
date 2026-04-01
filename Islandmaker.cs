using System.Collections.Generic;
using UnityEngine;


public class Islandmaker : MonoBehaviour
{
    public static Islandmaker Instance;
    Vector3Int islandOne = new Vector3Int();
    Vector3Int islandTwo = new Vector3Int();
    Vector3Int islandThree = new Vector3Int();
    Vector3Int islandFour = new Vector3Int();
    Vector3Int islandFive = new Vector3Int();
    public List<Vector3Int> allIslands = new List<Vector3Int>();

    void Awake(){
        allIslands = new List<Vector3Int>{
            new Vector3Int(-12, 0, 0), 
            new Vector3Int(-12, 1, 0),
            new Vector3Int(-11, 2, 0),
            new Vector3Int(-11, 3, 0),
            new Vector3Int(-10, 4, 0),
            new Vector3Int(-10, 5, 0),
            new Vector3Int(-9, 6, 0),
            new Vector3Int(-9, 7, 0),
            new Vector3Int(-8, 8, 0),
            new Vector3Int(-8, 9, 0),
            new Vector3Int(-7, 10, 0),
            new Vector3Int(-7, 11, 0),
            new Vector3Int(-6, 12, 0),
            new Vector3Int(-6, 13, 0),
            new Vector3Int(-5, 14, 0),
            new Vector3Int(-5, 15, 0),
            new Vector3Int(-4, 16, 0),
            new Vector3Int(-3, 16, 0),
            new Vector3Int(-2, 16, 0),
            new Vector3Int(-1, 16, 0),
            new Vector3Int(0, 16, 0),
            new Vector3Int(1, 16, 0),
            new Vector3Int(2, 16, 0),
            new Vector3Int(3, 16, 0),
            new Vector3Int(4, 16, 0),
            new Vector3Int(4, 15, 0),
            new Vector3Int(5, 14, 0),
            new Vector3Int(5, 13, 0),
            new Vector3Int(6, 12, 0),
            new Vector3Int(6, 11, 0),
            new Vector3Int(7, 10, 0),
            new Vector3Int(7, 9, 0),
            new Vector3Int(8, 8, 0),
            new Vector3Int(8, 7, 0),
            new Vector3Int(9, 6, 0),
            new Vector3Int(9, 5, 0),
            new Vector3Int(10, 4, 0),
            new Vector3Int(10, 3, 0),
            new Vector3Int(11, 2, 0),
            new Vector3Int(11, 1, 0),
            new Vector3Int(12, 0, 0),
            new Vector3Int(-12, -1, 0),
            new Vector3Int(-11, -2, 0),
            new Vector3Int(-11, -3, 0),
            new Vector3Int(-10, -4, 0),
            new Vector3Int(-10, -5, 0),
            new Vector3Int(-9, -6, 0),
            new Vector3Int(-9, -7, 0),
            new Vector3Int(-8, -8, 0),
            new Vector3Int(-8, -9, 0),
            new Vector3Int(-7, -10, 0),
            new Vector3Int(-7, -11, 0),
            new Vector3Int(-6, -12, 0),
            new Vector3Int(-6, -13, 0),
            new Vector3Int(-5, -14, 0),
            new Vector3Int(-5, -15, 0),
            new Vector3Int(-4, -16, 0),
            new Vector3Int(-3, -16, 0),
            new Vector3Int(-2, -16, 0),
            new Vector3Int(-1, -16, 0),
            new Vector3Int(0, -16, 0),
            new Vector3Int(1, -16, 0),
            new Vector3Int(2, -16, 0),
            new Vector3Int(3, -16, 0),
            new Vector3Int(4, -16, 0),
            new Vector3Int(4, -15, 0),
            new Vector3Int(5, -14, 0),
            new Vector3Int(5, -13, 0),
            new Vector3Int(6, -12, 0),
            new Vector3Int(6, -11, 0),
            new Vector3Int(7, -10, 0),
            new Vector3Int(7, -9, 0),
            new Vector3Int(8, -8, 0),
            new Vector3Int(8, -7, 0),
            new Vector3Int(9, -6, 0),
            new Vector3Int(9, -5, 0),
            new Vector3Int(10, -4, 0),
            new Vector3Int(10, -3, 0),
            new Vector3Int(11, -2, 0),
            new Vector3Int(11, -1, 0)
        };
        Instance=this;
        System.Random rnd = new System.Random();
        islandOne = new Vector3Int(rnd.Next(-4, 4), rnd.Next(-11, -5), 0);
        islandTwo = new Vector3Int(rnd.Next(-4, 4), rnd.Next(-4, 4), 0);
        islandThree = new Vector3Int(rnd.Next(-4, 4), rnd.Next(5, 11), 0);
        islandFour = new Vector3Int(rnd.Next(-4, 4), rnd.Next(-6, 0), 0);
        islandFive = new Vector3Int(rnd.Next(-4, 4), rnd.Next(0, 6), 0);
        allIslands.Add(islandOne);
        allIslands.Add(new Vector3Int(islandOne[0], islandOne[1]+1, 0));
        allIslands.Add(new Vector3Int(islandOne[0], islandOne[1]-1, 0));
        allIslands.Add(new Vector3Int(islandOne[0]-1, islandOne[1], 0));
        allIslands.Add(islandTwo);
        allIslands.Add(new Vector3Int(islandTwo[0], islandTwo[1]+1, 0));
        allIslands.Add(new Vector3Int(islandTwo[0], islandTwo[1]-1, 0));
        allIslands.Add(new Vector3Int(islandTwo[0]-1, islandTwo[1], 0));
        allIslands.Add(islandThree);
        allIslands.Add(new Vector3Int(islandThree[0], islandThree[1]+1, 0));
        allIslands.Add(new Vector3Int(islandThree[0], islandThree[1]-1, 0));
        allIslands.Add(new Vector3Int(islandThree[0]-1, islandThree[1], 0));
        allIslands.Add(islandFour);
        allIslands.Add(new Vector3Int(islandFour[0], islandFour[1]+1, 0));
        allIslands.Add(new Vector3Int(islandFour[0], islandFour[1]-1, 0));
        allIslands.Add(new Vector3Int(islandFour[0]-1, islandFour[1], 0));
        allIslands.Add(islandFive);
        allIslands.Add(new Vector3Int(islandFive[0], islandFive[1]+1, 0));
        allIslands.Add(new Vector3Int(islandFive[0], islandFive[1]-1, 0));
        allIslands.Add(new Vector3Int(islandFive[0]-1, islandFive[1], 0));
    
        
    }



}
