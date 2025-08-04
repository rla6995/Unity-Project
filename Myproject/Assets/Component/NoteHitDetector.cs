using System.Collections.Generic;
using UnityEngine;

public class NoteHitDetector : MonoBehaviour
{
    public static GameObject GetNearestNote(Vector3 center)
    {
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var obj in MultiObjectPool.Instance.ActiveObjects)
        {
            if (!obj || !obj.activeInHierarchy) continue;
            float dist = Vector2.Distance(center, obj.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = obj;
            }
        }

        return closest;
    }

    // ✅ 다중 판정: Nice 영역 내 노트 검색
    public static GameObject[] GetAllNotesInNiceZone(Collider2D niceZone)
    {
        Bounds bounds = niceZone.bounds;
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        List<GameObject> notes = new List<GameObject>();
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out NoteTypeHandler handler))
            {
                notes.Add(hit.gameObject);
            }
        }

        return notes.ToArray();
    }
}
