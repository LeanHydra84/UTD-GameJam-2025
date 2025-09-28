using System.Collections;
using UnityEngine;

public class FBDoor : InteractableHoverStatic
{

	[SerializeField] private Transform hinges;
	
	private bool open = false;
	
	public override bool Interact()
	{
		ToggleDoor();
		return true;
	}

	IEnumerator RotateHingesTo(float x)
	{
		float t = 0;
		float start = hinges.localEulerAngles.z;
		while (t < 0.5f)
		{
			float p = t / 0.5f;
			hinges.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(start, x, p));
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
		}
		
		hinges.localRotation = Quaternion.Euler(0, 0, x);
	}
	
	private void ToggleDoor()
	{
		StopAllCoroutines();
		open = !open;
		StartCoroutine(RotateHingesTo(open ? 150 : 0));
	}
	
}
