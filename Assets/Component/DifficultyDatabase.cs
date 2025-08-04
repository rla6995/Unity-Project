using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DifficultyDatabase", menuName = "Game/Difficulty Database")]
public class DifficultyDatabase : ScriptableObject
{
    [Tooltip("낮은 점수부터 오름차순으로 정렬된 설정 리스트")]
    public List<DifficultySetting> settings;
}
