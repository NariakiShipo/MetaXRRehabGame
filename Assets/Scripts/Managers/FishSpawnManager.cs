using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FishSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] fishPrefab;
    [Tooltip("每種魚的最小和最大生成數量")]
    [SerializeField] private int minSpawnCount = 1;
    [SerializeField] private int maxSpawnCount = 4;
    
    [Header("Safety Settings")]
    [Tooltip("魚之間的最小安全距離")]
    [SerializeField] private float minDistanceBetweenFish = 0.5f;
    [Tooltip("魚與牆壁的最小安全距離")]
    [SerializeField] private float wallSafetyMargin = 0.3f;
    [Tooltip("嘗試找到有效位置的最大次數")]
    [SerializeField] private int maxSpawnAttempts = 30;
    [Tooltip("生成後等待物理穩定的時間")]
    [SerializeField] private float spawnDelay = 0.1f;
    
    [Header("References")]
    [SerializeField] private BoxCollider boxCollider;
    
    private List<Fish> fish = new List<Fish>();
    private string[] fishname = {"redFish", "blueFish", "greenFish"};
    private List<Vector3> spawnedPositions = new List<Vector3>();
    private bool isDataInitialized = false; // 標記 Fish 資料是否已初始化
    
    void Awake()
    {
        // 在 Awake 中初始化 Fish 資料（同步執行，確保 Start 時資料已準備好）
        InitializeFishData();
    }
    
    void OnEnable()
    {
        if(boxCollider != null && fishPrefab != null)
        {
            StartCoroutine(SpawnFishWithDelay());
        }
    }
    
    /// <summary>
    /// 初始化 Fish 資料（在生成 GameObject 之前）
    /// </summary>
    private void InitializeFishData()
    {
        fish.Clear();
        
        // 為每種魚預先創建資料物件
        for (int i = 0; i < fishname.Length && i < fishPrefab.Length; i++)
        {
            // 先決定要生成多少隻，但還不生成 GameObject
            int spawnCount = Random.Range(minSpawnCount, maxSpawnCount);
            fish.Add(new Fish(fishname[i], spawnCount, i + 1));
            
            Debug.Log($"[Generator] 初始化 Fish 資料: {fishname[i]} - 預計生成 {spawnCount} 隻");
        }
        
        isDataInitialized = true;
        Debug.Log($"[Generator] Fish 資料初始化完成，總共 {fish.Count} 種魚");
    }

    /// <summary>
    /// 分批生成魚，避免同時生成導致碰撞
    /// </summary>
    private IEnumerator SpawnFishWithDelay()
    {
        // 確保 Fish 資料已初始化
        if (!isDataInitialized)
        {
            Debug.LogError("[Generator] Fish 資料尚未初始化！");
            yield break;
        }
        
        spawnedPositions.Clear();
        
        // 根據已初始化的 Fish 資料生成 GameObject
        for(int i = 0; i < fish.Count && i < fishPrefab.Length; i++)
        {
            Fish fishData = fish[i];
            int spawnCount = fishData.spawnedAmount; // 使用預先決定的數量
            
            Debug.Log($"[Generator] 開始生成 {fishData.color}: {spawnCount} 隻");
            
            // 生成魚 GameObject
            for(int j = 0; j < spawnCount; j++)
            {
                Vector3 spawnPosition = GetSafeSpawnPosition();
                
                if (spawnPosition != Vector3.zero)
                {
                    GameObject spawnedFish = Instantiate(fishPrefab[i], spawnPosition, Quaternion.identity);
                    
                    // 初始化 Rigidbody（如果有的話）
                    Rigidbody rb = spawnedFish.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // 暫時禁用重力和碰撞，讓魚穩定下來
                        rb.useGravity = false;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        
                        // 給魚一個隨機的初始速度方向（較小的速度）
                        StartCoroutine(InitializeFishMovement(spawnedFish, 0.2f));
                    }
                    
                    spawnedPositions.Add(spawnPosition);
                    
                    // 等待一小段時間再生成下一隻魚
                    yield return new WaitForSeconds(spawnDelay);
                }
                else
                {
                    Debug.LogWarning($"[Generator] 無法找到安全位置生成 {fishData.color}，跳過此魚");
                    // 減少實際生成數量
                    fishData.DecrementSpawned();
                }
            }
        }
        
        // 輸出總生成數量
        Debug.Log($"[Generator] GameObject 生成完成，總共 {GetTotalSpawnedCount()} 隻魚");
    }

    /// <summary>
    /// 延遲初始化魚的移動
    /// </summary>
    private IEnumerator InitializeFishMovement(GameObject fish, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        FishMovement movement = fish.GetComponent<FishMovement>();
        if (movement != null)
        {
            // 確保魚的移動腳本已啟動
            movement.enabled = true;
        }
    }

    /// <summary>
    /// Get all Fish data
    /// </summary>
    public List<Fish> GetFish()
    {
        return fish;
    }

    /// <summary>
    /// Get Fish data by color
    /// </summary>
    public Fish GetFishByColor(string color)
    {
        return fish.Find(f => f.color == color);
    }

    /// <summary>
    /// Get total spawned count
    /// </summary>
    public int GetTotalSpawnedCount()
    {
        int total = 0;
        foreach (Fish f in fish)
        {
            total += f.spawnedAmount;
        }
        return total;
    }

    /// <summary>
    /// Get total caught count
    /// </summary>
    public int GetTotalCaughtCount()
    {
        int total = 0;
        foreach (Fish f in fish)
        {
            total += f.caughtAmount;
        }
        return total;
    }

    /// <summary>
    /// 找到一個安全的生成位置（避免與其他魚或牆壁太近）
    /// </summary>
    private Vector3 GetSafeSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 candidatePosition = GetRandomPositionInBox();
            
            // 檢查是否離其他魚夠遠
            if (IsSafeFromOtherFish(candidatePosition))
            {
                return candidatePosition;
            }
        }
        
        // 如果找不到完美位置，返回零向量作為失敗標記
        Debug.LogWarning("[Generator] 嘗試多次後仍無法找到安全的生成位置");
        return Vector3.zero;
    }

    /// <summary>
    /// 檢查位置是否離其他已生成的魚夠遠
    /// </summary>
    private bool IsSafeFromOtherFish(Vector3 position)
    {
        foreach (Vector3 existingPosition in spawnedPositions)
        {
            float distance = Vector3.Distance(position, existingPosition);
            if (distance < minDistanceBetweenFish)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 在 Box 內取得隨機位置（考慮安全邊界）
    /// </summary>
    private Vector3 GetRandomPositionInBox()
    {
        // 取得 BoxCollider 的邊界
        Bounds bounds = boxCollider.bounds;
        
        // 縮小邊界以保持與牆壁的安全距離
        Vector3 safeMin = bounds.min + Vector3.one * wallSafetyMargin;
        Vector3 safeMax = bounds.max - Vector3.one * wallSafetyMargin;
        
        // 確保安全邊界有效
        if (safeMin.x >= safeMax.x || safeMin.y >= safeMax.y || safeMin.z >= safeMax.z)
        {
            Debug.LogWarning("[Generator] BoxCollider 太小，無法應用安全邊界，使用原始邊界");
            safeMin = bounds.min;
            safeMax = bounds.max;
        }

        float randomX = Random.Range(safeMin.x, safeMax.x);
        float randomY = Random.Range(safeMin.y, safeMax.y);
        float randomZ = Random.Range(safeMin.z, safeMax.z);

        return new Vector3(randomX, randomY, randomZ);
    }

    /// <summary>
    /// 清理已生成的魚（用於重新生成）
    /// </summary>
    public void ClearAllFish()
    {
        // 找到所有魚並銷毀
        foreach (string fishTag in fishname)
        {
            GameObject[] fishes = GameObject.FindGameObjectsWithTag(fishTag);
            foreach (GameObject f in fishes)
            {
                Destroy(f);
            }
        }
        
        fish.Clear();
        spawnedPositions.Clear();
        isDataInitialized = false;
        
        Debug.Log("[Generator] 已清除所有魚");
    }

    /// <summary>
    /// 手動觸發重新生成
    /// </summary>
    [ContextMenu("Regenerate All Fish")]
    public void RegenerateAllFish()
    {
        ClearAllFish();
        InitializeFishData(); // 重新初始化資料
        StartCoroutine(SpawnFishWithDelay());
    }
    
    /// <summary>
    /// 檢查 Fish 資料是否已初始化
    /// </summary>
    public bool IsDataInitialized()
    {
        return isDataInitialized;
    }
}
