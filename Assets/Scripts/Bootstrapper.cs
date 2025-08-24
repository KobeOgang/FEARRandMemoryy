using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        // Load the Main Menu scene right after the managers have initialized.
        SceneManager.LoadScene("MainMenu");
    }
}
