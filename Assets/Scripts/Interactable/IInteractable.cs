using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool IsInteractable { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Whether or not value OnHoverExit should be called after interact</returns>
    bool Interact();
    void OnHoverEnter();
    void OnHoverExit();
}
