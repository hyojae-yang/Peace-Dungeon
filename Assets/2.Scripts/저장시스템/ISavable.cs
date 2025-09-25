using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// �����͸� �����ϰ� �ҷ��� �� �ִ� ��� Ŭ������ �����ؾ� �ϴ� �������̽��Դϴ�.
/// </summary>
public interface ISavable
{
    /// <summary>
    /// ���� ��ũ��Ʈ�� �����͸� SaveData ��ü�� ��ȯ�Ͽ� ��ȯ�մϴ�.
    /// </summary>
    /// <returns>���� ������ ������ ��ü</returns>
    object SaveData();

    /// <summary>
    /// SaveData ��ü�� �����͸� ���� ��ũ��Ʈ�� �����մϴ�.
    /// </summary>
    /// <param name="data">����� ������ ��ü</param>
    void LoadData(object data);
}