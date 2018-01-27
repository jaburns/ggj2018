using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class CellController : MonoBehaviour 
{
    Rigidbody2D rb;

    [SerializeField] AnimationCurve attractionCurve;
    [SerializeField] float attractionScale;
    [SerializeField] AnimationCurve repulsionCurve;
    [SerializeField] float repulsionScale;
    [SerializeField] float churn = 1f;
    public ParticleSystem explosionEffect;

    public List<CellController> attraction { get; set; }
    public bool selected { get; set; }
    public Vector3 seekPoint { get; set; }

    MeshRenderer renderer;
    float blobboTimeScale;
    GameObject rotoBoi;
    float rotoBoiSign;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attraction = new List<CellController>(PlayerController.AllCells);
        attraction.Remove(this);
        FindRotoboi();

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

    public void FindRotoboi()
    {
        rotoBoiSign = Random.value > .5f ? 1f : -1f;

        var bestSqrDist = float.MaxValue;
        foreach (var cell in attraction) {
            var sqrDist = (cell.transform.position - transform.position).sqrMagnitude;
            if (sqrDist < bestSqrDist) {
                bestSqrDist = sqrDist;
                rotoBoi = cell.gameObject;
            }
        }
    }

    void FixedUpdate()
    {
        foreach (var cell in attraction) {
            var ds = cell.transform.position - transform.position;
            var dsMag = ds.magnitude;
            var dsNorm = ds / dsMag;
            var normalizedDist = Mathf.Clamp(dsMag, 0f, 10f) / 10f;

            var attraction = attractionCurve.Evaluate(normalizedDist) * attractionScale;
            var repulsion = repulsionCurve.Evaluate(normalizedDist) * repulsionScale;

            rb.AddForce((attraction - repulsion) *  dsNorm);
        }


        var seekVec = seekPoint - transform.position;
        var seekForce = 1f * seekVec.normalized * Mathf.Clamp(seekVec.magnitude, 1f, 10f);
        rb.AddForce(seekForce);

        if (rotoBoi != null) {
            var rotoVec = rotoBoiSign * Vector3.Cross(transform.position - rotoBoi.transform.position, Vector3.forward).normalized;
            rb.AddForce(rotoVec * churn);
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