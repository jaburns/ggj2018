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
    [SerializeField] Material selectedMaterial;
    [SerializeField] float rotationAmountBase;
    [SerializeField] float rotationAmount;

    public List<CellController> attraction { get; set; }
    public bool selected { get; set; }
    public Vector3 seekPoint { get; set; }
    public ParticleSystem apoptosisEffect;
    public float damageDistance = 3f;
    public float damage = 50f;
    public float damageModifier = 2f;
    public float powerUpAmount = 50f;
    public float power = 0f;
    public float mitosisPowerThreshold = 100f;
    public float health = 100f;

    MeshRenderer renderer;
    float blobboTimeScale;
    GameObject rotoBoi;
    float rotoBoiSign;
    Quaternion animRotation;

    Material ogMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attraction = new List<CellController>(PlayerController.AllCells);
        attraction.Remove(this);
        FindRotoboi();

        renderer = GetComponentInChildren<MeshRenderer>();
        blobboTimeScale = 5f * Random.Range(.7f, 1.3f);

        ogMaterial = renderer.sharedMaterial;

        animRotation = Quaternion.Euler(Random.Range(rotationAmountBase, rotationAmountBase + rotationAmount) * Random.onUnitSphere);

        seekPoint = transform.position;
    }

    void Update()
    {
        if(health < 50f)
        {
            ExplosionEffect();
            return;
        }

        float powerModifier = 1f + power / 100f;

        renderer.transform.rotation *= animRotation;
        renderer.transform.localScale = new Vector3(
            powerModifier + 0.1f*Mathf.Sin(Time.time*blobboTimeScale),
            powerModifier + 0.1f*Mathf.Cos(Time.time*blobboTimeScale),
            powerModifier
        );

        if (selected) {
            renderer.sharedMaterial = selectedMaterial;
        }else{
            renderer.sharedMaterial = ogMaterial;
        }
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

            if(power >= mitosisPowerThreshold)
            {
                power = 0f;
                GameObject newCell = GameObject.Instantiate(this.gameObject);

                PlayerController.AllCells.Add(newCell.GetComponent<CellController>());
            }
        }
    }

    public void FindRotoboi()
    {
        if (this == null) return;
        while (attraction.Remove(null)) { }

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
        while (attraction.Remove(null)) { }

        foreach (var cell in attraction) {
            var ds = cell.transform.position - transform.position;
            var dsMag = ds.magnitude;
            var dsNorm = ds / dsMag;
            var normalizedDist = Mathf.Clamp(dsMag, 0f, 10f) / 10f;

            var attraction = attractionCurve.Evaluate(normalizedDist) * attractionScale;
            var repulsion = repulsionCurve.Evaluate(normalizedDist) * repulsionScale;

            var forceVector = (attraction - repulsion) * dsNorm;

            // TODO why this happen
            if (float.IsNaN(forceVector.x)
                || float.IsNaN(forceVector.y)
                || float.IsNaN(forceVector.z))
            {
                continue;
            }
            rb.AddForce(forceVector);
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

        Vector3 myPosition = this.gameObject.transform.position;
        foreach(EnemyController ec in PlayerController.EnemyCells)
        {
            float distance = Vector3.Distance(myPosition, ec.transform.position);
            if(distance < damageDistance)
            {
                ec.TakeDamage(damage + damage/(distance + damageModifier));
            }
        }

        if (OrganController.Instance != null) {
            float distance = Vector3.Distance(myPosition, OrganController.Instance.transform.position);
            if(distance < damageDistance) {
                OrganController.Instance.GetRekt();
            }
        }

        ExplosionEffect();
    }

    private void ExplosionEffect()
    {
        ParticleSystem explosion = GameObject.Instantiate(apoptosisEffect, gameObject.transform.position, Quaternion.Euler(180f, 0f, 0f));
        GameObject.Destroy(explosion, 4f);
        GameObject.Destroy(gameObject);
    }
}