using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 語音管理器 - 負責播放任務語音提示
/// 方案C：混合模式
/// - 簡單/中級：直接播放任務語音
/// - 高級：任務開始時播放完整語音，每階段開始時播放當前階段提示
/// </summary>
public class VoiceManager : MonoBehaviour
{
    [Header("音源組件")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("語音片段 - 前綴")]
    [SerializeField] private AudioClip voicePrefix_Please;      // "請幫我撈"
    [SerializeField] private AudioClip voicePrefix_Now;         // "現在請撈"
    
    [Header("語音片段 - 數字")]
    [SerializeField] private AudioClip voiceNumber_1;           // "1 隻"
    [SerializeField] private AudioClip voiceNumber_2;           // "2 隻"
    [SerializeField] private AudioClip voiceNumber_3;           // "3 隻"
    [SerializeField] private AudioClip voiceNumber_4;           // "4 隻"
    [SerializeField] private AudioClip voiceNumber_5;           // "5 隻"
    
    [Header("語音片段 - 顏色")]
    [SerializeField] private AudioClip voiceColor_Red;          // "紅色的"
    [SerializeField] private AudioClip voiceColor_Blue;         // "藍色的"
    [SerializeField] private AudioClip voiceColor_Green;        // "綠色的"
    [SerializeField] private AudioClip voiceColor_Yellow;       // "黃色的"
    
    [Header("語音片段 - 連接詞")]
    [SerializeField] private AudioClip voiceConnector_And;      // "、"（頓號停頓）
    
    [Header("語音片段 - 後綴")]
    [SerializeField] private AudioClip voiceSuffix_Fish;        // "魚"
    
    [Header("完整語音（可選 - 如果使用預錄完整句子）")]
    [Tooltip("如果你有預錄完整句子的音檔，可以在這裡設置")]
    [SerializeField] private bool useFullSentenceAudio = false;
    
    [Header("播放設定")]
    [SerializeField] private float delayBetweenClips = 0.1f;    // 片段之間的延遲
    [SerializeField] private bool enableVoice = true;           // 是否啟用語音
    
    // 管理器引用
    private TaskManager taskManager;
    private MultiBucketManager multiBucketManager;
    private HardModeManager hardModeManager;  // ✅ 困難模式管理器
    
    // 當前播放隊列
    private Queue<AudioClip> playbackQueue = new Queue<AudioClip>();
    private bool isPlaying = false;
    
    private void Awake()
    {
        // 如果沒有指定 AudioSource，自動添加
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 配置 AudioSource
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }
    
    private void Start()
    {
        // 從 ServiceLocator 獲取管理器並立即訂閱事件（在 Start 中執行以確保 ServiceLocator 已註冊完成）
        if (ServiceLocator.Instance.TryGet(out taskManager))
        {
            taskManager.OnTaskGenerated.AddListener(OnTaskGenerated);
            Debug.Log("[VoiceManager] 已訂閱 TaskManager.OnTaskGenerated 事件（簡單/中級模式）");
        }
        else
        {
            Debug.LogWarning("[VoiceManager] TaskManager 未找到，無法訂閱事件");
        }
        
        if (ServiceLocator.Instance.TryGet(out multiBucketManager))
        {
            multiBucketManager.OnBucketStageCompleted.AddListener(OnStageCompleted);
            Debug.Log("[VoiceManager] 已訂閱 MultiBucketManager.OnBucketStageCompleted 事件");
        }
        else
        {
            Debug.LogWarning("[VoiceManager] MultiBucketManager 未找到，無法訂閱事件");
        }
        
        // ✅ 使用單例獲取 HardModeManager（困難模式）
        hardModeManager = HardModeManager.Instance;
        if (hardModeManager != null)
        {
            hardModeManager.OnTaskGenerated.AddListener(OnHardModeTaskGenerated);
            Debug.Log("[VoiceManager] 已訂閱 HardModeManager.OnTaskGenerated 事件（困難模式，使用單例）");
        }
        else
        {
            Debug.LogWarning("[VoiceManager] HardModeManager.Instance 為 null，無法訂閱困難模式事件");
        }
    }
    
    private void OnDestroy()
    {
        // 取消訂閱事件
        if (taskManager != null)
        {
            taskManager.OnTaskGenerated.RemoveListener(OnTaskGenerated);
        }
        
        if (multiBucketManager != null)
        {
            multiBucketManager.OnBucketStageCompleted.RemoveListener(OnStageCompleted);
        }
        
        if (hardModeManager != null)
        {
            hardModeManager.OnTaskGenerated.RemoveListener(OnHardModeTaskGenerated);
        }
    }
    
    /// <summary>
    /// 任務生成時播放語音（簡單/中級模式）
    /// </summary>
    private void OnTaskGenerated(TaskData task)
    {
        if (!enableVoice || task == null) return;
        
        Debug.Log($"[VoiceManager] 任務生成（TaskManager），類型: {task.taskType}");
        
        switch (task.taskType)
        {
            case TaskType.CountOnly:
                // 簡單模式："請幫我撈 X 隻魚"
                PlaySimpleTask(task.targetCount);
                break;
                
            case TaskType.ColorCount:
                // 中級模式："請幫我撈 X 隻 [顏色] 的魚"
                PlayColorTask(task.targetCount, task.targetColor);
                break;
                
            case TaskType.MultiStage:
                // 高級模式：播放完整任務（所有階段）
                PlayFullMultiStageTask(task);
                break;
        }
    }
    
    /// <summary>
    /// 困難模式任務生成時播放語音（HardModeManager）
    /// </summary>
    private void OnHardModeTaskGenerated(HardModeTask hardTask)
    {
        if (!enableVoice || hardTask == null) return;
        
        Debug.Log($"[VoiceManager] 困難模式任務生成（HardModeManager），階段數: {hardTask.stages.Count}");
        
        // 播放完整的多階段任務語音
        PlayFullHardModeTask(hardTask);
    }
    
    /// <summary>
    /// 困難模式：某個階段完成時播放下一階段提示
    /// </summary>
    private void OnStageCompleted(int completedStageIndex)
    {
        if (!enableVoice || taskManager == null) return;
        
        TaskData currentTask = taskManager.GetCurrentTask();
        if (currentTask == null || currentTask.taskType != TaskType.MultiStage) return;
        
        // 獲取下一個階段
        SubTask nextStage = currentTask.GetCurrentSubTask();
        if (nextStage != null)
        {
            Debug.Log($"[VoiceManager] 階段 {completedStageIndex} 完成，播放下一階段提示");
            // 播放當前階段提示："現在請撈 X 隻 [顏色] 的魚"
            PlayCurrentStagePrompt(nextStage);
        }
    }
    
    /// <summary>
    /// 播放簡單模式語音："請幫我撈 X 隻魚"
    /// </summary>
    private void PlaySimpleTask(int count)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        clips.Add(voicePrefix_Please);      // "請幫我撈"
        clips.Add(GetNumberClip(count));    // "X 隻"
        clips.Add(voiceSuffix_Fish);        // "魚"
        
        PlayClipSequence(clips);
    }
    
    /// <summary>
    /// 播放中級模式語音："請幫我撈 X 隻 [顏色] 的魚"
    /// </summary>
    private void PlayColorTask(int count, string colorKey)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        clips.Add(voicePrefix_Please);      // "請幫我撈"
        clips.Add(GetNumberClip(count));    // "X 隻"
        clips.Add(GetColorClip(colorKey));  // "[顏色] 的"
        clips.Add(voiceSuffix_Fish);        // "魚"
        
        PlayClipSequence(clips);
    }
    
    /// <summary>
    /// 播放高級模式完整語音："請幫我撈 X 隻 [顏色] 的魚、Y 隻 [顏色] 的魚、Z 隻 [顏色] 的魚"
    /// </summary>
    private void PlayFullMultiStageTask(TaskData task)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        clips.Add(voicePrefix_Please);      // "請幫我撈"
        
        // 遍歷所有階段
        for (int i = 0; i < task.subTasks.Count; i++)
        {
            SubTask stage = task.subTasks[i];
            
            clips.Add(GetNumberClip(stage.count));      // "X 隻"
            clips.Add(GetColorClip(stage.color));       // "[顏色] 的"
            clips.Add(voiceSuffix_Fish);                // "魚"
            
            // 如果不是最後一個階段，加上連接詞
            if (i < task.subTasks.Count - 1)
            {
                clips.Add(voiceConnector_And);          // "、"
            }
        }
        
        PlayClipSequence(clips);
    }
    
    /// <summary>
    /// 播放困難模式完整語音（HardModeTask）："請幫我撈 X 隻 [顏色] 的魚、Y 隻 [顏色] 的魚、Z 隻 [顏色] 的魚"
    /// </summary>
    private void PlayFullHardModeTask(HardModeTask hardTask)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        clips.Add(voicePrefix_Please);      // "請幫我撈"
        
        // 遍歷所有階段
        for (int i = 0; i < hardTask.stages.Count; i++)
        {
            TaskStage stage = hardTask.stages[i];
            
            clips.Add(GetNumberClip(stage.count));                 // "X 隻"
            clips.Add(GetColorClipFromEnum(stage.targetColor));    // "[顏色] 的"
            clips.Add(voiceSuffix_Fish);                           // "魚"
            
            // 如果不是最後一個階段，加上連接詞
            if (i < hardTask.stages.Count - 1)
            {
                clips.Add(voiceConnector_And);          // "、"
            }
        }
        
        PlayClipSequence(clips);
    }
    
    /// <summary>
    /// 播放當前階段提示："現在請撈 X 隻 [顏色] 的魚"
    /// </summary>
    private void PlayCurrentStagePrompt(SubTask stage)
    {
        List<AudioClip> clips = new List<AudioClip>();
        
        clips.Add(voicePrefix_Now);         // "現在請撈"
        clips.Add(GetNumberClip(stage.count));      // "X 隻"
        clips.Add(GetColorClip(stage.color));       // "[顏色] 的"
        clips.Add(voiceSuffix_Fish);                // "魚"
        
        PlayClipSequence(clips);
    }
    
    /// <summary>
    /// 根據數字獲取對應音檔
    /// </summary>
    private AudioClip GetNumberClip(int number)
    {
        switch (number)
        {
            case 1: return voiceNumber_1;
            case 2: return voiceNumber_2;
            case 3: return voiceNumber_3;
            case 4: return voiceNumber_4;
            case 5: return voiceNumber_5;
            default:
                Debug.LogWarning($"[VoiceManager] 沒有數字 {number} 的音檔");
                return null;
        }
    }
    
    /// <summary>
    /// 根據顏色 key 獲取對應音檔
    /// </summary>
    private AudioClip GetColorClip(string colorKey)
    {
        // 將 FishColor enum 轉換為對應音檔
        FishColor color = FishColorHelper.GetColorFromTag(colorKey);
        return GetColorClipFromEnum(color);
    }
    
    /// <summary>
    /// 根據 FishColor enum 獲取對應音檔
    /// </summary>
    private AudioClip GetColorClipFromEnum(FishColor color)
    {
        switch (color)
        {
            case FishColor.Red:
                return voiceColor_Red;
            case FishColor.Blue:
                return voiceColor_Blue;
            case FishColor.Green:
                return voiceColor_Green;
            case FishColor.Yellow:
                return voiceColor_Yellow;
            default:
                Debug.LogWarning($"[VoiceManager] 沒有顏色 {color} 的音檔");
                return null;
        }
    }
    
    /// <summary>
    /// 播放音檔序列
    /// </summary>
    private void PlayClipSequence(List<AudioClip> clips)
    {
        // 過濾掉 null 音檔
        clips.RemoveAll(clip => clip == null);
        
        if (clips.Count == 0)
        {
            Debug.LogWarning("[VoiceManager] 沒有可播放的音檔");
            return;
        }
        
        // 停止當前播放
        StopCurrentPlayback();
        
        // 將音檔加入隊列
        playbackQueue.Clear();
        foreach (AudioClip clip in clips)
        {
            playbackQueue.Enqueue(clip);
        }
        
        // 開始播放
        StartCoroutine(PlayQueueCoroutine());
    }
    
    /// <summary>
    /// 播放隊列協程
    /// </summary>
    private IEnumerator PlayQueueCoroutine()
    {
        isPlaying = true;
        
        while (playbackQueue.Count > 0)
        {
            AudioClip clip = playbackQueue.Dequeue();
            
            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
                
                Debug.Log($"[VoiceManager] 播放音檔: {clip.name}");
                
                // 等待音檔播放完畢
                yield return new WaitForSeconds(clip.length);
                
                // 片段之間的延遲
                if (playbackQueue.Count > 0)
                {
                    yield return new WaitForSeconds(delayBetweenClips);
                }
            }
        }
        
        isPlaying = false;
        Debug.Log("[VoiceManager] 語音播放完成");
    }
    
    /// <summary>
    /// 停止當前播放
    /// </summary>
    private void StopCurrentPlayback()
    {
        if (isPlaying)
        {
            StopAllCoroutines();
            audioSource.Stop();
            playbackQueue.Clear();
            isPlaying = false;
        }
    }
    
    /// <summary>
    /// 手動播放任務語音（供外部調用）
    /// </summary>
    public void PlayTaskVoice(TaskData task)
    {
        OnTaskGenerated(task);
    }
    
    /// <summary>
    /// 啟用/停用語音
    /// </summary>
    public void SetVoiceEnabled(bool enabled)
    {
        enableVoice = enabled;
        if (!enabled)
        {
            StopCurrentPlayback();
        }
    }
}
