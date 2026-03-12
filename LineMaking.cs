using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LineMaking : MonoBehaviour
{
    public BoatController boat; 
    public Tilemap tilemap;     
    public GameObject arrowheadPrefab; 

    private LineRenderer[] lineRenderers = new LineRenderer[3];
    private List<LineRenderer> fireLineRenderers = new List<LineRenderer>(); // For firing lines
    private GameObject arrowheadInstance;

    private Color[] commandColors = new Color[] { Color.red, Color.yellow, Color.green };
    private Color fireColor = new Color(1f, 0.5f, 0f); // Orange for all firing lines

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
            // Firing lines will be created dynamically per movement step
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
            if (boat == null || tilemap == null || !boat.Selected || !TurnManager.Instance.ordersOpen || BoatSelection.Instance.currentTurn == BoatSelection.Turn.Neither)
            {
                foreach (var lr in lineRenderers) lr.positionCount = 0;
                foreach (var flr in fireLineRenderers) Destroy(flr.gameObject);
                fireLineRenderers.Clear();
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
            var fireCommands = new List<FireCommand>(boat.fireQueue);
            while (fireCommands.Count < 3) fireCommands.Insert(0, null); // Pad to 3 for firing

            // Remove old firing lines
            foreach (var flr in fireLineRenderers) Destroy(flr.gameObject);
            fireLineRenderers.Clear();

            for (int i = 0; i < 3; i++)
            {
                var cmd = commands[i];
                var fireCmd = fireCommands[i];
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

                        // Draw firing line for this movement step
                        if (fireCmd != null && fireCmd.fireCommandType != FireCommandType.Nothing)
                        {
                            // Use the current simulated facing for this step
                            int fireDir = -1;
                            switch (fireCmd.fireCommandType)
                            {
                                case FireCommandType.FireFrontLeft:
                                    fireDir = (simFacing + 5) % 6;
                                    break;
                                case FireCommandType.FireFrontRight:
                                    fireDir = (simFacing + 1) % 6;
                                    break;
                                case FireCommandType.FireBackRight:
                                    fireDir = (simFacing + 2) % 6;
                                    break;
                                case FireCommandType.FireBackLeft:
                                    fireDir = (simFacing + 4) % 6;
                                    break;
                            }
                            if (fireDir != -1)
                            {
                                Vector3Int fireCell = simCell;
                                List<Vector3> firePoints = new List<Vector3>();
                                firePoints.Add(tilemap.GetCellCenterWorld(fireCell));
                                for (int f = 0; f < Combat.Instance.firingRange; f++)
                                {
                                    var fireDirs = boat.GetDirs(fireCell.y);
                                    fireCell += fireDirs[fireDir];
                                    firePoints.Add(tilemap.GetCellCenterWorld(fireCell));
                                }
                                GameObject fireObj = new GameObject("FireLineStep" + i + "_" + s);
                                fireObj.transform.parent = this.transform;
                                var flr = fireObj.AddComponent<LineRenderer>();
                                flr.material = new Material(Shader.Find("Sprites/Default"));
                                flr.widthMultiplier = 0.07f;
                                flr.positionCount = firePoints.Count;
                                flr.SetPositions(firePoints.ToArray());
                                flr.startColor = flr.endColor = fireColor;
                                flr.sortingOrder = 11;
                                fireLineRenderers.Add(flr);
                            }
                        }
                    }

                    // Apply rotation only after all movement steps for this command
                    if (cmd.commandType == BoatCommandType.RotateLeft)
                        simFacing = (simFacing + 5) % 6;
                    else if (cmd.commandType == BoatCommandType.RotateRight)
                        simFacing = (simFacing + 1) % 6;
                }

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