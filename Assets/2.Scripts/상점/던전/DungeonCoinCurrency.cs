using UnityEngine;

public class DungeonCoinCurrency : MonoBehaviour
{
    public int currentDungeonCoins { get; private set; } = 100;

    /// <summary>
    /// ���� ������ �߰��ϴ� �޼���. (��: ���� óġ �� ȣ��)
    /// </summary>
    /// <param name="amount">�߰��� ���� ��</param>
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            currentDungeonCoins += amount;
        }
    }

    /// <summary>
    /// ���� ������ ����ϴ� �޼���.
    /// </summary>
    /// <param name="amount">����� ���� ��</param>
    /// <returns>���� ��� ���� ����</returns>
    public bool SubtractCoins(int amount)
    {
        if (amount > 0 && currentDungeonCoins >= amount)
        {
            currentDungeonCoins -= amount;
            return true;
        }
        Debug.LogWarning("���� ������ �����մϴ�.");
        return false;
    }
}