using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public Vector2 Mouse { get; private set; }

    public bool Interact { get; private set; }

    public bool Sprint { get; private set; }
    
    public bool Jump { get; private set; }

    void Update()
    {
        Movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Mouse = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Interact = Input.GetKeyDown(KeyCode.E);

        Jump = Input.GetKeyDown(KeyCode.Space);
        Sprint = Input.GetKey(KeyCode.LeftShift);
        
    }
}
