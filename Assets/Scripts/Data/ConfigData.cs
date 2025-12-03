using System.Collections.Generic;

public struct FishSpawnConfig
{
    public int MinFishPerColor;
    public string[] EnabledColors;
}

public struct TaskConfig
{
    public TaskType TaskType;
    public int MinFishPerColor;
}