using UnityEngine;

public class CenterParent : MonoBehaviour
{
    [Tooltip("勾選後，將父物件移動到所有子物件的中心。此操作只會執行一次。")]
    public bool CenterNow = false;

    // 在編輯器中運行，以便您可以立即看到結果（可選）
    // [ExecuteInEditMode] 

    void Update()
    {
        if (CenterNow)
        {
            CenterParentAroundChildren();
            // 重置 CenterNow，避免每幀都重複執行
            CenterNow = false; 
        }
    }

    private void CenterParentAroundChildren()
    {
        // 確保至少有一個子物件
        if (transform.childCount == 0)
        {
            Debug.LogWarning("父物件 " + gameObject.name + " 沒有任何子物件可以計算中心點。");
            return;
        }

        Vector3 sumPosition = Vector3.zero;
        int childCount = 0;

        // 1. 計算所有子物件的世界座標位置總和
        foreach (Transform child in transform)
        {
            // 忽略被停用的子物件，如果您想包含它們，可以移除這個條件判斷
            if (child.gameObject.activeInHierarchy) 
            {
                sumPosition += child.position;
                childCount++;
            }
        }

        // 再次檢查是否有活動的子物件
        if (childCount == 0)
        {
            Debug.LogWarning("父物件 " + gameObject.name + " 的子物件皆未啟用。");
            return;
        }

        // 2. 計算中心點（平均位置）
        Vector3 centerPosition = sumPosition / childCount;

        // 3. 計算父物件需要移動的位移量
        Vector3 offset = centerPosition - transform.position;

        // 4. 將父物件移到新的中心點
        transform.position = centerPosition;

        // 5. 保持子物件在世界空間中的位置不變
        // 因為父物件的位置改變了，所以必須將子物件向相反方向移動相同的位移量，
        // 才能抵消父物件移動造成的影響，使其保持在原來的世界座標位置。
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                child.position -= offset;
            }
        }

        Debug.Log("父物件 " + gameObject.name + " 已重新定位到其子物件的中心：" + centerPosition);
    }
}