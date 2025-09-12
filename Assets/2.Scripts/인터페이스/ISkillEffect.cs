using UnityEngine;

// ��� ��Ƽ�� ��ų ȿ���� �����ؾ� �ϴ� ��Ģ�� �����մϴ�.
public interface ISkillEffect
{
    /// <summary>
    /// ��ų�� ������ ȿ���� �����մϴ�.
    /// </summary>
    /// <param name="levelInfo">���� ��ų ������ �ش��ϴ� ������</param>
    /// <param name="spawnPoint">��ų ȿ���� ���۵� ��ġ</param>
    /// <param name="playerStats">�÷��̾��� ����</param>
    void ExecuteEffect(SkillLevelInfo levelInfo, Transform spawnPoint, PlayerStats playerStats);
}