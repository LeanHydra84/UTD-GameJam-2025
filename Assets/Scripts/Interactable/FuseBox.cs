using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class FuseBox : InteractableHoverStatic
{

	[SerializeField] private GameObject fuseRed;
	[SerializeField] private GameObject fuseBlue;
	[SerializeField] private UnityEvent onAllFuses;
	
	public override bool Interact()
	{

		TryAddFuse();
		
		return false;
	}

	void TryAddFuse()
	{
		InventoryManager inv = InventoryManager.Instance;
		if (!fuseRed.activeSelf && inv.HasFuzeRed)
		{
			fuseRed.SetActive(true);
			CheckFuses();
			return;
		}

		if (!fuseBlue.activeSelf && inv.HasFuzeBlue)
		{
			fuseBlue.SetActive(true);
			CheckFuses();
			return;
		}
	}

	void CheckFuses()
	{
		if (fuseRed.activeSelf && fuseBlue.activeSelf)
		{
			onAllFuses.Invoke();
		}
	}
	
}
