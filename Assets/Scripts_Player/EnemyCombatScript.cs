using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCombatScript : MonoBehaviour
{
    [SerializeField] private GameObject parryIndicator;
    [SerializeField] private Transform indicatorTrans;
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
        public float lungeForce;
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
    bool timing = false;
    public float currentTime;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //print("Input Received");
            StartCoroutine(FullCombo());
        }
        if (timing)
            currentTime += Time.deltaTime;
    }
    private void InterruptAction()
    {
        StopCoroutine(currentAction);
    }

    private IEnumerator FullCombo()
    {
        yield return StartCoroutine(PreAttackDisplay(lightAttack));
        //print("Begun Attack");
        currentTime = 0;
        timing = true;
        isActioning = true;

        for(int a = 0; a < lightAttack.Length; a++)
        {
            for (int i = 0; i < lightAttack[a].attackSprites.Length; i++)
            {
                spriteRenderer.sprite = lightAttack[a].attackSprites[i];

                if (lightAttack[a].colliderTransforms[i] != null)
                {
                    print(currentTime);
                    currentTime = 0;
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
        yield return new WaitForSeconds(lightAttackCooldown);
        spriteRenderer.sprite = defaultSprite;

        isActioning = false;
    }

    private IEnumerator PreAttackDisplay(Attack[] nextAttack)
    {
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
            Instantiate(parryIndicator, new Vector2(indicatorTrans.position.x + ((float)i/2), indicatorTrans.position.y), Quaternion.identity);
        }
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator Parry()
    {
        isActioning = true;
        isParrying = true;
        GetComponent<SpriteRenderer>().color = Color.green;
        yield return new WaitForSeconds(parryCooldown);
        GetComponent<SpriteRenderer>().color = Color.white;
        isParrying = false;
        //Debug.Log("End Action");
        isActioning = false;
    }
}
