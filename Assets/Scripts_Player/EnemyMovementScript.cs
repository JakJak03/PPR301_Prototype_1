using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementScript : MonoBehaviour
{
    [SerializeField] private float attackDistance;
    public float currentDistance;
    [SerializeField] private float moveSpeed;
    private Rigidbody2D myRb;
    private CapsuleCollider2D capsuleCollider;

    private SpriteRenderer sprite;
    private Transform player;

    [HideInInspector]
    public bool moveIntoAttackPosition = false;
    public static Dictionary<int, EnemyMovementScript> instances = new Dictionary<int, EnemyMovementScript>();

    int enemyDirection = 0;
    public bool currentlyMovingAwayFromEnemy = false;
    float lastKnownDistanceToEnemy;
    bool edgeDetected = false;

    public enum Direction
    {
        Left,
        Right,
        None
    }
    Direction direction;

    public Direction enemyCurrentDirection;

    public enum State
    {
        Chasing,
        Avoiding,
        AvoidingEnemy,
        Attacking,
        Stunned,
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
            if (currentState != State.Attacking)
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
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Edge")
        currentState = State.Chasing;
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
        currentState = State.None;
    }

    private void Update()
    {
            //print("Current Distance: " + currentDistance + " Attack Distance: " + attackDistance);

        currentDistance = Vector2.Distance(player.transform.position, transform.position);
        Vector2 playerDirection = player.position - transform.position;
        //Debug.Log(currentDistance);

        if(currentState == State.None)
        {
            sprite.color = Color.black;
            WhoIsAttacking();
        }
        // CHASING
        if (currentState == State.Chasing)
        {
            if (!WhoIsAttacking())
                currentState = State.None;

            // Player is to the left
            if (playerDirection.x < 0)
            {
                direction = Direction.Left;
                if (currentDistance > attackDistance)
                {
                    //Debug.Log("Move Left");
                    sprite.color = Color.magenta;
                    MoveLeft(moveSpeed);
                }
            }
            else // Player is right
            {
                direction = Direction.Right;
                if (currentDistance > attackDistance)
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
        // AVOIDING
        if (currentState == State.Avoiding) 
        {
            Direction oppositeDirection = direction == Direction.Left ? Direction.Left : Direction.Right;
            Avoid(moveSpeed, oppositeDirection);
            sprite.color = Color.yellow;

            // Attack Condition
            if (currentDistance <= attackDistance)
                currentState = State.Attacking;
            // Chase Condition
            if (edgeDetected)
            {
                currentState = State.Chasing;
            }
        }
        // MOVING AWAY FROM OTHER ENEMIES
        if (currentState == State.AvoidingEnemy)
        {
            // Avoid Enemy Condition
            if (!currentlyMovingAwayFromEnemy)
            {
                currentlyMovingAwayFromEnemy = true;
                StartCoroutine(AvoidOtherEnemies(1));
            }
            // Attack Condition
            if (currentDistance <= attackDistance)
                currentState = State.Attacking;
        }
        // ATTACKING
        if (currentState == State.Attacking) 
        {
            sprite.color = Color.green;


            // Chase Condition
            if (currentDistance > attackDistance)
                currentState = State.Chasing;

        }
        if(currentState == State.Stunned) 
        {
            sprite.color = Color.grey;
        }
    }

    public bool WhoIsAttacking()
    {
        bool someoneElseIsAttacking = false;
        float closestEnemyDistance = 200;
        EnemyMovementScript closestEnemy = null;
        EnemyMovementScript[] enemies = FindObjectsOfType<EnemyMovementScript>();
        foreach(EnemyMovementScript enemy in enemies)
        {
            if (enemy.currentState == State.Chasing || enemy.currentState == State.Attacking)
                someoneElseIsAttacking = true;
            float myDistance = Vector2.Distance(player.position, enemy.transform.position);
            if (myDistance < closestEnemyDistance)
            {
                closestEnemyDistance = myDistance;
                closestEnemy = enemy;
            }
        }
        if(!someoneElseIsAttacking)
        {
            if (closestEnemy == this)
                currentState = State.Chasing;
            else
                currentState = State.None;
        } 
        return someoneElseIsAttacking;
    }

    void MoveLeft(float moveSpeed)
    {
        //print("LEFT");
        ChangeDirection(Direction.Left);

        enemyCurrentDirection = Direction.Left;

        myRb.velocity = new Vector3(moveSpeed * -1, myRb.velocity.y, 0);
    }

    void MoveRight(float moveSpeed)
    {
        ChangeDirection(Direction.Right);

        enemyCurrentDirection = Direction.Right;

        myRb.velocity = new Vector3(moveSpeed * 1, myRb.velocity.y, 0);
    }

    void ChangeDirection(Direction direction)
    {
        if (direction == Direction.Left)
        {
            Vector3 scale = transform.localScale;
            if (scale.x > 0f)
                scale.x *= -1;
            transform.localScale = scale;
        }
        else if (direction == Direction.Right)
        {
            Vector3 scale = transform.localScale;
            if (scale.x < 0f)
                scale.x *= -1;
            transform.localScale = scale;
        }
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
        currentState = State.None;
    }


    void OnDestroy()
    {
        instances.Remove(gameObject.GetInstanceID());
    }
}
