using UnityEngine;

public class SafeButton : InteractableHoverStatic
{
    public Safe safe;
    public int label;

    private Material unpressed;
    public Material pressed;

    protected override void Start()
    {
        safe.Register(this);
        unpressed = GetComponent<Renderer>().material;
        base.Start();
    }

    public override bool Interact()
    {
        Pressed();
        safe.Push(label);
        return true;
    }

    public void Pressed()
    {
        GetComponent<Renderer>().sharedMaterial = pressed;
    }
    
    public void Reset()
    {
        GetComponent<Renderer>().sharedMaterial = unpressed;
    }
    
}
