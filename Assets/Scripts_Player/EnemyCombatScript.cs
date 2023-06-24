using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlayerCombatScript;

public class EnemyCombatScript : MonoBehaviour
{
    [SerializeField] private GameObject parryIndicator;
    [SerializeField] private Transform indicatorTrans;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private float parryCooldown;
    private bool isParrying;
    [SerializeField] private float lightAttackCooldown;
    bool isActioning;

    [Header("Attacks")]
    public Attack[] lightAttack;
    [SerializeField] private Transform colliderTrans;

    public Parry parry;
    private bool canBeHurt;
    private int health;
    public float hitStopParry;
    public float hitStopDamage;

    private EnemyMovementScript enemyMovementScript;

    private GameObject[] parryIndicators = new GameObject[3];

    [Serializable]
    public class Attack
    {
        public float[] attackTimings; // How long to hold each frame
        public float lungeForce;
        public Sprite[] attackSprites; // The sprites for each frame of the attack
        public Transform[] colliderTransforms; // Where the collider should be on every frame of the attack
    }

    [Serializable]
    public class Parry
    {
        public float knockbackForce;
        public float[] frameTimings;
        public Sprite[] sprites;
    }

    enum PossibleActions
    {
        None,
        LightAttack,
        HeavyAttack,
        Parry
    }

    private void Start()
    {
        enemyMovementScript = GetComponent<EnemyMovementScript>();
        health = 6;
        canBeHurt = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    bool timing = false;
    public float currentTime;
    private void Update()
    {
        if (timing)
            currentTime += Time.deltaTime;
        if (enemyMovementScript.currentState == EnemyMovementScript.State.Attacking && !isActioning)
            StartCoroutine(FullCombo());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerAttack"))
        {
            GetHit(collision.GetComponentInParent<PlayerCombatScript>().gameObject.transform);
        }
    }
    public void GetHit(Transform playerPosition)
    {
        if (!canBeHurt)
        {
            print("Parried");
            StartCoroutine(PerformParry());
            KnockBack.Begin(GetComponent<Rigidbody2D>(), (transform.position - playerPosition.position).normalized, parry.knockbackForce);
            FindObjectOfType<HitStopScript>().HitStop(hitStopParry);
        }
        else
        {
            health--;
            if(health <= 0)
            {
                for (int i = 0; i < parryIndicators.Length; i++)
                    if (parryIndicators[i].gameObject != null)
                        Destroy(parryIndicators[i].gameObject);
                Destroy(gameObject);
            }
            KnockBack.Begin(GetComponent<Rigidbody2D>(), (playerPosition.position - transform.position).normalized, parry.knockbackForce * 2);
            FindObjectOfType<HitStopScript>().HitStop(hitStopDamage);
        }
    }
    private IEnumerator FullCombo()
    {
        isActioning = true;
        yield return StartCoroutine(PreAttackDisplay(lightAttack));
        currentTime = 0;
        timing = true;

        canBeHurt = true;
        for(int a = 0; a < lightAttack.Length; a++)
        {
            for (int i = 0; i < lightAttack[a].attackSprites.Length; i++)
            {
                spriteRenderer.sprite = lightAttack[a].attackSprites[i];

                if (lightAttack[a].colliderTransforms[i] != null)
                {
                    currentTime = 0;
                    Destroy(parryIndicators[a]);
                    colliderTrans.transform.position = lightAttack[a].colliderTransforms[i].transform.position;
                    colliderTrans.localScale = lightAttack[a].colliderTransforms[i].transform.localScale;
                    colliderTrans.GetComponent<BoxCollider2D>().enabled = true;
                    KnockBack.Begin(GetComponent<Rigidbody2D>(), Vector2.right * transform.localScale.x, lightAttack[a].lungeForce);
                }
                else
                    colliderTrans.GetComponent<BoxCollider2D>().enabled = false;
                yield return new WaitForSeconds(lightAttack[a].attackTimings[i]);
            }
        }
        canBeHurt = false;
        yield return new WaitForSeconds(lightAttackCooldown);
        spriteRenderer.sprite = defaultSprite;

        isActioning = false;
    }

    private IEnumerator PreAttackDisplay(Attack[] nextAttack)
    {
        Array.Clear(parryIndicators,0, parryIndicators.Length);
        float currentWaitTime = 0;
        float nextWaitTime = 0;
        for (int i = 0; i < nextAttack.Length; i++)
        {
            currentWaitTime += nextWaitTime;
            //print("1. Current wait time: " + currentWaitTime + " Next wait time: " + nextWaitTime);
            nextWaitTime = 0;
            bool reachedCollision = false;
            for (int j = 0; j < nextAttack[i].attackTimings.Length; j++)
            {
                if (!reachedCollision)
                {
                    currentWaitTime += nextAttack[i].attackTimings[j];
                    if (nextAttack[i].colliderTransforms[j] != null)
                    {
                        reachedCollision = true;
                        //print("Reached collision at: " + j);
                    }
                }
                else if(reachedCollision)
                {
                    nextWaitTime += nextAttack[i].attackTimings[j];
                }
            }

            if(i != 0)
                yield return new WaitForSeconds(currentWaitTime);

            currentWaitTime = 0;
            parryIndicators[i] = Instantiate(parryIndicator, new Vector2(indicatorTrans.position.x + ((float)i/2), indicatorTrans.position.y), Quaternion.identity, transform);
            
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator PerformParry()
    {
        //GetComponent<PlayerMovement>().canMove = false;
        print("Began Parry IENUMERATOR");
        for (int i = 0; i < parry.sprites.Length; i++)
        {
            spriteRenderer.sprite = parry.sprites[i];
            yield return new WaitForSeconds(parry.frameTimings[i]);
        }
        print("Completed Parry IENUMERATOR");
        spriteRenderer.sprite = defaultSprite;
        //GetComponent<PlayerMovement>().canMove = true;
    }
}
