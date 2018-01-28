using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {
    public GameObject[] CellGuts;
    public ParticleSystem deathEffect;
    private Rigidbody2D rigidBody;

    private CellController seekCell;

    public float Difficulty = 1f;
    public float AggroDistModifier = 10f;
    public float SpeedModifier = 10f;
    public float DmgDistModifier = 1.5f;
    public float HealthModifier = 100;

    private float TotalHealth;
    private float CurrentHealth;

    public AnimationCurve animationCurve;
    public ParticleSystem attackParticles;

    public GameObject doorToOpen;

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
    }

    float blobboTimeScale;
    private new MeshRenderer renderer;
    private void Start()
    {
        blobboTimeScale = 5f * Random.Range(.7f, 1.3f);
        renderer = GetComponentInChildren<MeshRenderer>();
        TotalHealth = HealthModifier;
        rigidBody = GetComponent<Rigidbody2D>();

        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.radius = collider.radius * Difficulty;

        CurrentHealth = TotalHealth = Difficulty * HealthModifier;

        Audio.SetLoopPlaying("menuloop", false);
        Audio.SetLoopPlaying("levelloop", true);
    }

    private void Update()
    {
        if (CurrentHealth < 50f)
        {
            DestroyCell();
        }

        float animationModifier = 0f;
        if(startAttackAnimation)
        {
            float animationTime = Time.realtimeSinceStartup - timeAttackStarted;
            animationModifier = animationCurve.Evaluate(animationTime);

            if(animationTime > 1f)
            {
                Attack();
            }
        }

        float damageIndication = 1f - CurrentHealth / TotalHealth;
        float scaleModifier = CurrentHealth / 100f + animationModifier;
        float transArgument = Time.time * (blobboTimeScale + damageIndication);

        renderer.transform.localScale = new Vector3(
           scaleModifier + Mathf.Sin(transArgument) * damageIndication,
           scaleModifier + Mathf.Cos(transArgument) * damageIndication,
           scaleModifier);

        SeekClosestCell();
    }

    bool startAttackAnimation = false;
    float timeAttackStarted = 0f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        CellController cell = collision.collider.GetComponent<CellController>();
        if(cell && !startAttackAnimation)
        {
            startAttackAnimation = true;
            timeAttackStarted = Time.realtimeSinceStartup;
        }
    }

    public void Attack()
    {
        Vector3 myPos = this.gameObject.transform.position;
        foreach(CellController cell in PlayerController.AllCells)
        {
            float distance = Vector3.Distance(myPos, cell.transform.position);
            if(distance < DmgDistModifier * Difficulty)
            {
                cell.health -= 50f;
            }
        }

        ParticleSystem attack = GameObject.Instantiate(attackParticles, gameObject.transform.position, Quaternion.Euler(180, 0, 0));
        attack.transform.localScale = new Vector3(1, 1, 1) * Difficulty / 2f;
        GameObject.Destroy(attack, 3f);
        startAttackAnimation = false;
    }

    private void FixedUpdate()
    {
        if (seekCell)
        {
            Vector3 direction = (seekCell.transform.position - transform.position).normalized * SpeedModifier * Difficulty;

            rigidBody.AddForce(direction, ForceMode2D.Impulse);
        }
        else
        {
            aggroed = false;
        }
    }

    bool dead = false;
    public void DestroyCell()
    {
        if(dead)
        {
            return;
        }

        if (doorToOpen != null) {
            var p = FindObjectOfType<PlayerController>();
            p.StartCoroutine(p.TriggerCameraPanAndOpen(doorToOpen));
        }

        dead = true;
        ParticleSystem explosion = GameObject.Instantiate(deathEffect, gameObject.transform.position, Quaternion.Euler(180f, 0f, 0f));

        for (int i = 5; i < TotalHealth; i += 5)
        {
            GameObject go = GameObject.Instantiate(CellGuts[(i/5) % CellGuts.Length], gameObject.transform.position, Quaternion.identity);
            go.GetComponent<Rigidbody2D>().AddForce(Quaternion.Euler(i, 0, i) * Vector3.one * 10f, ForceMode2D.Impulse);
        }

        PlayerController.EnemyCells.Remove(this);

        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }

    bool aggroed = false;
    float timeToNextCheck = 0f;
    const float SEEK_CHECK_FREQUENCY = 2f;
    public void SeekClosestCell()
    {
        if(Time.realtimeSinceStartup > timeToNextCheck)
        {
            float distance = aggroed ? float.PositiveInfinity : AggroDistModifier * Difficulty;
            CellController closestCell = null;

            foreach(CellController cell in PlayerController.AllCells)
            {
                float currDist = Vector3.Distance(cell.transform.position, this.transform.position);
                if(currDist < distance)
                {
                    distance = currDist;
                    closestCell = cell;

                    aggroed = true;
                }
            }

            seekCell = closestCell;

            timeToNextCheck = Time.realtimeSinceStartup + SEEK_CHECK_FREQUENCY;
        }
    }
}
