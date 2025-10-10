using UnityEngine;

public class GameEvent : MonoBehaviour
{
    public int score = 0;
    private int previousScore = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (score != previousScore)
        {
            Debug.LogWarning("Score: " + score);
            previousScore = score;
        }
    }
}
