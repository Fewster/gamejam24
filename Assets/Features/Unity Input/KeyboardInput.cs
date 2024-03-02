using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyboardInput : MonoBehaviour
{
    public KeyCode Key;

    public UnityEvent OnKeyDown;

    void Update()
    {
        if (Input.GetKeyDown(Key))
        {
            OnKeyDown?.Invoke();
        }
    }
}
