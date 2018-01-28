using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CellController : MonoBehaviour 
{
    Rigidbody2D rb;

    public List<CellController> attraction = new List<CellController>();
    public bool selected { get; set; }
    public Vector3? seekPoint;
    public ParticleSystem apoptosisEffect;
    public float damageDistance = 3f;
    public float damage = 50f;
    public float damageModifier = 2f;
    public float powerUpAmount = 50f;
    public float power = 0f;
    public float mitosisPowerThreshold = 100f;

    MeshRenderer renderer;
    float blobboTimeScale;

    CircleCollider2D circleCollider;
    private float radius;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attraction = new List<CellController>(PlayerController.AllCells);
        attraction.Remove(this);

        renderer = GetComponentInChildren<MeshRenderer>();
        blobboTimeScale = 5f * Random.Range(.7f, 1.3f);

        circleCollider = GetComponent<CircleCollider2D>();
        radius = circleCollider.radius;
    }

    void Update()
    {
        float powerModifier = 1f + power / 100f;

        renderer.transform.localScale = new Vector3(
            powerModifier + 0.1f*Mathf.Sin(Time.time*blobboTimeScale),
            powerModifier + 0.1f*Mathf.Cos(Time.time*blobboTimeScale),
            powerModifier
        );

        //if (selected) {
        //    GetComponentInChildren<Renderer>().material.color = new Color(.2f,.2f,.2f);
        //} else {
        //    GetComponentInChildren<Renderer>().material.color = new Color(.1f,.1f,.1f);
        //}
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider.GetComponent<CellController>();
        if (other != null) {
            if (!other.attraction.Contains(this))
            {
                other.attraction.Add(this);
            }
            else
            if (!attraction.Contains(other))
            {
                attraction.Contains(other);
            }
        }


        string tag = collision.gameObject.tag;
        if(tag == "PowerUp")
        {
            GameObject.Destroy(collision.gameObject);
            power += powerUpAmount;

            circleCollider.radius = radius * (power / 50f);

            if(power >= mitosisPowerThreshold)
            {
                power = 0f;
                GameObject newCell = GameObject.Instantiate(this.gameObject);

                PlayerController.AllCells.Add(newCell.GetComponent<CellController>());

                circleCollider.radius = radius;
            }
        }
    }

    void FixedUpdate()
    {
        foreach (var cell in attraction) {
            var ds = cell.transform.position - transform.position;
            rb.AddForce(0.5f * ds.normalized / ds.magnitude);
        }

        if (seekPoint.HasValue) {
            rb.AddForce(10f * (seekPoint.Value - transform.position).normalized);
        }
    }

    bool dead = false;
    public void Apoptosis()
    {
        if(dead)
        {
            return;
        }

        dead = true;

        Vector3 myPosition = this.gameObject.transform.position;
        foreach(EnemyController ec in PlayerController.EnemyCells)
        {
            float distance = Vector3.Distance(myPosition, ec.transform.position);
            if(distance < damageDistance)
            {
                ec.TakeDamage(damage + damage/(distance + damageModifier));
            }
        }

        ParticleSystem explosion = GameObject.Instantiate(apoptosisEffect, gameObject.transform.position, Quaternion.Euler(180f, 0f, 0f));
        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }
}