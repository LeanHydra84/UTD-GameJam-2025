using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
	public bool isInteractable { get; }
	public void Interact()
	{
		Debug.Log("Picked up item");
		
		Destroy(gameObject);
	}

	public void OnHoverEnter()
	{
		gameObject.layer = LayerMask.NameToLayer("MaskLayer");
	}

	public void OnHoverExit()
	{
		gameObject.layer = LayerMask.NameToLayer("Default");
	}
}
