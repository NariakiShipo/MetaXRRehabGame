using UnityEngine;
using UnityEngine.Events;

public class ButtonEvent : MonoBehaviour
{
    [Header("Button Settings")]
    [Tooltip("觸發按鈕的標籤（例如：Player, Hand）")]
    [SerializeField] private string triggerTag = "Player";
    
    [Tooltip("按鈕是否被按下")]
    public bool isPressed = false;
    
    [Tooltip("是否可以重複按下")]
    [SerializeField] private bool canRepeatPress = true;
    
    [Tooltip("按鈕冷卻時間（秒）")]
    [SerializeField] private float cooldownTime = 2f;
    
    private float lastPressTime = -999f; // 上次按下的時間
    
    [Header("Audio")]
    [Tooltip("按鈕按下時的音效")]
    [SerializeField] private AudioClip buttonPressSound;
    
    [Tooltip("按鈕釋放時的音效（可選）")]
    [SerializeField] private AudioClip buttonReleaseSound;
    
    [Tooltip("AudioSource 組件（自動獲取或手動指定）")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("音效音量（0-1）")]
    [SerializeField] [Range(0f, 1f)] private float soundVolume = 1f;
    
    [Header("Events")]
    [Tooltip("按鈕被按下時觸發")]
    public UnityEvent onButtonPressed;
    
    [Tooltip("按鈕被釋放時觸發")]
    public UnityEvent onButtonReleased;
    
    
    void Start()
    {
        Debug.Log($"[ButtonEvent] {gameObject.name} 已初始化，觸發標籤：{triggerTag}");
        
        // 自動獲取 AudioSource 組件
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // 如果沒有 AudioSource，嘗試添加一個
            if (audioSource == null && (buttonPressSound != null || buttonReleaseSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                Debug.Log($"[ButtonEvent] 已自動添加 AudioSource 組件");
            }
        }
    }
    
    void Update()
    {
        if (isPressed)
        {
            // 檢查冷卻時間
            if (Time.time - lastPressTime >= cooldownTime)
            {
                onButtonPressed?.Invoke();
                lastPressTime = Time.time;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 只對特定標籤的物體做出反應
        if (other.CompareTag(triggerTag))
        {
            // 檢查冷卻時間
            if (Time.time - lastPressTime < cooldownTime)
            {
                return;
            }
            
            // 如果不允許重複按下且已經按下，則忽略
            if (!canRepeatPress && isPressed)
            {
                return;
            }
            
            // 設置為按下狀態
            isPressed = true;
            
            // 播放按下音效
            PlayButtonPressSound();
            
            // 觸發按下事件
            onButtonPressed?.Invoke();
            lastPressTime = Time.time;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // 只對特定標籤的物體做出反應
        if (other.CompareTag(triggerTag))
        {
            // 如果允許重複按下，則在離開時釋放按鈕
            if (canRepeatPress)
            {
                isPressed = false;
                
                // 播放釋放音效
                PlayButtonReleaseSound();
                
                // 觸發釋放事件
                onButtonReleased?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// 手動重置按鈕狀態（用於外部調用）
    /// </summary>
    public void ResetButton()
    {
        isPressed = false;
        lastPressTime = -999f; // 重置冷卻時間
    }
    
    /// <summary>
    /// 手動按下按鈕（用於外部調用）
    /// </summary>
    public void PressButton()
    {
        // 檢查冷卻時間
        if (Time.time - lastPressTime < cooldownTime)
        {
            return;
        }
        
        if (!isPressed || canRepeatPress)
        {
            isPressed = true;
            onButtonPressed?.Invoke();
            lastPressTime = Time.time;
        }
    }
    
    /// <summary>
    /// 檢查按鈕是否在冷卻中
    /// </summary>
    public bool IsOnCooldown()
    {
        return Time.time - lastPressTime < cooldownTime;
    }
    
    /// <summary>
    /// 獲取剩餘冷卻時間
    /// </summary>
    public float GetRemainingCooldown()
    {
        float remaining = cooldownTime - (Time.time - lastPressTime);
        return Mathf.Max(0f, remaining);
    }
    
    /// <summary>
    /// 播放按鈕按下音效
    /// </summary>
    private void PlayButtonPressSound()
    {
        if (buttonPressSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonPressSound, soundVolume);
        }
    }
    
    /// <summary>
    /// 播放按鈕釋放音效
    /// </summary>
    private void PlayButtonReleaseSound()
    {
        if (buttonReleaseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonReleaseSound, soundVolume);
        }
    }
}
