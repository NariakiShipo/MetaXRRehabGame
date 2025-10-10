using System;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    private Vector3 velocity;
    private float speed = 0.5f;
    private float changeTargetInterval = 3.0f;
    private float timer;

    void Start()
    {
        if (GetComponent<Rigidbody>() == null)
            gameObject.AddComponent<Rigidbody>();
        
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<SphereCollider>();
        
        // Set Rigidbody properties
        velocity = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * speed;
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;
        
        timer += Time.deltaTime;
        if (timer >= changeTargetInterval)
        {
            // Randomly change direction
            velocity = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized * speed;
            timer = 0f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Calculate reflection vector
            Debug.LogWarning("Collided with wall, changing direction.");
            Vector3 normal = collision.contacts[0].normal;
            velocity = Vector3.Reflect(velocity, normal);
        }
    }
}
