using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSelection : MonoBehaviour
{
    public static BoatSelection Instance;
    private BoatActions actions;
    public static BoatController SelectedBoat;
    public enum Turn { Good, Evil, Neither };

    public Turn currentTurn = Turn.Good;
    public Turn previousTurn;

    public void SelectBoat(BoatController boat)
    {
        if (boat == null)
        {
            SelectedBoat = null;
            return;
        }
        for (int i = 0; i < TurnManager.Instance.boats.Count; i++)
        {
            TurnManager.Instance.boats[i].SetSelected(false);
        }
        SelectedBoat = boat;
        boat.SetSelected(true);
        //print("Selected boat: " + boat.name);
    }

    Vector3 mousePosition;
    RaycastHit2D raycastHit2D;
    Transform clickObject;

    // Update is called once per frame
    void Update()
    {
        mousePosition = Mouse.current.position.ReadValue();

        Ray mouseRay = Camera.main.ScreenPointToRay(mousePosition);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            raycastHit2D = Physics2D.Raycast(mouseRay.origin, mouseRay.direction);
            clickObject = raycastHit2D ? raycastHit2D.collider.transform : null;

            if (clickObject)
            {
                BoatController boat = clickObject.GetComponentInParent<BoatController>();
                if (boat)
                {
                    SelectBoat(boat);
                }
            }
        }
    }

    private void Awake()
    {
        Instance = this;


        actions = new BoatActions();
        actions.Movement.Tab.performed += ctx =>
        {
            selectNextBoat();
        };
        
        actions.Movement.Move.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            Vector2 input = ctx.ReadValue<Vector2>();
            if (input.y > 0) SelectedBoat.AddCommand(new BoatCommand(BoatCommandType.Forward));
            if (input.y < 0) SelectedBoat.AddCommand(new BoatCommand(BoatCommandType.Backward));
        };
        actions.Movement.Rotation.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            Vector2 input = ctx.ReadValue<Vector2>();
            if (input.x > 0) SelectedBoat.AddCommand(new BoatCommand(BoatCommandType.RotateRight));
            if (input.x < 0) SelectedBoat.AddCommand(new BoatCommand(BoatCommandType.RotateLeft));
        };
        actions.Movement.NoAction.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddCommand(new BoatCommand(BoatCommandType.Nothing));
        };


        actions.Movement.ShootFR.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddFireCommand(new FireCommand(FireCommandType.FireFrontRight));
        };
        actions.Movement.ShootFL.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddFireCommand(new FireCommand(FireCommandType.FireFrontLeft));
        };
        actions.Movement.ShootBR.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddFireCommand(new FireCommand(FireCommandType.FireBackRight));
        };
        actions.Movement.ShootBL.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddFireCommand(new FireCommand(FireCommandType.FireBackLeft));
        };
        actions.Movement.NoShoot.performed += ctx =>
        {
            if (SelectedBoat == null) return;
            SelectedBoat.AddFireCommand(new FireCommand(FireCommandType.Nothing));
        };

        
        actions.Movement.ChangeTurn.performed += ctx =>
        {
            if (TurnManager.Instance.gameMode == gameType.Singleplayer)
            {
                return;
            }
            changeTurn();
        };
    }

    public void selectNextBoat()
    {
        if (SelectedBoat == null)
        {
            if (currentTurn == Turn.Evil)
            {
                SelectBoat(TurnManager.Instance.evilBoats[0]);
            } else
            {
                SelectBoat(TurnManager.Instance.goodBoats[0]);
            }
            
        }
        else
        {
            if (currentTurn == Turn.Evil)
            {
                if (!SelectedBoat.isEvil)
                {
                    SelectBoat(TurnManager.Instance.evilBoats[0]);
                } else
                {
                    int index = TurnManager.Instance.evilBoats.IndexOf(SelectedBoat);
                    index = (index + 1) % TurnManager.Instance.evilBoats.Count;
                    SelectBoat(TurnManager.Instance.evilBoats[index]);
                }
            } else
            {
                if (SelectedBoat.isEvil)
                {
                    SelectBoat(TurnManager.Instance.goodBoats[0]);
                } else
                {
                    int index = TurnManager.Instance.goodBoats.IndexOf(SelectedBoat);
                    index = (index + 1) % TurnManager.Instance.goodBoats.Count;
                    SelectBoat(TurnManager.Instance.goodBoats[index]);
                }
            }
        }
    }

    public void changeTurn()
    {
        if (TurnManager.Instance.gameMode == gameType.Singleplayer)
        {
            currentTurn = Turn.Good; 
            return;
        }
        if (currentTurn != Turn.Neither)
        {
            previousTurn = currentTurn;
            currentTurn = Turn.Neither;
            SelectBoat(null);
        } else if (currentTurn == Turn.Neither && previousTurn == Turn.Good)
        {
            currentTurn = Turn.Evil;
            SelectBoat(TurnManager.Instance.evilBoats[0]);
        } else if (currentTurn == Turn.Neither && previousTurn == Turn.Evil)
        {
            currentTurn = Turn.Good;
            SelectBoat(TurnManager.Instance.goodBoats[0]);
        }
    }

    private void OnEnable()
    {
        actions.Enable();
    }

    private void OnDisable()
    {
        actions.Disable();
    }
}
