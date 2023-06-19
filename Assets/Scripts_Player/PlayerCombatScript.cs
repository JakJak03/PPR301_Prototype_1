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


    [Serializable]
    public class Attack
    {
        public float[] attackTimings; // How long to hold each frame
        public Sprite[] attackSprites; // The sprites for each frame of the attack
        public Transform[] colliderTransforms; // Where the collider should be on every frame of the attack
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
                currentAction = StartCoroutine(Parry());
            }
            else if (queuedAction == PossibleActions.None)
                queuedAction = PossibleActions.Parry;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Test Dummy"))
        {
            if (isParrying)
            {
                print("PARRIED");
                FindAnyObjectByType<HitStopScript>().HitStop(0.05f);
            }
            else
            {
                print("HIT");
                if(isActioning) InterruptAction();
                FindAnyObjectByType<HitStopScript>().HitStop(0.4f);
            }
        }
    }

    private void PlayQueuedAction(PossibleActions previousAction)
    {
        if (queuedAction == previousAction)
            currentCombo++;
        else
            currentCombo = 0;

        // This will be called at the end every other action IF an action was queue during it
        if(queuedAction == PossibleActions.LightAttack)
            StartCoroutine(LightAttack());

        if (queuedAction == PossibleActions.Parry)
            StartCoroutine(Parry());

        queuedAction = PossibleActions.None;
    }

    private void InterruptAction()
    {
        StopCoroutine(currentAction);
    }

    IEnumerator LightAttack()
    {
        isActioning = true;

        if (currentCombo >= lightAttack.Length)
            currentCombo = 0;

        for (int i = 0; i < lightAttack[currentCombo].attackSprites.Length; i++)
        {
            spriteRenderer.sprite = lightAttack[currentCombo].attackSprites[i];

            if (lightAttack[currentCombo].colliderTransforms[i] != null)
            {
                colliderTrans.transform.position = lightAttack[currentCombo].colliderTransforms[i].transform.position;
                colliderTrans.localScale = lightAttack[currentCombo].colliderTransforms[i].transform.localScale;
                colliderTrans.GetComponent<BoxCollider2D>().enabled = true;
            }
            else
                colliderTrans.GetComponent<BoxCollider2D>().enabled = false;

            yield return new WaitForSeconds(lightAttack[currentCombo].attackTimings[i]);
        }

        yield return new WaitForSeconds(lightAttackCooldown);
        spriteRenderer.sprite = defaultSprite;

        isActioning = false;
        // Check if any action was queue and if so call PlayQueueAction
        if (queuedAction != PossibleActions.None)
            PlayQueuedAction(PossibleActions.LightAttack);
        else
            currentCombo = 0;
    }

    IEnumerator Parry()
    {
        isActioning = true;
        isParrying = true;
        GetComponent<SpriteRenderer>().color = Color.green;
        yield return new WaitForSeconds(parryCooldown);
        GetComponent<SpriteRenderer>().color = Color.white;
        isParrying = false;
        //Debug.Log("End Action");
        isActioning = false;
        // Check if any action was queue and if so call PlayQueueAction
        if (queuedAction != PossibleActions.None)
            PlayQueuedAction(PossibleActions.Parry);
        else
            currentCombo = 0;
    }
}
