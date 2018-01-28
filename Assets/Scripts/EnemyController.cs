using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

    public float Health = 200;
    private float TotalHealth;

    public GameObject[] CellGuts;
    public ParticleSystem deathEffect;
    private Rigidbody2D rigidBody;

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
        if(Health < 50f)
        {
            DestroyCell();
        }

        float healthModifier = 100f / (100f + Health);
        float scaleModifier = Health / 100f;
        float transArgument = Time.time * (blobboTimeScale + healthModifier);

        renderer.transform.localScale = new Vector3(
           scaleModifier + Mathf.Sin(transArgument) * healthModifier,
           scaleModifier + Mathf.Cos(transArgument) * healthModifier,
           scaleModifier);
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

        for (int i = 50; i < TotalHealth; i += 50)
        {
            GameObject go = GameObject.Instantiate(CellGuts[(i/50) % CellGuts.Length], gameObject.transform.position, Quaternion.identity);
            go.GetComponent<Rigidbody2D>().AddForce(Quaternion.Euler(i, 0, i) * Vector3.one * 10f, ForceMode2D.Impulse);
        }

        PlayerController.EnemyCells.Remove(this);

        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }
}
