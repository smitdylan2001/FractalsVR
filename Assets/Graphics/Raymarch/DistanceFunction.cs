using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DistanceFunction : MonoBehaviour
{
    [SerializeField] float radius = 0.5f;
    [SerializeField] float friction = 0.3f;
    [SerializeField] float angularFriction = 0.6f;
    [SerializeField] float restitution = 0.9f;

    private const float MAX_DIST = 10f;
    private const float MIN_DIST = 0.01f;

    private const float STATIC_GRAVITY_MODIFIER = 1.2f;
    private const float BUERIED_GRAVITY_MODIFIER = 3f;

    private Rigidbody rigidbody_;

    struct RaymarchingResult
    {
        public int loop;
        public bool isBuried;
        public float distance;
        public float length;
        public Vector3 direction;
        public Vector3 position;
        public Vector3 normal;
    }

    RaymarchingResult Raymarching(Vector3 dir)
    {
        var dist = 0f;
        var len = 0f;
        var pos = transform.position + radius * dir;
        var loop = 0;

        for (loop = 0; loop < 10; ++loop)
        {
            dist = Functions.CalcDistance(pos);
            len += dist;
            pos += dir * dist;
            if (dist < MIN_DIST || len > MAX_DIST) break;
        }

        var result = new RaymarchingResult();

        result.loop = loop;
        result.isBuried = Functions.CalcDistance(transform.position) < MIN_DIST;
        result.distance = dist;
        result.length = len;
        result.direction = dir;
        result.position = pos;
        result.normal = Functions.CalcNormal(pos);

        return result;
    }

    void Start()
    {
        rigidbody_ = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        var ray = Raymarching(rigidbody_.velocity.normalized);
        var v = rigidbody_.velocity;
        var g = Physics.gravity;

        if (ray.isBuried)
        {
            rigidbody_.AddForce((rigidbody_.mass * g.magnitude * BUERIED_GRAVITY_MODIFIER) * ray.normal);
        }
        else if (ray.length < MIN_DIST)
        {
            var prod = Vector3.Dot(v.normalized, ray.normal);
            var vv = (prod * v.magnitude) * ray.normal;
            var vh = v - vv;
            rigidbody_.velocity = vh * (1f - friction) + (-vv * restitution);
            rigidbody_.AddForce(-rigidbody_.mass * STATIC_GRAVITY_MODIFIER * g);
            rigidbody_.AddTorque(-rigidbody_.angularVelocity * (1f - angularFriction));
        }
    }
}