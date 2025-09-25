using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool isInteractable { get; }
    void Interact();
    void OnHoverEnter();
    void OnHoverExit();
}
