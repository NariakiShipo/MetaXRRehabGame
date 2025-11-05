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
    
    [Header("Events")]
    [Tooltip("按鈕被按下時觸發")]
    public UnityEvent onButtonPressed;
    
    [Tooltip("按鈕被釋放時觸發")]
    public UnityEvent onButtonReleased;
    
    
    void Start()
    {
        Debug.Log($"[ButtonEvent] {gameObject.name} 已初始化，觸發標籤：{triggerTag}");
    }
    
    void Update()
    {
        if (isPressed)
        {
            onButtonPressed?.Invoke();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // 只對特定標籤的物體做出反應
        if (other.CompareTag(triggerTag))
        {
            // 如果不允許重複按下且已經按下，則忽略
            if (!canRepeatPress && isPressed)
            {
                return;
            }
            
            // 設置為按下狀態
            isPressed = true;
            
            // 觸發按下事件
            onButtonPressed?.Invoke();
            
            Debug.Log($"[ButtonEvent] {gameObject.name} 被按下！觸發者：{other.gameObject.name}");
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
                
                // 觸發釋放事件
                onButtonReleased?.Invoke();
                
                Debug.Log($"[ButtonEvent] {gameObject.name} 被釋放！");
            }
        }
    }
    
    /// <summary>
    /// 手動重置按鈕狀態（用於外部調用）
    /// </summary>
    public void ResetButton()
    {
        isPressed = false;
        
        Debug.Log($"[ButtonEvent] {gameObject.name} 已重置");
    }
    
    /// <summary>
    /// 手動按下按鈕（用於外部調用）
    /// </summary>
    public void PressButton()
    {
        if (!isPressed || canRepeatPress)
        {
            isPressed = true;
            onButtonPressed?.Invoke();
            
            Debug.Log($"[ButtonEvent] {gameObject.name} 被手動按下");
        }
    }
}
