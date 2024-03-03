using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityInputEvent : MonoBehaviour
{
    public UnityEvent OnKeyDown;
    // I refuse to do OnKeyHeld;
    public UnityEvent OnKeyUp;

    public KeyCode Key;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(Key))
        {
            OnKeyDown.Invoke();
        }

        if (Input.GetKeyUp(Key))
        {
            OnKeyUp.Invoke();
        }
    }
}
