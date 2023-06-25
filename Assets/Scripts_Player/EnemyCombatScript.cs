using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public int posture = 100;

    private EnemyMovementScript enemyMovementScript;

    private GameObject[] parryIndicators = new GameObject[3];

    [SerializeField] private float stunTime;
    private bool isStunned;
    private Coroutine currentAction;
    private Coroutine currentParryIndicatorAction;

    private bool invincible = false;

    [SerializeField] private float attackVariationAmount;

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
        StartCoroutine(StunTimer());
        enemyMovementScript = GetComponent<EnemyMovementScript>();
        health = 6;
        canBeHurt = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    bool timing = false;
    public float currentTime;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            posture = 0;
        if (timing)
            currentTime += Time.deltaTime;
        if (enemyMovementScript.currentState == EnemyMovementScript.State.Attacking && !isActioning)
            currentAction = StartCoroutine(FullCombo());
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("PlayerAttack") && !invincible)
        {
            StartCoroutine(HitCooldown());
            GetHit(collision.GetComponentInParent<PlayerCombatScript>().gameObject.transform);
        }
    }
    public void GetHit(Transform playerPosition)
    {
        if (!canBeHurt)
        {
            StartCoroutine(PerformParry());
            KnockBack.Begin(GetComponent<Rigidbody2D>(), (transform.position - playerPosition.position).normalized, parry.knockbackForce);
            FindObjectOfType<HitStopScript>().HitStop(hitStopParry);
        }
        else
        {
            health--;
            SoundManager.PlaySound("Hurt 1");
            if(health <= 0)
            {
                for (int i = 0; i < parryIndicators.Length; i++)
                    if (parryIndicators[i] != null)
                        Destroy(parryIndicators[i].gameObject);
                Destroy(gameObject);
            }
            KnockBack.Begin(GetComponent<Rigidbody2D>(), (transform.position - playerPosition.position).normalized, parry.knockbackForce);
            FindObjectOfType<HitStopScript>().HitStop(hitStopDamage);
        }
    }
    private IEnumerator HitCooldown()
    {
        invincible = true;
        yield return new WaitForEndOfFrame();
        invincible = false;
    }
    private IEnumerator FullCombo()
    {
        if (isStunned)
            yield break;

        float[] attackVariation = AttackVariation(lightAttack);

        isActioning = true;
        currentParryIndicatorAction = StartCoroutine(PreAttackDisplay(lightAttack, attackVariation));
        yield return currentParryIndicatorAction;
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
                if (i == 0)
                {
                    print("Waiting for: " + attackVariation[a] + " Seconds");
                    yield return new WaitForSeconds(lightAttack[a].attackTimings[i] + attackVariation[a]);
                }
                else
                    yield return new WaitForSeconds(lightAttack[a].attackTimings[i]);
            }
        }
        canBeHurt = false;
        yield return new WaitForSeconds(lightAttackCooldown);
        spriteRenderer.sprite = defaultSprite;

        isActioning = false;
    }
    private void InterruptAction()
    {
        StopCoroutine(currentAction);
        if (currentParryIndicatorAction != null)
        {
            StopCoroutine(currentParryIndicatorAction);
            for (int i = 0; i < parryIndicators.Length; i++)
                if (parryIndicators[i] != null)
                    Destroy(parryIndicators[i].gameObject);
        }
        colliderTrans.GetComponent<BoxCollider2D>().enabled = false;
        invincible = false;
        spriteRenderer.sprite = defaultSprite;
        isActioning = false;
    }
    private float[] AttackVariation(Attack[] nextAttack)
    {
        float[] variation = new float[nextAttack.Length];
        for (int i = 0; i < nextAttack.Length; i++)
        {
            variation[i] = UnityEngine.Random.Range(-attackVariationAmount, attackVariationAmount);
        }
        return variation;
    }
    private IEnumerator PreAttackDisplay(Attack[] nextAttack, float[] attackVariation)
    {
        if (isStunned)
            yield break;


        Array.Clear(parryIndicators,0, parryIndicators.Length);
        float currentWaitTime = 0;
        float nextWaitTime = 0;
        for (int i = 0; i < nextAttack.Length; i++)
        {
            attackVariation[i] = UnityEngine.Random.Range(-0.2f, 0.2f);
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
                    if (j == 0)
                        nextWaitTime += nextAttack[i].attackTimings[j] + attackVariation[i];
                    else
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
        SoundManager.PlaySound(0, 4);
        posture -= 10;
        for (int i = 0; i < parry.sprites.Length; i++)
        {
            spriteRenderer.sprite = parry.sprites[i];
            yield return new WaitForSeconds(parry.frameTimings[i]);
        }
        spriteRenderer.sprite = defaultSprite;
        //GetComponent<PlayerMovement>().canMove = true;
    }

    IEnumerator Stunned()
    {
        if(currentAction != null)
            InterruptAction();
        canBeHurt = true;
        enemyMovementScript.currentState = EnemyMovementScript.State.Stunned;
        yield return new WaitForSeconds(stunTime);
        enemyMovementScript.currentState = EnemyMovementScript.State.None;
        canBeHurt = false;
    }

    IEnumerator StunTimer()
    {
        while(true)
        {
            if (posture <= 0)
            {
                posture = 0;
                StartCoroutine(Stunned());
            }
            if (posture > 100) posture = 100;
            if(posture >= 0 && posture < 100) posture += 2;

            yield return new WaitForSeconds(0.2f);
        }
    }
}
