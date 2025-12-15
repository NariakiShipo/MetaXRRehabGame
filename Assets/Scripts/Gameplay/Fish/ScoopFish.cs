using UnityEngine;
using UnityEngine.InputSystem;

public class ScoopFish : MonoBehaviour
{
    [Header("References")]
    public Transform snapPoint; // Pole snap location

    private FishForwardMovement hoveredFish; // Lantern near pole
    private FishForwardMovement heldFish;    // Lantern currently held
    
    private ButtonEvent hoveredButton; // 懸停的按鈕
    private ButtonEvent pressedButton; // 正在按下的按鈕

    [Header("Controller Settings")]
    public InputActionProperty grabAction; // Assign grip/trigger

    private static FishForwardMovement snappedFish = null; // Only one lantern can snap

    void OnEnable() => grabAction.action.Enable();
    void OnDisable() => grabAction.action.Disable();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float grabValue = grabAction.action.ReadValue<float>();
        
        // Debug: 顯示當前狀態
        if (hoveredButton != null && Time.frameCount % 60 == 0) // 每 60 幀顯示一次
        {
            Debug.Log($"[ScoopFish] hoveredButton={hoveredButton.gameObject.name}, grabValue={grabValue:F2}, pressedButton={(pressedButton != null ? pressedButton.gameObject.name : "null")}");
        }

        // Snap any hovered lantern on grab, no basket check
        if (hoveredFish != null && grabValue > 0.8f && heldFish == null && snappedFish == null)
        {
            heldFish = hoveredFish;
            SnapFish(heldFish);
        }

        // Release held lantern
        if (heldFish != null && grabValue < 0.2f)
        {
            ReleaseFish(heldFish);
            heldFish = null;
        }
        
        // 按鈕互動邏輯
        // 當懸停在按鈕上且抓取值 > 0.8 時，按下按鈕
        if (hoveredButton != null && grabValue > 0.8f && pressedButton == null)
        {   
            Debug.Log($"[ScoopFish] 嘗試按下按鈕, grabValue={grabValue:F2}");
            pressedButton = hoveredButton;
            PressButton(pressedButton);
        }
        
        // 當抓取值 < 0.2 時，釋放按鈕
        if (pressedButton != null && grabValue < 0.2f)
        {
            Debug.Log($"[ScoopFish] 嘗試釋放按鈕, grabValue={grabValue:F2}");
            ReleaseButton(pressedButton);
            pressedButton = null;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[ScoopFish] OnCollisionEnter: {collision.gameObject.name}");
        
        // 檢測魚
        if (collision.gameObject.CompareTag("redFish") || collision.gameObject.CompareTag("grayFish") || collision.gameObject.CompareTag("greenFish"))
        {
            hoveredFish = collision.gameObject.GetComponent<FishForwardMovement>();
        }
        
        // 檢測按鈕（根據 tag 判斷）
        ButtonEvent button = collision.gameObject.GetComponent<ButtonEvent>();
        if (button != null)
        {
            hoveredButton = button;
            Debug.Log($"[ScoopFish] 懸停在按鈕：{collision.gameObject.name}");
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ScoopFish] OnTriggerEnter: {other.gameObject.name}");
        
        // 檢測魚
        if (other.gameObject.CompareTag("redFish") || other.gameObject.CompareTag("grayFish") || other.gameObject.CompareTag("greenFish"))
        {
            hoveredFish = other.gameObject.GetComponent<FishForwardMovement>();
        }
        
        // 檢測按鈕
        ButtonEvent button = other.gameObject.GetComponent<ButtonEvent>();
        if (button != null)
        {
            hoveredButton = button;
            Debug.Log($"[ScoopFish] (觸發器)懸停在按鈕：{other.gameObject.name}");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // 魚離開
        if (collision.gameObject.GetComponent<FishForwardMovement>() == hoveredFish)
        {
            hoveredFish = null;
        }
        
        // 按鈕離開
        ButtonEvent button = collision.gameObject.GetComponent<ButtonEvent>();
        if (button != null && button == hoveredButton)
        {
            hoveredButton = null;
            Debug.Log($"[ScoopFish] 離開按鈕：{collision.gameObject.name}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        // 魚離開
        if (other.gameObject.GetComponent<FishForwardMovement>() == hoveredFish)
        {
            hoveredFish = null;
        }
        
        // 按鈕離開
        ButtonEvent button = other.gameObject.GetComponent<ButtonEvent>();
        if (button != null && button == hoveredButton)
        {
            hoveredButton = null;
            Debug.Log($"[ScoopFish] (觸發器)離開按鈕：{other.gameObject.name}");
        }
    }

    private void SnapFish(FishForwardMovement fish)
    {
        if (snapPoint == null || fish == null) return;

        // Snap to pole immediately
        fish.SnapTo(snapPoint);

        snappedFish = fish;
        fish.selected = true;

        hoveredFish = null; // ✅ prevent re-snapping
        Debug.Log("Fish Caught");
    }

    private void ReleaseFish(FishForwardMovement fish)
    {
        if (fish == null) return;

        snappedFish = null;
        fish.selected = false;

        // Return only if NOT in basket
        if (!fish.isInBucket)
            fish.ReturnToOriginal();
        else
            fish.GoToNewPosition();
    }
    
    /// <summary>
    /// 按下按鈕（調用按鈕的 isPressed 設定）
    /// </summary>
    private void PressButton(ButtonEvent button)
    {
        if (button == null) return;
        
        // 直接設定 isPressed 為 true
        button.isPressed = true;
        
        // 調用按鈕的公開方法來觸發按下事件
        button.PressButton();
        
        Debug.Log($"[ScoopFish] 按下按鈕：{button.gameObject.name}");
    }
    
    /// <summary>
    /// 釋放按鈕（調用按鈕的 isPressed 設定）
    /// </summary>
    private void ReleaseButton(ButtonEvent button)
    {
        if (button == null) return;
        
        // 直接設定 isPressed 為 false
        button.isPressed = false;
        
        Debug.Log($"[ScoopFish] 釋放按鈕：{button.gameObject.name}");
    }
}
