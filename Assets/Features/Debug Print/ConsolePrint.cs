using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsolePrint : MonoBehaviour
{
    public void PrintLog(string message)
    {
        Debug.Log(message);
    }

    public void PrintWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public void PrintError(string message)
    {
        Debug.LogError(message);
    }
}
