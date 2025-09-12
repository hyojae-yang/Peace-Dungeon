using UnityEngine;

public class Door : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            Debug.Log("������ ����");
            // �浹�� �� ���� ��Ȱ��ȭ
            collision.gameObject.SetActive(false);

        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            Debug.Log("������ ������");
            // �浹�� ���� �� ���� �ٽ� Ȱ��ȭ
            collision.gameObject.SetActive(true);
        }
    }
}
