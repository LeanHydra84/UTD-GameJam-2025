using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interacter : MonoBehaviour
{

    [SerializeField] PlayerInput pInput;
    [SerializeField] float reachDistance = 1;

    IInteractable currentHover = null;

    void Update()
    {
        if(Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, reachDistance))
        {
            IInteractable interact = hit.transform.GetComponent<IInteractable>();
            if (interact?.IsInteractable == false)
            {
                interact = null;
            }
            
            if(interact != currentHover)
            {
                currentHover?.OnHoverExit();
                currentHover = interact;
                interact?.OnHoverEnter();
            }

            if (!pInput.Interact) return;
            
            bool? exit = interact?.Interact();
            if (exit != true) return;
            currentHover.OnHoverExit();
            currentHover = null;
        }
        else
        {
            currentHover?.OnHoverExit();
            currentHover = null;
        }
    }
}
