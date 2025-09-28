using UnityEngine;

public class RedFuse : InteractableHoverStatic
{
	public override bool Interact()
	{
		InventoryManager.Instance.HasFuzeRed = true;
		Destroy(gameObject);
		return true;
	}
}
