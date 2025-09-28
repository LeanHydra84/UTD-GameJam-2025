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
		
		blueAndClear = GameObject.FindGameObjectsWithTag("BlueClear");
		redAndClear = GameObject.FindGameObjectsWithTag("RedClear");
		
		redEnemy = new RBEnemy(GameObject.FindGameObjectWithTag("RedEnemy"));
		blueEnemy = new RBEnemy(GameObject.FindGameObjectWithTag("BlueEnemy"));
		
		Debug.Log($"{blueObjects.Length} blue objects found");
		Debug.Log($"{redObjects.Length} red objects found");

		Resources.Load<TextOverrideInstance>("textOverride").text = string.Empty;
		
		Instance = this;
	}

	private void SetAll(GameObject[] list, bool en)
	{
		foreach (GameObject obj in list)
		{
			if (obj == null) continue;
			obj.SetActive(en);
		}
	}

	public void SetState(ColorState state)
	{
		if (state == ColorState.Blue)
		{
			SetAll(blueObjects, true);
			SetAll(redObjects, false);

			SetAll(blueAndClear, true);
			SetAll(redAndClear, false);

			
			redEnemy?.SetEnabled(false, discardMaterial);
			blueEnemy?.SetEnabled(true, discardMaterial);
		}
		else if (state == ColorState.Red)
		{
			SetAll(blueObjects, false);
			SetAll(redObjects, true);
			
			SetAll(blueAndClear, false);
			SetAll(redAndClear, true);
			
			redEnemy?.SetEnabled(true, discardMaterial);
			blueEnemy?.SetEnabled(false, discardMaterial);
		}
		else
		{
			SetAll(blueObjects, false);
			SetAll(redObjects, false);
			
			SetAll(blueAndClear, true);
			SetAll(redAndClear, true);
			
			redEnemy?.SetEnabled(false, discardMaterial);
			blueEnemy?.SetEnabled(false, discardMaterial);
		}
	}
	
	[SerializeField] private Material discardMaterial;

	private class RBEnemy
	{
		public GlobalFollowerAI ai;
		public Renderer renderer;
		private Material baseMat;

		public RBEnemy(GameObject go)
		{
			if (go == null) return;
			ai = go.GetComponent<GlobalFollowerAI>();
			renderer = go.GetComponent<Renderer>();
			baseMat = renderer.material;
		}

		public void SetEnabled(bool enabled, Material discardMaterial)
		{
			ai.enabled = enabled;
			renderer.material = enabled ? discardMaterial : baseMat;
			ai.gameObject.layer = enabled ? LayerMask.NameToLayer("MaskLayer") : LayerMask.NameToLayer("Default");
		}
	}

	private RBEnemy redEnemy;
	private RBEnemy blueEnemy;

	private GameObject[] blueObjects;
	private GameObject[] redObjects;

	private GameObject[] redAndClear;
	private GameObject[] blueAndClear;

}
