using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LineMaking : MonoBehaviour
{
    public BoatController boat; 
    public Tilemap tilemap;     
    public GameObject arrowheadPrefab; 

    private LineRenderer[] lineRenderers = new LineRenderer[3];
    private GameObject arrowheadInstance;

    private Color[] commandColors = new Color[] { Color.red, Color.yellow, Color.green };

    void Start()
    {
        // Create 3 LineRenderers for the 3 commands
        for (int i = 0; i < 3; i++)
        {
            GameObject lrObj = new GameObject("CommandLine" + i);
            lrObj.transform.parent = this.transform;
            lineRenderers[i] = lrObj.AddComponent<LineRenderer>();
            lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderers[i].widthMultiplier = 0.15f;
            lineRenderers[i].positionCount = 0;
            lineRenderers[i].startColor = lineRenderers[i].endColor = commandColors[i];
            lineRenderers[i].sortingOrder = 10;
        }
        // Create arrowhead instance
        arrowheadInstance = Instantiate(arrowheadPrefab, Vector3.zero, Quaternion.identity, this.transform);
        // Ensure arrowhead is above the line
        var sr = arrowheadInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 20; // Higher than lineRenderers' sortingOrder (10)
        }
        arrowheadInstance.SetActive(false);
    }

    void Update()
    {
        if (boat == null || tilemap == null || !boat.Selected || !TurnManager.Instance.ordersOpen)
        {
            foreach (var lr in lineRenderers) lr.positionCount = 0;
            if (arrowheadInstance) arrowheadInstance.SetActive(false);
            return;
        }

        // Simulate command queue
        Vector3Int simCell = boat.currentCell;
        int simFacing = boat.GetFacing();
        int simSpeed = boat.speed;

        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(tilemap.GetCellCenterWorld(simCell));

        // Copy queue for simulation
        var commands = new List<BoatCommand>(boat.commandQueue);
        while (commands.Count < 3) commands.Insert(0, null); // Pad to 3 for color mapping

        for (int i = 0; i < 3; i++)
        {
            var cmd = commands[i];
            if (cmd == null)
            {
                lineRenderers[i].positionCount = 0;
                continue;
            }

            // Simulate command effect
            if (cmd.commandType == BoatCommandType.Forward)
            {
                simSpeed = Mathf.Min(simSpeed + 1, boat.maxSpeed);
            }
            else if (cmd.commandType == BoatCommandType.Backward)
            {
                simSpeed = Mathf.Max(simSpeed - 1, boat.minSpeed);
            }

            // Simulate movement for current speed
            if (simSpeed != 0)
            {
                int moveSteps = Mathf.Abs(simSpeed);
                int moveDir = simSpeed > 0 ? simFacing : (simFacing + 3) % 6;
                for (int s = 0; s < moveSteps; s++)
                {
                    var dirs = boat.GetDirs(simCell.y);
                    simCell += dirs[moveDir];
                    pathPoints.Add(tilemap.GetCellCenterWorld(simCell));
                }
            }

            // Simulate rotation
            if (cmd.commandType == BoatCommandType.RotateLeft)
                simFacing = (simFacing + 5) % 6;
            else if (cmd.commandType == BoatCommandType.RotateRight)
                simFacing = (simFacing + 1) % 6;

            // Draw segment
            
            lineRenderers[i].positionCount = pathPoints.Count;
            lineRenderers[i].SetPositions(pathPoints.ToArray());
            lineRenderers[i].startColor = lineRenderers[i].endColor = commandColors[i];
            
        }

        // Draw arrowhead
        if (arrowheadInstance)
        {
            arrowheadInstance.SetActive(true);
            arrowheadInstance.transform.position = pathPoints[pathPoints.Count - 1];
            arrowheadInstance.transform.rotation = Quaternion.Euler(0, 0, -simFacing * 60f + 90);
            if (simSpeed < 0) // If moving backward, flip the arrow
            {
                arrowheadInstance.transform.rotation *= Quaternion.Euler(0, 0, 180);
            }
        }
        
    }
}