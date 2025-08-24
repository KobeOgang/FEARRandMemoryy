using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public GameObject flashlight;

    public bool on;
    public bool off;

    private Animator animator;

    private void Start()
    {
        off = true;
        flashlight.SetActive(false);
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (off && Input.GetKeyDown(KeyCode.F))
        {
            flashlight.SetActive(true);
            off = false;
            on = true;

            // ---  ANIMATION VARIANT ---
            // Tell the animator to switch to the "holding flashlight" animation layer/set.
            // animator.SetBool("IsHoldingFlashlight", true);

            //animator.SetLayerWeight(1, 1f);
        }
        else if (on && Input.GetKeyDown(KeyCode.F))
        {
            flashlight.SetActive(false);
            on = false;
            off = true;

            // --- ADD THIS ANIMATION COMMENT ---
            // Tell the animator to switch back to the default animations.
            // animator.SetBool("IsHoldingFlashlight", false);

            //animator.SetLayerWeight(1, 0f);
        }
    }
}
