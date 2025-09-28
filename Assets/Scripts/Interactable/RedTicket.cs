using UnityEngine;

public class RedTicket : InteractableHoverStatic
{

	[SerializeField] private GameObject showCanvas;

	public override void OnHoverEnter()
	{
		showCanvas.SetActive(true);
		base.OnHoverEnter();
	}
	public override void OnHoverExit()
	{
		showCanvas.SetActive(false);
		base.OnHoverExit();
	}

}
