using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
/*
    [SerializeField]
    Transform player;
    PlayerMovement playerActions;

    List<EnemyMovementScript> enemyMoveScripts;

    EnemyMovementScript currentEnemyMove;

    int currentEnemyCount, previousEnemyCount;

    // detection 
    BoxCollider2D detectionCollider;
    public float distanceToUpdateDetector = 1f;
    Vector2 previousPlayerPos;

    private void Start()
    {
        playerActions = player.GetComponent<PlayerMovement>();
        enemyMoveScripts = new List<EnemyMovementScript>();
        detectionCollider = GetComponent<BoxCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.transform.parent != null && other.gameObject.transform.parent.name == "Enemies")
        {
            int instanceID = other.gameObject.GetInstanceID();
            if (EnemyMovementScript.instances.ContainsKey(instanceID))
            {
                enemyMoveScripts.Add(EnemyMovementScript.instances[instanceID]);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.transform.parent != null && other.gameObject.transform.parent.name == "Enemies")
        {
            int instanceID = other.gameObject.GetInstanceID();
            if (EnemyMovementScript.instances.ContainsKey(instanceID))
            {
                enemyMoveScripts.Remove(EnemyMovementScript.instances[instanceID]);
            }
        }
    }

    void Update()
    {
        // Check if the player has moved further than distanceToUpdateDetector
        if (Vector2.Distance(player.position, previousPlayerPos) > distanceToUpdateDetector && !playerActions.IsJumping)
        {
            // If the player has moved far enough, move the detectionCollider closer to the player and record the new position
            detectionCollider.offset = player.localPosition;
            previousPlayerPos = player.position;
        }

        if (enemyMoveScripts.Count > 0)
        {
             // Only runs when the enemiesInRange count changes.
            currentEnemyCount = enemyMoveScripts.Count;
            if (currentEnemyCount != previousEnemyCount)
            {
                // Pick a random enemy in range
                int randomIndex = Random.Range(0, enemyMoveScripts.Count);
                currentEnemyMove = enemyMoveScripts[randomIndex];
                // Iterate through the list of enemies 
                int enemyIndex = 0;
                foreach (EnemyMovementScript enemy in enemyMoveScripts)
                {
                    //Debug.Log("Enemies in range: " + enemy.name);

                    // The random enemy will moveIntoAttackPosition
                    if (randomIndex == enemyIndex)
                    {
                        // Tell it to move into attack position
                        currentEnemyMove.moveIntoAttackPosition = true;
                        currentEnemyMove.currentState = EnemyMovementScript.State.Chasing;
                    }
                    else
                    {
                        // The other enemies will avoid
                        currentEnemyMove.moveIntoAttackPosition = false;
                        //currentEnemyMove.currentState = EnemyMovementScript.State.Avoiding;
                    }

                    enemyIndex++;
                }
            }
        }
        previousEnemyCount = currentEnemyCount;
    }
    */
}
