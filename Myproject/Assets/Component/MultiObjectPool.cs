using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class MultiObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PooledPrefab
    {
        public GameObject prefab;
        public int initialSize = 10;
    }

    public List<PooledPrefab> prefabsToPool;
    private Dictionary<GameObject, Queue<GameObject>> poolDict = new();
    private Dictionary<GameObject, GameObject> instanceToPrefab = new();

    public static MultiObjectPool Instance { get; private set; }

    // ✅ 현재 허용된 NoteType 리스트 (ScoreBasedDifficultyManager에서 설정)
    private List<NoteType> allowedNoteTypes = new();
    private List<NoteSpawnChance> currentSpawnChances = new();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var entry in prefabsToPool)
        {
            if (entry.prefab == null) continue;

            var queue = new Queue<GameObject>();
            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
                instanceToPrefab[obj] = entry.prefab;
            }

            poolDict[entry.prefab] = queue;
        }
        }
    public void SetNoteSpawnChances(List<NoteSpawnChance> chances)
    {
        currentSpawnChances = chances;
    }

    public GameObject Get(GameObject prefab)
    {
        if (!poolDict.ContainsKey(prefab))
            poolDict[prefab] = new Queue<GameObject>();

        GameObject obj;
        if (poolDict[prefab].Count > 0)
        {
            obj = poolDict[prefab].Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, transform);
            instanceToPrefab[obj] = prefab;
        }

        foreach (var col in obj.GetComponentsInChildren<Collider2D>(true))
            col.enabled = true;

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(this.transform);
        if (instanceToPrefab.TryGetValue(obj, out GameObject prefab))
        {
            poolDict[prefab].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning("Returned object was not recognized by the pool.");
        }
    }

    public List<GameObject> ActiveObjects
    {
        get
        {
            List<GameObject> allActive = new();
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy && instanceToPrefab.ContainsKey(child.gameObject))
                {
                    allActive.Add(child.gameObject);
                }
            }
            return allActive;
        }
    }

    // ✅ 외부에서 현재 허용된 NoteType을 설정
    public void SetAllowedNoteTypes(List<NoteType> types)
    {
        allowedNoteTypes = types;
    }

    // ✅ 직접 리스트를 받아 랜덤으로 가져오는 방식
    public GameObject GetRandomInclude(List<NoteType> types)
    {
        if (currentSpawnChances == null || currentSpawnChances.Count == 0)
            return null;

        // 1. 확률 누적 합계 계산
        float totalWeight = currentSpawnChances.Sum(c => c.spawnChance);
        float rand = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var chance in currentSpawnChances)
        {
            cumulative += chance.spawnChance;
            if (rand <= cumulative)
            {
                GameObject prefab = FindPrefabByNoteType(chance.noteType);
                if (prefab != null)
                    return Get(prefab);
            }
        }

        return null;
    }
    private GameObject FindPrefabByNoteType(NoteType type)
    {
        foreach (var entry in prefabsToPool)
        {
            var handler = entry.prefab.GetComponent<NoteTypeHandler>();
            if (handler != null && handler.noteType == type)
            {
                return entry.prefab;
            }
        }
        return null;
    }
    public GameObject GetRandomFromTypes(List<NoteType> types)
{
    var candidates = new List<GameObject>();

    foreach (var entry in prefabsToPool)
    {
        GameObject prefab = entry.prefab;
        NoteTypeHandler handler = prefab.GetComponent<NoteTypeHandler>();

        if (handler != null && types.Contains(handler.noteType))
        {
            candidates.Add(prefab);
        }
    }

    if (candidates.Count == 0) return null;

    GameObject selected = candidates[Random.Range(0, candidates.Count)];
    return Get(selected);
}

    // ✅ 사전 설정된 allowedNoteTypes 기반으로 가져오는 오버로드
    public GameObject GetRandomByChance()
    {
        return GetRandomInclude(allowedNoteTypes);
    }
}
