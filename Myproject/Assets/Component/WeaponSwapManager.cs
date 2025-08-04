using UnityEngine;

public class WeaponSwapManager : MonoBehaviour
{
    public static WeaponSwapManager Instance;

    public bool IsMainWeaponLeft { get; private set; } = true;

    private void Awake()
    {
                // 이미 존재하는 인스턴스가 있으면 자신 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleWeaponSide()
    {
        IsMainWeaponLeft = !IsMainWeaponLeft;
    }
}
