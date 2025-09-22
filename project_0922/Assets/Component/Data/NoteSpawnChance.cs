using UnityEngine;

[System.Serializable]
public class NoteSpawnChance
{
    public NoteType noteType;
    
    [Range(0f, 1f)]
    public float spawnChance; // 0 ~ 1 사이 확률
}
