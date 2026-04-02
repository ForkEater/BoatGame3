using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayOrders : MonoBehaviour
{
	public BoatController boat;

	public GameObject forwardArrow;
	public GameObject backwardArrow;
	public GameObject rotateLeftArrow;
	public GameObject rotateRightArrow;
    public GameObject nothingArrow;
	public GameObject frontRightShot;
	public GameObject frontLeftShot;
	public GameObject backRightShot;
	public GameObject backLeftShot;
	public GameObject nothingShot;

	public Sprite goodDamagedSprite;
	public Sprite evilDamagedSprite;


	public Transform arrowContainer;
	public Transform shotContainer;

	public float arrowSpacing = 40f;

	private List<GameObject> arrows = new List<GameObject>();
	private List<GameObject> shots = new List<GameObject>();

    private Image tabImage;
	private bool destroyed = false;

    private void Start()
    {
        tabImage = GetComponent<Image>();
    }

	public void Update()
	{
		DestroyDisplay();
		if (!destroyed && ((boat.isEvil && BoatSelection.Instance.currentTurn == BoatSelection.Turn.Evil) || (!boat.isEvil && BoatSelection.Instance.currentTurn == BoatSelection.Turn.Good)))
		{
			UpdateArrows(); 
			UpdateShots();
		}
	}

	private void DestroyDisplay()
	{
		foreach (var obj in arrows)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}
		arrows.Clear();
		foreach (var obj in shots)
		{
			if (obj != null)
			{
				Destroy(obj);
			}
		}
		shots.Clear();
	}
    private void UpdateArrows()
    {
		if (boat == null || boat.commandQueue == null)
			return;

		for (int i = 0; i < boat.commandQueue.Count; i++)
		{
			var command = boat.commandQueue[i];
			GameObject arrowPrefab = null;
			switch (command.commandType)
			{
				case BoatCommandType.Forward:
					arrowPrefab = forwardArrow;
					break;
				case BoatCommandType.Backward:
					arrowPrefab = backwardArrow;
					break;
				case BoatCommandType.RotateLeft:
					arrowPrefab = rotateLeftArrow;
					break;
				case BoatCommandType.RotateRight:
					arrowPrefab = rotateRightArrow;
					break;
                case BoatCommandType.Nothing:
                    arrowPrefab = nothingArrow;
                    break;
			}
			if (arrowPrefab != null)
			{
				GameObject arrow = Instantiate(arrowPrefab, arrowContainer);
				arrow.transform.localPosition = new Vector3(0, -(i + 1) * arrowSpacing, 0);
				arrows.Add(arrow);
			}
		}
    }

	private void UpdateShots()
    {
		if (boat == null || boat.commandQueue == null)
			return;

		for (int i = 0; i < boat.fireQueue.Count; i++)
		{
			var command = boat.fireQueue[i];
			GameObject fireDirection = null;
			switch (command.fireCommandType)
			{
				case FireCommandType.FireFrontRight:
					fireDirection = frontRightShot;
					break;
				case FireCommandType.FireFrontLeft:
					fireDirection = frontLeftShot;
					break;
				case FireCommandType.FireBackRight:
					fireDirection = backRightShot;
					break;
				case FireCommandType.FireBackLeft:
					fireDirection = backLeftShot;
					break;
				case FireCommandType.Nothing:
					fireDirection = nothingShot;
					break;
			}
			if (fireDirection != null)
			{
				GameObject aim = Instantiate(fireDirection, shotContainer);
				aim.transform.localPosition = new Vector3(0, -(i + 1) * arrowSpacing, 0);
				shots.Add(aim);
			}
		}
    }

    public void setDamaged()
    {
		// print("boat " + boat.name + " has been hit :(");
        boat.boatImage.color = new Color32(255,0,0,230);
		if (boat.isEvil)
		{
			gameObject.GetComponent<Image>().sprite = evilDamagedSprite;
			boat.gameObject.GetComponent<SpriteRenderer>().sprite = evilDamagedSprite;
		} else
		{
			gameObject.GetComponent<Image>().sprite = goodDamagedSprite;
			boat.gameObject.GetComponent<SpriteRenderer>().sprite = goodDamagedSprite;
		}
        tabImage.color = new Color32(255,0,0,230);
    }

    public void setDestroyed()
    {
        boat.speed = 0;
		boat.boatImage.color = new Color32(50,70,100,230);
        tabImage.color = new Color32(50,70,100,230);
		destroyed = true;
    }
}
