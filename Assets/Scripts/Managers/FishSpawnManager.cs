using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FishSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] fishPrefab;
    
    [Tooltip("場景中的生成點（空的 GameObjects）")]
    [SerializeField] private Transform[] spawnPoints;
    
    [Tooltip("每種魚的最小和最大生成數量（如果為0則使用生成點數量）")]
    [SerializeField] private int minSpawnCount = 0;
    [SerializeField] private int maxSpawnCount = 0;
    
    [Header("Randomization Settings")]
    [Tooltip("Y 軸隨機偏移範圍（上下浮動）")]
    [SerializeField] private float yAxisRandomOffset = 0.1f;
    
    [Tooltip("X, Z 軸微幅隨機偏移（可選）")]
    [SerializeField] private float xzAxisRandomOffset = 0.05f;
    
    [Tooltip("生成後等待物理穩定的時間")]
    [SerializeField] private float spawnDelay = 0.1f;
    
    [Header("Spawn Point Settings")]
    [Tooltip("是否隨機打亂生成點順序")]
    [SerializeField] private bool shuffleSpawnPoints = true;
    
    [Tooltip("是否允許重複使用生成點")]
    [SerializeField] private bool allowReuseSpawnPoints = false;
    
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
        if(spawnPoints != null && spawnPoints.Length > 0 && fishPrefab != null)
        {
            StartCoroutine(SpawnFishWithDelay());
        }
        else
        {
            Debug.LogError("[FishSpawnManager] 請在 Inspector 中設置 Spawn Points 和 Fish Prefabs！");
        }
    }
    
    /// <summary>
    /// 初始化 Fish 資料（在生成 GameObject 之前）
    /// </summary>
    private void InitializeFishData()
    {
        fish.Clear();
        
        // 計算要生成的魚數量
        int totalSpawnPointsCount = spawnPoints != null ? spawnPoints.Length : 0;
        
        // 為每種魚預先創建資料物件
        for (int i = 0; i < fishname.Length && i < fishPrefab.Length; i++)
        {
            int spawnCount;
            
            // 如果設置了 min/max spawn count，使用隨機數量
            if (maxSpawnCount > 0)
            {
                spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
            }
            else
            {
                // 否則，平均分配生成點給每種魚
                int pointsPerFishType = totalSpawnPointsCount / fishname.Length;
                spawnCount = pointsPerFishType;
            }
            
            fish.Add(new Fish(fishname[i], spawnCount, i + 1));
            
            Debug.Log($"[FishSpawnManager] 初始化 Fish 資料: {fishname[i]} - 預計生成 {spawnCount} 隻");
        }
        
        isDataInitialized = true;
        Debug.Log($"[FishSpawnManager] Fish 資料初始化完成，總共 {fish.Count} 種魚，可用生成點：{totalSpawnPointsCount} 個");
    }

    /// <summary>
    /// 使用場景中的生成點生成魚
    /// 新流程：
    /// 1. 準備生成點列表（可選擇是否打亂順序）
    /// 2. 根據生成點位置分配給每種魚
    /// 3. 加入 Y 軸隨機偏移讓魚看起來隨機
    /// 4. 延遲生成避免物理碰撞
    /// </summary>
    private IEnumerator SpawnFishWithDelay()
    {
        // 確保 Fish 資料已初始化
        if (!isDataInitialized)
        {
            Debug.LogError("[FishSpawnManager] Fish 資料尚未初始化！");
            yield break;
        }
        
        // 確保有生成點
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[FishSpawnManager] 沒有設置生成點！請在 Inspector 中添加 Spawn Points");
            yield break;
        }
        
        // Step 1: 準備生成點列表
        List<Transform> availableSpawnPoints = PrepareSpawnPointsList();
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogError("[FishSpawnManager] 沒有可用的生成點！");
            yield break;
        }
        
        Debug.Log($"[FishSpawnManager] 總共 {GetTotalSpawnedCount()} 隻魚，可用生成點：{availableSpawnPoints.Count} 個");
        
        // Step 2: 依序分配生成點給每種魚並生成 GameObject
        int spawnPointIndex = 0;
        List<GameObject> spawnedFishObjects = new List<GameObject>();
        
        for(int i = 0; i < fish.Count && i < fishPrefab.Length; i++)
        {
            Fish fishData = fish[i];
            int spawnCount = fishData.spawnedAmount;

            Debug.Log($"[FishSpawnManager] 開始生成 {fishData.color}: {spawnCount} 隻");
            
            // 為這種魚生成對應數量的 GameObject
            for(int j = 0; j < spawnCount; j++)
            {
                // 檢查是否還有可用的生成點
                if (spawnPointIndex >= availableSpawnPoints.Count)
                {
                    if (allowReuseSpawnPoints)
                    {
                        // 如果允許重複使用，回到列表開頭
                        spawnPointIndex = 0;
                        Debug.Log($"[FishSpawnManager] 生成點用完，開始重複使用");
                    }
                    else
                    {
                        Debug.LogWarning($"[FishSpawnManager] 生成點不足，無法生成所有 {fishData.color}");
                        fishData.DecrementSpawned();
                        break;
                    }
                }
                
                // Step 3: 從生成點獲取位置，並加入隨機偏移
                Transform spawnPoint = availableSpawnPoints[spawnPointIndex];
                Vector3 spawnPosition = GetRandomizedPosition(spawnPoint.position);

                // Step 4: 生成魚 GameObject
                GameObject spawnedFish = Instantiate(fishPrefab[i], spawnPosition, Quaternion.identity);
                
                if (spawnedFish == null)
                {
                    Debug.LogError($"[FishSpawnManager] 無法生成 {fishData.color} 的 GameObject");
                    fishData.DecrementSpawned();
                    continue;
                }
                
                spawnedFishObjects.Add(spawnedFish);
                
                // 初始化 Rigidbody
                Rigidbody rb = spawnedFish.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    // 延遲初始化移動
                    StartCoroutine(InitializeFishMovement(spawnedFish, 0.2f));
                }
                
                Debug.Log($"[FishSpawnManager] 生成 {fishData.color} #{j+1} 在位置 {spawnPosition}");
                
                spawnPointIndex++;
                
                // 等待一小段時間再生成下一隻魚
                yield return new WaitForSeconds(spawnDelay);
            }
        }
        
        // 輸出總生成數量
        Debug.Log($"[FishSpawnManager] GameObject 生成完成，總共 {spawnedFishObjects.Count} 隻魚");
    }
    
    /// <summary>
    /// 準備生成點列表（可選擇是否打亂順序）
    /// </summary>
    private List<Transform> PrepareSpawnPointsList()
    {
        List<Transform> points = new List<Transform>();
        
        // 過濾掉 null 的生成點
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                points.Add(spawnPoint);
            }
            else
            {
                Debug.LogWarning("[FishSpawnManager] 發現 null 的生成點，已跳過");
            }
        }
        
        // 如果設置了打亂順序
        if (shuffleSpawnPoints)
        {
            ShuffleList(points);
            Debug.Log("[FishSpawnManager] 已隨機打亂生成點順序");
        }
        
        return points;
    }
    
    /// <summary>
    /// 打亂列表順序（Fisher-Yates shuffle）
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    /// <summary>
    /// 從生成點位置獲取隨機化的位置（加入 Y 軸和可選的 X/Z 軸偏移）
    /// </summary>
    private Vector3 GetRandomizedPosition(Vector3 basePosition)
    {
        //float offsetX = Random.Range(-xzAxisRandomOffset, xzAxisRandomOffset);
        float offsetY = Random.Range(-yAxisRandomOffset, yAxisRandomOffset);
        //float offsetZ = Random.Range(-xzAxisRandomOffset, xzAxisRandomOffset);
        
        return new Vector3(
            basePosition.x ,
            basePosition.y + offsetY,  // 主要的 Y 軸隨機偏移
            basePosition.z
        );
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
        isDataInitialized = false;
        
        Debug.Log("[FishSpawnManager] 已清除所有魚");
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
    
    /// <summary>
    /// 在 Scene View 中顯示生成點的輔助線（用於調試）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        
        Gizmos.color = Color.cyan;
        
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
            {
                // 繪製球體標記生成點
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.1f);
                
                // 繪製向上的箭頭
                Gizmos.DrawLine(spawnPoints[i].position, spawnPoints[i].position + Vector3.up * 0.2f);
                
                // 繪製 Y 軸隨機範圍
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoints[i].position + Vector3.up * yAxisRandomOffset, 0.05f);
                Gizmos.DrawWireSphere(spawnPoints[i].position - Vector3.up * yAxisRandomOffset, 0.05f);
                Gizmos.color = Color.cyan;
            }
        }
    }
}
