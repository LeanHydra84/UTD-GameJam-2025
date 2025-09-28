using UnityEngine;

public class PickupItem : InteractableHoverStatic
{
	public override bool Interact()
	{
		Debug.Log("Picked up item");
		Destroy(gameObject);
		return true;
	}
}
