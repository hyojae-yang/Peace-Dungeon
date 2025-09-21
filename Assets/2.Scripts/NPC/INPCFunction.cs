using UnityEngine;

/// <summary>
/// NPC�� Ư�� ����� �����ϴ� �������̽�.
/// �� �������̽��� �����ϴ� ��� Ŭ������ NPC���� ��ȣ�ۿ� ��
/// UI ��ư�� �����ϰ� Ư�� ����� �����ϴ� �� ���˴ϴ�.
/// </summary>
public interface INPCFunction
{
    /// <summary>
    /// UI ��ư�� ǥ�õ� �̸��� ��ȯ�մϴ�.
    /// �� ������Ƽ�� �ش� ����� �̸��� ��Ÿ���ϴ� (��: "����", "���尣").
    /// </summary>
    string FunctionButtonName { get; }

    /// <summary>
    /// UI ��ư�� Ŭ���Ǿ��� �� ȣ��� �Լ��Դϴ�.
    /// �� �޼���� �ش� ����� �ٽ� ����(��: ���� UI ����)�� �����մϴ�.
    /// </summary>
    void ExecuteFunction();
}