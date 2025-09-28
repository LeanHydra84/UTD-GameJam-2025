using UnityEngine;

public class BlueFuse : InteractableHoverStatic
{
    public override bool Interact()
    {
        InventoryManager.Instance.HasFuzeBlue = true;
        Destroy(gameObject);
        return true;
    }
}
