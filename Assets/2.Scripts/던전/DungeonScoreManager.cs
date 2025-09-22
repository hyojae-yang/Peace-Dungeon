using UnityEngine;
using System.Collections.Generic;

public class DungeonScoreManager : MonoBehaviour
{
    public static DungeonScoreManager Instance { get; private set; }

    private int totalScore = 0;

    private Dictionary<GameObject, int> monsterScores;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// DungeonSpawnManager�κ��� ����-���� �����͸� �ʱ�ȭ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="scores">���� ���ӿ�����Ʈ�� ������ ���� ��ųʸ�</param>
    public void InitializeScores(Dictionary<GameObject, int> scores)
    {
        monsterScores = scores;
        totalScore = 0;
        Debug.Log("DungeonScoreManager�� ���� ���� �����͸� �ʱ�ȭ�߽��ϴ�.");
    }

    /// <summary>
    /// �������� ���� �� ȣ��Ǿ� ���� ������ ����ϰ� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ����</returns>
    public int CalculateFinalScore()
    {
        if (monsterScores == null)
        {
            Debug.LogWarning("���� ����� ���� ���� ��ųʸ��� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return 0;
        }

        // �ı��� ���͵��� ���� ����ϰ� ��ųʸ����� �����մϴ�.
        foreach (var pair in monsterScores)
        {
            // ���� ���ӿ�����Ʈ�� �ı��Ǹ� �� ������ 'null'�� �˴ϴ�.
            if (pair.Key == null)
            {
                totalScore += pair.Value;
            }
        }

        // ���� ������ ���� ��ųʸ��� ���ϴ�.
        monsterScores.Clear();

        return totalScore;
    }
}