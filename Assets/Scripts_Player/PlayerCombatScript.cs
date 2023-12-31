using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCombatScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private float parryCooldown;
    private bool isParrying;
    [SerializeField] private float lightAttackCooldown;
    private Coroutine currentAction;
    bool isActioning;
    PossibleActions queuedAction;
    [SerializeField]
    private int currentCombo;

    [Header("Attacks")]
    public Attack[] lightAttack;
    [SerializeField] private Transform colliderTrans;

    public Parry parry;
    [SerializeField] private float parryHitStop;
    [SerializeField] private float damagedHitStop;


    [Serializable]
    public class Attack
    {
        public float[] frameTimings; // How long to hold each frame
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
        isParrying = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentCombo = 0;
    }

    private void Update() 
    {
        if (Input.GetMouseButtonDown(0))
            if (!isActioning)
            {
                currentAction = StartCoroutine(LightAttack());
            }
            else if (queuedAction == PossibleActions.None)
                queuedAction = PossibleActions.LightAttack;

        if (Input.GetMouseButtonDown(1))
            if (!isActioning)
            {
                currentAction = StartCoroutine(PerformParry());
            }
            else if (queuedAction == PossibleActions.None)
                queuedAction = PossibleActions.Parry;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("EnemyAttack"))
        {
            if (isParrying)
            {
                collision.GetComponentInParent<EnemyCombatScript>().posture -= 30;
                SoundManager.PlaySound(0, 4);
                KnockBack.Begin(GetComponent<Rigidbody2D>(), (transform.position - collision.GetComponentInParent<EnemyMovementScript>().transform.position).normalized, parry.knockbackForce);
                FindAnyObjectByType<HitStopScript>().HitStop(parryHitStop);
            }
            else
            {
                SoundManager.PlaySound("Hurt 1");
                if (isActioning) InterruptAction();
                KnockBack.Begin(GetComponent<Rigidbody2D>(), (transform.position - collision.GetComponentInParent<EnemyMovementScript>().transform.position).normalized, parry.knockbackForce);
                FindAnyObjectByType<HitStopScript>().HitStop(damagedHitStop);
            }
        }
    }

    private void PlayQueuedAction(PossibleActions previousAction)
    {
        if (queuedAction == previousAction)
            currentCombo++;
        else
            currentCombo = 0;

        // This will be called at the end every other action IF an action was queued during it
        if(queuedAction == PossibleActions.LightAttack)
        {
            currentAction = StartCoroutine(LightAttack());
        }

        if (queuedAction == PossibleActions.Parry)
            currentAction = StartCoroutine(PerformParry());

        queuedAction = PossibleActions.None;
    }

    private void InterruptAction()
    {
        StopCoroutine(currentAction);
        spriteRenderer.sprite = defaultSprite;
        colliderTrans.GetComponent<BoxCollider2D>().enabled = false;
        isActioning = false;
        GetComponent<PlayerMovement>().canMove = true;
    }

    IEnumerator LightAttack()
    {
        isActioning = true;

        if (currentCombo >= lightAttack.Length)
            currentCombo = 0;

        GetComponent<PlayerMovement>().canMove = false;
        for (int i = 0; i < lightAttack[currentCombo].attackSprites.Length; i++)
        {
            spriteRenderer.sprite = lightAttack[currentCombo].attackSprites[i];

            if (lightAttack[currentCombo].colliderTransforms[i] != null)
            {
                colliderTrans.transform.position = lightAttack[currentCombo].colliderTransforms[i].transform.position;
                colliderTrans.localScale = lightAttack[currentCombo].colliderTransforms[i].transform.localScale;
                colliderTrans.GetComponent<BoxCollider2D>().enabled = true;
                KnockBack.Begin(GetComponent<Rigidbody2D>(), Vector2.right * transform.localScale.x, lightAttack[currentCombo].lungeForce);
            }
            else
                colliderTrans.GetComponent<BoxCollider2D>().enabled = false;

            yield return new WaitForSeconds(lightAttack[currentCombo].frameTimings[i]);
        }

        yield return new WaitForSeconds(lightAttackCooldown);
        GetComponent<PlayerMovement>().canMove = true;
        spriteRenderer.sprite = defaultSprite;

        isActioning = false;
        // Check if any action was queue and if so call PlayQueueAction
        if (queuedAction != PossibleActions.None)
            PlayQueuedAction(PossibleActions.LightAttack);
        else
            currentCombo = 0;
    }

    IEnumerator PerformParry()
    {
        isActioning = true;
        GetComponent<PlayerMovement>().canMove = false;
        for (int i = 0; i < parry.sprites.Length; i++)
        {
            spriteRenderer.sprite = parry.sprites[i];
    
            if (i == 0)
            {
                isParrying = true;
            }
            else
                isParrying = false;
            yield return new WaitForSeconds(parry.frameTimings[i]);
        }
        isParrying = false;
        spriteRenderer.sprite = defaultSprite;
        yield return new WaitForSeconds(parryCooldown);
        GetComponent<PlayerMovement>().canMove = true;
        isActioning = false;
        if (queuedAction != PossibleActions.None)
            PlayQueuedAction(PossibleActions.Parry);
    }
}
