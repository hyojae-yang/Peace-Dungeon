using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            Debug.Log("문끼리 닿음");
            // 충돌한 두 문을 비활성화
            collision.gameObject.SetActive(false);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            Debug.Log("문끼리 떨어짐");
            // 충돌이 끝난 후 문을 다시 활성화
            collision.gameObject.SetActive(true);
        }
    }
}
