using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CellController : MonoBehaviour 
{
    Rigidbody2D rb;

    public List<CellController> attraction = new List<CellController>();
    public bool selected { get; set; }
    public Vector3? seekPoint;
    public ParticleSystem explosionEffect;

    MeshRenderer renderer;
    float blobboTimeScale;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attraction = new List<CellController>(PlayerController.AllCells);
        attraction.Remove(this);

        renderer = GetComponentInChildren<MeshRenderer>();
        blobboTimeScale = 5f * Random.Range(.7f, 1.3f);
    }

    void Update()
    {
        renderer.transform.localScale = new Vector3(
            1f + 0.1f*Mathf.Sin(Time.time*blobboTimeScale),
            1f + 0.1f*Mathf.Cos(Time.time*blobboTimeScale),
            1f
        );

        if (selected) {
            GetComponentInChildren<Renderer>().material.color = new Color(.2f,.2f,.2f);
        } else {
            GetComponentInChildren<Renderer>().material.color = new Color(.1f,.1f,.1f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider.GetComponent<CellController>();
        if (other != null) {
            if (!other.attraction.Contains(this)) {
                other.attraction.Add(this);
            }
            if (!attraction.Contains(other)) {
                attraction.Contains(other);
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
        ParticleSystem explosion = GameObject.Instantiate(explosionEffect, gameObject.transform.position, Quaternion.Euler(180f, 0f, 0f));
        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }
}