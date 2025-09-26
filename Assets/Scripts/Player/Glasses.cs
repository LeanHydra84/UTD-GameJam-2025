using UnityEngine;
using UnityEngine.Rendering;

public class Glasses : MonoBehaviour
{
	
	[SerializeField] private PlayerInput playerInput;
	
	public bool IsBlue { get; private set; }
	public bool GlassesOn { get; private set; }

	public Volume redVolume;
	public Volume blueVolume;

	private void SwapVolumes()
	{
		if (!GlassesOn)
			return;
		IsBlue = !IsBlue;
		redVolume.enabled = IsBlue;
		blueVolume.enabled = !IsBlue;

		if (ColorObjectsManager.Instance == null)
			return;

		ColorObjectsManager.Instance.SetState(IsBlue
			? ColorObjectsManager.ColorState.Blue
			: ColorObjectsManager.ColorState.Red);
	}

	private void ToggleGlasses()
	{
		GlassesOn = !GlassesOn;
		if (GlassesOn)
		{
			redVolume.enabled = IsBlue;
			blueVolume.enabled = !IsBlue;
			if (ColorObjectsManager.Instance != null)
				ColorObjectsManager.Instance.SetState(IsBlue ? ColorObjectsManager.ColorState.Blue : ColorObjectsManager.ColorState.Red);
		}
		else
		{
			redVolume.enabled = false;
			blueVolume.enabled = false;
			if (ColorObjectsManager.Instance != null)
				ColorObjectsManager.Instance.SetState(ColorObjectsManager.ColorState.None);
		}
	}

	void Start()
	{
		IsBlue = true;
		GlassesOn = true;
		SwapVolumes();
		ToggleGlasses();
	}

	void Update()
	{
		if (playerInput.SwapGlasses)
		{
			SwapVolumes();
		}

		if (playerInput.ToggleGlasses)
		{
			ToggleGlasses();
		}
	}
	
}
