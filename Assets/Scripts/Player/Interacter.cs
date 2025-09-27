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

            if(interact != currentHover)
            {
                currentHover?.OnHoverExit();
                currentHover = interact;
                interact?.OnHoverEnter();
            }

            if (pInput.Interact)
            {
                currentHover.OnHoverExit();
                currentHover = null;
                interact?.Interact();
            }

        }
        else
        {
            currentHover?.OnHoverExit();
            currentHover = null;
        }
    }
}
