using UnityEngine;

public class InteractableHoverStatic : MonoBehaviour, IInteractable
{
	[field: SerializeField] public bool IsInteractable { get; set; } = true;

	private int startLayer;
	
	protected virtual void Start()
	{
		startLayer = gameObject.layer;
	}
	
	public virtual bool Interact() => false;

	public virtual void OnHoverEnter()
	{
		gameObject.layer = LayerMask.NameToLayer("MaskLayer");
	}

	public virtual void OnHoverExit()
	{
		gameObject.layer = startLayer;
	}
}
