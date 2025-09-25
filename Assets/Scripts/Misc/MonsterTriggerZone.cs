using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTriggerZone : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterObject;
    public Animator monsterAnimator;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Trigger Settings")]
    public float despawnDelay = 5f;

    private bool hasTriggered = false;
    private bool isMoving = false;

    private void Start()
    {
        if (monsterObject == null)
        {
            monsterObject = GameObject.FindWithTag("Enemy");
        }

        if (monsterAnimator == null && monsterObject != null)
        {
            monsterAnimator = monsterObject.GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (isMoving && monsterObject != null)
        {
            MoveMonsterForward();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            TriggerMonsterRun();
        }
    }

    private void TriggerMonsterRun()
    {
        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger("Run");
            isMoving = true;
            StartCoroutine(DespawnMonsterAfterDelay());
        }
    }

    private void MoveMonsterForward()
    {
        Vector3 forwardDirection = monsterObject.transform.forward;
        monsterObject.transform.position += forwardDirection * moveSpeed * Time.deltaTime;
    }

    private IEnumerator DespawnMonsterAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);

        isMoving = false;

        if (monsterObject != null)
        {
            Destroy(monsterObject);
        }
    }
}
