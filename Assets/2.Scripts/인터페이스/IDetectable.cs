using UnityEngine;

// ���Ϳ��� ������ �� �ִ� ��� ����� �����ؾ� �ϴ� �������̽��Դϴ�.
public interface IDetectable
{
    // ���� �������� ���θ� ��ȯ�ϴ� �޼����Դϴ�.
    bool IsDetectable();

    // �� �޼���� ���Ͱ� ������ ����� ��ġ�� ��� ���� ���˴ϴ�.
    Transform GetTransform();
}