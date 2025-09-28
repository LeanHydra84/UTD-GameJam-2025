using UnityEngine;

public class FilmReel : InteractableHoverStatic
{
	public override bool Interact()
	{
		InventoryManager.Instance.HasFilmReel = true;
		Destroy(gameObject);
		return true;
	}
}
