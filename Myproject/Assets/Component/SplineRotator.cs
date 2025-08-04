using UnityEngine;

public class SplineRotator : MonoBehaviour
{
    public float rotationSpeed = 100f; // 도/초

    private float currentAngle = 0f;
    public float CurrentRotation => currentAngle;

    void Update()
    {
        float deltaAngle = rotationSpeed * Time.deltaTime;
        currentAngle = (currentAngle + deltaAngle) % 360f;

        // 제자리에서 자전 (자신의 Pivot 기준 회전)
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}
