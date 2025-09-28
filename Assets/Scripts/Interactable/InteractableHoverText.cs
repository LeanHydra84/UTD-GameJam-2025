using UnityEngine;

public class InteractableHoverText : MonoBehaviour, IInteractable
{
    
    [field: SerializeField] public bool IsInteractable { get; set; } = true;
    [SerializeField] private string hoverText;
    
    private TextOverrideInstance textOverride;

    protected virtual void Start()
    {
        textOverride = Resources.Load<TextOverrideInstance>("textOverride");
    }

    public virtual bool Interact() => false;

    public virtual void OnHoverEnter()
    {
        if (textOverride == null) return;
        textOverride.text = hoverText;
    }

    public virtual void OnHoverExit()
    {
        if (textOverride == null) return;
        textOverride.text = string.Empty;
    }
}
