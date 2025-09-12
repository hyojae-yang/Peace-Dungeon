using UnityEngine;

// IPassiveEffect �������̽��� ��� ���� �нú� ��ų ��ũ��Ʈ�� �����ؾ� �ϴ� ��Ģ�� �����մϴ�.
public interface IPassiveEffect
{
    /// <summary>
    /// �� ��ų�� ���� ȿ���� Ȱ��ȭ�ϰ� �����մϴ�.
    /// ��ų ������ ���� ȿ���� ������ �����մϴ�.
    /// </summary>
    /// <param name="skillLevel">���� ��ų ����</param>
    void ApplyEffect(int skillLevel);

    /// <summary>
    /// �� ��ų�� ���� ȿ���� ��Ȱ��ȭ�ϰ� �����մϴ�.
    /// </summary>
    void RemoveEffect();

    /// <summary>
    /// ��ų�� ������ ����� �� ȿ���� ������Ʈ�մϴ�.
    /// �нú� ��ų ������ �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="newSkillLevel">����� ��ų ����</param>
    void UpdateLevel(int newSkillLevel);
}