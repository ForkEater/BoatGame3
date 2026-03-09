using UnityEngine;
using UnityEngine.InputSystem;

public class BoatSelection : MonoBehaviour
{
    private BoatActions actions;
    public static BoatController SelectedBoat;

    public void SelectBoat(BoatController boat)
    {
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
        actions = new BoatActions();
        actions.Movement.Tab.performed += ctx =>
        {
            if (SelectedBoat == null)
            {
                SelectBoat(TurnManager.Instance.boats[0]);
            }
            else
            {
                int index = TurnManager.Instance.boats.IndexOf(SelectedBoat);
                index = (index + 1) % TurnManager.Instance.boats.Count;
                while (TurnManager.Instance.boats[index].isEvil)
                {
                    index = (index + 1) % TurnManager.Instance.boats.Count;
                }
                SelectBoat(TurnManager.Instance.boats[index]);
            }
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
