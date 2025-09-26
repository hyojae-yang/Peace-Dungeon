using UnityEngine;
using System.Collections.Generic;
using System;

// ���� ���� �����͸� �����ϱ� ���� �����̳� Ŭ�����Դϴ�.
// SaveManager�� SaveDataContainer ������ ���� �����͸� ��� ���� �뵵�Դϴ�.
[Serializable]
public class DungeonCoinSaveData
{
    public int coins; // ���� ���� ����
}

/// <summary>
/// ���� ���� ��ȭ�� �����ϴ� �̱��� Ŭ�����Դϴ�.
/// �� Ŭ������ ���� ��ȯ�Ǿ �ı����� ������, ���� �ý��۰� �����˴ϴ�.
/// SOLID: ���� å�� ��Ģ (���� ���� ��ȭ ����).
/// </summary>
public class DungeonCoinCurrency : MonoBehaviour, ISavable
{
    // �̱��� �ν��Ͻ��� �ܺο� �����ϴ� ���� ������Ƽ�Դϴ�.
    public static DungeonCoinCurrency Instance { get; private set; }

    // ���� �÷��̾ ������ ���� ���� �����Դϴ�.
    // �� ������ ���� �� �ε� ����� �˴ϴ�.
    public int currentDungeonCoins { get; private set; } = 10;

    /// <summary>
    /// �� ��ü�� ������ �� �� �� ȣ��Ǹ�, �̱��� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    private void Awake()
    {
        // �ν��Ͻ��� ���� �������� �ʴ� ���
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // �̹� �ν��Ͻ��� �����ϸ� ���� ������ ��ü�� �ı��Ͽ� �ߺ��� �����մϴ�.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ��ü�� Ȱ��ȭ�� �� �� �� ȣ��Ǹ�, SaveManager�� �ڽ��� ����մϴ�.
    /// </summary>
    private void Start()
    {
        // ������ ����/�ε� �ý��ۿ� �� ��ü�� ����մϴ�.
        SaveManager.Instance.RegisterSavable(this);
    }

    /// <summary>
    /// ���� ������ �߰��ϴ� �޼����Դϴ�. (��: ���� óġ �� ȣ��)
    /// </summary>
    /// <param name="amount">�߰��� ���� ��</param>
    public void AddCoins(int amount)
    {
        if (amount > 0)
        {
            currentDungeonCoins += amount;
            Debug.Log($"<color=yellow>���� ȹ��:</color> ���� ���� {amount}���� ȹ���Ͽ� ���� {currentDungeonCoins}���Դϴ�.");
        }
    }

    /// <summary>
    /// ���� ������ ����ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="amount">����� ���� ��</param>
    /// <returns>���� ��� ���� ����</returns>
    public bool SubtractCoins(int amount)
    {
        if (amount > 0 && currentDungeonCoins >= amount)
        {
            currentDungeonCoins -= amount;
            Debug.Log($"<color=red>���� ���:</color> ���� ���� {amount}���� ����Ͽ� ���� {currentDungeonCoins}���Դϴ�.");
            return true;
        }
        Debug.LogWarning("���� ������ �����մϴ�.");
        return false;
    }

    // === ISavable �������̽� ���� ===

    /// <summary>
    /// ���� ���� ���� ������ ��ȯ�Ͽ� �����մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    public object SaveData()
    {
        // ����� ������ �����̳� ��ü�� �����ϰ� ���� ���� ������ ��� ��ȯ�մϴ�.
        return new DungeonCoinSaveData { coins = currentDungeonCoins };
    }

    /// <summary>
    /// ����� ������(���� ���� ����)�� �ҷ��� �����մϴ�.
    /// �� �޼���� SaveManager�� ���� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="data">����� ���� ���� ������ ���� DungeonCoinSaveData ��ü</param>
    public void LoadData(object data)
    {
        // object Ÿ������ ���� �����͸� DungeonCoinSaveData�� ĳ�����Ͽ� �����մϴ�.
        if (data is DungeonCoinSaveData loadedData)
        {
            currentDungeonCoins = loadedData.coins;
        }
    }
}