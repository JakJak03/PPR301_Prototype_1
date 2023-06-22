using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementScript : MonoBehaviour
{
    [SerializeField] private float attackDistance;
    private float currentDistance;
    [SerializeField] private float moveSpeed;
    private Rigidbody2D myRb;
    private CapsuleCollider2D capsuleCollider;

    private SpriteRenderer sprite;
    private Transform player;

    [HideInInspector]
    public bool moveIntoAttackPosition = false;
    [HideInInspector]
    public static Dictionary<int, EnemyMovementScript> instances = new Dictionary<int, EnemyMovementScript>();

    int enemyDirection = 0;
    public bool currentlyMovingAwayFromEnemy = false;
    float lastKnownDistanceToEnemy;

    bool edgeDetected = false;

    enum Direction
    {
        Left,
        Right,
        None
    }
    Direction direction;

    public enum State
    {
        Chasing,
        Avoiding,
        AvoidingEnemy,
        Attacking,
        None
    }
    public State currentState = State.None;

    void Awake()
    {
        instances.Add(gameObject.GetInstanceID(), this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Edge")
        {
          edgeDetected = true;
        }
        else if (other.gameObject.tag == "Enemy")
        {
            // Calculate enemy direction
            Vector2 enemyDirectionVec = transform.position - other.transform.position;
            lastKnownDistanceToEnemy = Vector2.Distance(transform.position, other.transform.position);
            if (enemyDirectionVec.x < 0) enemyDirection = -1;
            else enemyDirection = 1;
            // Change state
            currentState = State.AvoidingEnemy;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Edge")
        {
            edgeDetected = false;
        }
    }
    private void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        myRb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        player = FindAnyObjectByType<PlayerCombatScript>().transform;
    }

    private void Update()
    {
        currentDistance = Vector2.Distance(player.transform.position, transform.position);
        Vector2 playerDirection = player.position - transform.position;

      
        //Debug.Log(currentDistance);
        if (currentState == State.Chasing)
        {
            // Player is to the left
            if (playerDirection.x < 0)
            {
                direction = Direction.Left;
                if (currentDistance > attackDistance && moveIntoAttackPosition)
                {
                    //Debug.Log("Move Left");
                    sprite.color = Color.magenta;
                    MoveLeft(moveSpeed);
                }
            }
            else // Player is right
            {
                direction = Direction.Right;
                if (currentDistance > attackDistance && moveIntoAttackPosition)
                {
                    //Debug.Log("Move Right");
                    sprite.color = Color.magenta;
                    MoveRight(moveSpeed);
                }
            }
            // Attack Condition
            if (currentDistance <= attackDistance)
                currentState = State.Attacking;
        }
        else if (currentState == State.Avoiding) 
        {
            Direction oppositeDirection = direction == Direction.Left ? Direction.Left : Direction.Right;
            Avoid(moveSpeed, oppositeDirection);
            sprite.color = Color.yellow;

            // Attack Condition
            if (currentDistance <= attackDistance)
                currentState = State.Attacking;
        }
        else if (currentState == State.AvoidingEnemy && !currentlyMovingAwayFromEnemy)
        {
            StartCoroutine(AvoidOtherEnemies(1));

            // Attack Condition
            if (currentDistance <= attackDistance)
                currentState = State.Attacking;
        }
        else if (currentState == State.Attacking) 
        {
            sprite.color = Color.green;

            // Chase Condition
            if (currentDistance > attackDistance)
                currentState = State.Chasing;
        }
    }

    void MoveLeft(float moveSpeed)
    {
        myRb.velocity = new Vector3(moveSpeed * -1, myRb.velocity.y, 0);
    }

    void MoveRight(float moveSpeed)
    {
        myRb.velocity = new Vector3(moveSpeed * 1, myRb.velocity.y, 0);
    }

    void Avoid(float speed, Direction playerDirection)
    {
        // Debug.Log(this.gameObject.name + " is avoiding");
        // Edge detection
       
        if (!edgeDetected)
        {
            // If there's ground below, continue moving
            if (playerDirection == Direction.Left)
                MoveRight(speed);
            else if (playerDirection == Direction.Right)
                MoveLeft(speed);
        }
        else
        {
            //Debug.Log(this.gameObject.name + " has detected edge");
            // Edge detected stop moving
            myRb.velocity = new Vector3(0, myRb.velocity.y, 0);
        }
    }

    IEnumerator AvoidOtherEnemies(float duration)
    {
        currentlyMovingAwayFromEnemy = true;
        sprite.color = Color.blue;
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            // move away from the enemy in the opposite direction for specified amount of time
            if (enemyDirection < 0)
                MoveLeft(moveSpeed);
            else
                MoveRight(moveSpeed);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        currentlyMovingAwayFromEnemy = false;
    }


    void OnDestroy()
    {
        instances.Remove(gameObject.GetInstanceID());
    }
}
