using UnityEngine;

public class BossShatter : MonoBehaviour
{
    public Transform center;
    public float minImpulse = 6f, maxImpulse = 12f;
    public float minTorque = 5f, maxTorque = 20f;
    public bool useExplosion = true;
    public float radius = 4f, upwards = 0.5f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    public void Fly()
    {
        if (!rb) return;
        rb.isKinematic = false;

        Vector3 c = center ? center.position : transform.position;
        float impulse = Random.Range(minImpulse, maxImpulse);
        float torque = Random.Range(minTorque, maxTorque);

        if (useExplosion) rb.AddExplosionForce(impulse, c, radius, upwards, ForceMode.Impulse);
        else
        {
            Vector3 dir = (rb.worldCenterOfMass - c).normalized;
            rb.AddForce(dir * impulse, ForceMode.Impulse);
        }

        rb.AddTorque(Random.onUnitSphere * torque, ForceMode.Impulse);
    }
}
