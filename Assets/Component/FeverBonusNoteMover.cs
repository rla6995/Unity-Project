using UnityEngine;

public class FeverBonusNoteMover : MonoBehaviour
{
    private float moveSpeed = 10f;

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    void Update()
    {
        transform.position += Vector3.right * moveSpeed * Time.deltaTime;

        if (transform.position.x > 11f) // 오차 방지 여유 추가
        {
            MultiObjectPool pool = FindAnyObjectByType<MultiObjectPool>();
            if (pool != null)
                pool.Return(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
