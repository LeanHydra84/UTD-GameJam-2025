using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ApplyBlueFilm : InteractableHoverStatic
{

	[SerializeField] private GameObject showReel;
	[SerializeField] private GameObject showStrip;

	public UnityEvent onApplied;

	IEnumerator WinGame()
	{
		yield return new WaitForSeconds(3);
		SceneManager.LoadScene("MainMenu");
	}
	
	public override bool Interact()
	{
		if (InventoryManager.Instance.HasFilmReel)
		{
			IsInteractable = false;
			showReel.SetActive(true);
			Destroy(gameObject);
			onApplied.Invoke();
			return true;
		}

		return false;
	}
}
