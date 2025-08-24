using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit: " + other.name);
    }
}

