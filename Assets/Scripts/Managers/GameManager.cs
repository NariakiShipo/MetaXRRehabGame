using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField]GameObject scoopNet;
    [SerializeField] TMP_Text timerText;
    private static float[] times = new float[] { 180f, 300f, 600f };
    private float timer = times[0];
    private bool isEnd = false;
    public int score = 0;
    
    void Start()
    {
        scoopNet.SetActive(true);
    }
    void Update()
    {
        if (timer >= 0f && !isEnd)
        {
            timer -= Time.deltaTime;
            float minutes = Mathf.Floor(timer / 60f);
            float seconds = Mathf.Floor(timer % 60f);
            timerText.text = "Time: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else
        {
            isEnd = true;
            Debug.LogWarning("Time's up!");
            Time.timeScale = 0f;
        }
    }

    public void SetTime(int index)
    {
        timer = times[index];
    }
}
