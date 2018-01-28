using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public float Health = 200;
    private float TotalHealth;

    public GameObject[] CellGuts;
    public ParticleSystem deathEffect;
    private Rigidbody2D rigidBody;

    private CellController seekCell;

    public float AggroDistance = 20f;
    public float SeekModifier = 20f;

    public float DamageDistance = 3f;

    public AnimationCurve animationCurve;
    public ParticleSystem attackParticles;

    public void TakeDamage(float damage)
    {
        Health -= damage;
    }

    float blobboTimeScale;
    private new MeshRenderer renderer;
    private void Start()
    {
        blobboTimeScale = 5f * Random.Range(.7f, 1.3f);
        renderer = GetComponent<MeshRenderer>();
        TotalHealth = Health;
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (Health < 50f)
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

        float healthModifier = 100f / (100f + Health);
        float scaleModifier = Health / 100f + animationModifier;
        float transArgument = Time.time * (blobboTimeScale + healthModifier);

        renderer.transform.localScale = new Vector3(
           scaleModifier + Mathf.Sin(transArgument) * healthModifier,
           scaleModifier + Mathf.Cos(transArgument) * healthModifier,
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
            if(distance < DamageDistance)
            {
                cell.health -= 50f;
            }
        }

        ParticleSystem attack = GameObject.Instantiate(attackParticles, gameObject.transform.position, Quaternion.Euler(180, 0, 0));
        GameObject.Destroy(attack, 3f);
        startAttackAnimation = false;
    }

    private void FixedUpdate()
    {
        if (seekCell)
        {
            Vector3 direction = (seekCell.transform.position - transform.position) * SeekModifier;

            if(direction.sqrMagnitude < 12f)
            {
                direction *= 2;
            }

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

        dead = true;
        ParticleSystem explosion = GameObject.Instantiate(deathEffect, gameObject.transform.position, Quaternion.Euler(180f, 0f, 0f));

        for (int i = 10; i < TotalHealth; i += 10)
        {
            GameObject go = GameObject.Instantiate(CellGuts[(i/10) % CellGuts.Length], gameObject.transform.position, Quaternion.identity);
            go.GetComponent<Rigidbody2D>().AddForce(Quaternion.Euler(i, 0, i) * Vector3.one * 10f, ForceMode2D.Impulse);
        }

        PlayerController.EnemyCells.Remove(this);

        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }

    bool aggroed = false;
    float timeToNextCheck = 0f;
    const float SEEK_CHECK_FREQUENCY = 10f;
    public void SeekClosestCell()
    {
        if(Time.realtimeSinceStartup > timeToNextCheck)
        {
            float distance = aggroed ? float.PositiveInfinity : AggroDistance;
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
