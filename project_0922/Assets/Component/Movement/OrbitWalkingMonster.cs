using UnityEngine;

public class OrbitWalkingMonster : MonoBehaviour
{
    private Transform center;
    private float radius;
    private float angle;
    private SplineRotator rotator;

    private int mySlotIndex = -1;
    private bool[] slotArray;

    private bool isPaused = false; // ✅ 이동 정지 플래그

    private void Update()
    {
        if (isPaused || center == null || rotator == null) return;

        float currentSpeed = rotator.rotationSpeed * Mathf.Deg2Rad;
        angle += currentSpeed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        transform.position = center.position + offset;

        transform.up = (center.position - transform.position).normalized;
    }

    public void Initialize(Transform wheelTransform, float orbitRadius, SplineRotator rotatorRef)
    {
        center = wheelTransform;
        radius = orbitRadius;
        rotator = rotatorRef;

        Vector3 direction = transform.position - center.position;
        angle = Mathf.Atan2(direction.y, direction.x);
    }

    public void SetSlotIndex(int index, bool[] slotRef)
    {
        mySlotIndex = index;
        slotArray = slotRef;
    }

    public void PauseMovement() // ✅ 정지 함수
    {
        isPaused = true;
    }

    public void ResumeMovement() // ✅ 재개 함수
    {
        isPaused = false;
    }

    private void OnDisable()
    {
        if (slotArray != null && mySlotIndex >= 0 && mySlotIndex < slotArray.Length)
        {
            slotArray[mySlotIndex] = false;
        }
    }
}
