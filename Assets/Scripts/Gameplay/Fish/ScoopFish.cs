using UnityEngine;
using UnityEngine.InputSystem;

public class ScoopFish : MonoBehaviour
{
    [Header("References")]
    public Transform snapPoint; // Pole snap location

    private FishForwardMovement hoveredFish; // Lantern near pole
    private FishForwardMovement heldFish;    // Lantern currently held
    private bool isHolding = false;

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

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Fish"))
            hoveredFish = collision.gameObject.GetComponent<FishForwardMovement>();
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<FishForwardMovement>() == hoveredFish)
            hoveredFish = null;
        
    }

    private void SnapFish(FishForwardMovement fish)
    {
        if (snapPoint == null || fish == null) return;

        // Snap to pole immediately
        fish.SnapTo(snapPoint);

        snappedFish = fish;
        isHolding = true;
        fish.selected = true;

        hoveredFish = null; // ✅ prevent re-snapping
        Debug.Log("Fish Caught");
    }

    private void ReleaseFish(FishForwardMovement fish)
    {
        if (fish == null) return;

        isHolding = false;
        snappedFish = null;
        fish.selected = false;

        // Return only if NOT in basket
        if (!fish.isInBucket)
            fish.ReturnToOriginal();
    }
}
