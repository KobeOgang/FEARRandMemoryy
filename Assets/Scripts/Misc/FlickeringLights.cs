using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickeringLights : MonoBehaviour
{
    [Header("Flicker Settings")]
    [Tooltip("The minimum amount of time the light will stay ON.")]
    [SerializeField] private float minOnTime = 0.05f;

    [Tooltip("The maximum amount of time the light will stay ON.")]
    [SerializeField] private float maxOnTime = 0.75f;

    [Tooltip("The minimum amount of time the light will stay OFF.")]
    [SerializeField] private float minOffTime = 0.05f;

    [Tooltip("The maximum amount of time the light will stay OFF.")]
    [SerializeField] private float maxOffTime = 0.5f;

    private Light lightSource;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
    }

    private void OnEnable()
    {
        StartCoroutine(Flicker());
    }

    private IEnumerator Flicker()
    {
        while (true)
        {
            lightSource.enabled = true;
            yield return new WaitForSeconds(Random.Range(minOnTime, maxOnTime));

            lightSource.enabled = false;
            yield return new WaitForSeconds(Random.Range(minOffTime, maxOffTime));
        }
    }
}
