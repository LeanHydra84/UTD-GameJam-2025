using System;
using UnityEngine;

public class ColorObjectsManager : MonoBehaviour
{
	public enum ColorState
	{
		None,
		Red,
		Blue,
	}
	
	
	public static ColorObjectsManager Instance;

	private void Awake()
	{
		blueObjects = GameObject.FindGameObjectsWithTag("Blue");
		redObjects = GameObject.FindGameObjectsWithTag("Red");
		
		Debug.Log($"{blueObjects.Length} blue objects found");
		Debug.Log($"{redObjects.Length} red objects found");
		
		Instance = this;
	}

	private void SetAll(GameObject[] list, bool en)
	{
		foreach (GameObject obj in list)
		{
			obj.SetActive(en);
		}
	}

	public void SetState(ColorState state)
	{
		if (state == ColorState.Blue)
		{
			SetAll(blueObjects, true);
			SetAll(redObjects, false);
		}
		else if (state == ColorState.Red)
		{
			SetAll(blueObjects, false);
			SetAll(redObjects, true);
		}
		else
		{
			SetAll(blueObjects, false);
			SetAll(redObjects, false);
		}
	}

	private GameObject[] blueObjects;
	private GameObject[] redObjects;

}
