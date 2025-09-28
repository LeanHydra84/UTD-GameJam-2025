using System.Collections;
using UnityEngine;

public class Door : InteractableHoverText
{

	[SerializeField] private Vector3 openPosition;
	[SerializeField] private float openTime = 0.5f;
	private Vector3 closePosition;
	private bool open = false;

	protected override void Start()
	{
		closePosition = transform.localPosition;
		base.Start();
	}

	IEnumerator LerpDoorPos(Vector3 pos)
	{

		float t = 0;
		Vector3 spos = transform.localPosition;
		while (t < openTime)
		{
			float percent = t / openTime;
			transform.localPosition = Vector3.Lerp(spos, pos, percent);
			
			yield return new WaitForEndOfFrame();
			t += Time.deltaTime;
		}
		
		transform.localPosition = pos;
	}
	
	public void OpenDoor()
	{
		StopAllCoroutines();
		StartCoroutine(LerpDoorPos(openPosition));
		open = true;
	}

	public void CloseDoor()
	{
		StopAllCoroutines();
		StartCoroutine(LerpDoorPos(closePosition));
		open = false;
	}

	public void ToggleDoor()
	{
		if (open)
		{
			CloseDoor();
		}
		else
		{
			OpenDoor();
		}
	}
	
}
