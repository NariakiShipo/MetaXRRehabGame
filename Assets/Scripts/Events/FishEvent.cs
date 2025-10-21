using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FishEvent : MonoBehaviour
{
    [SerializeField]private GameManager gameEvent;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fish"))
        {
            gameEvent.score += 1;
            Debug.LogWarning("Fish caught! Score: " + gameEvent.score);
            Destroy(collision.gameObject);
        }
    }
}