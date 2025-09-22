using UnityEngine;

public enum NoteType { None, WeaponNote, ManualNote, BonusNote, FeverNote, MergeHead, MergeTail }

public class NoteTypeHandler : MonoBehaviour
{
    public NoteType noteType;
}
