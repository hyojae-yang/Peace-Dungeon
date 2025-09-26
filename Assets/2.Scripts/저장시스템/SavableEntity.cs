using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ��� ���� ������ ���� ������Ʈ�� �����Ǵ� ��ũ��Ʈ�Դϴ�.
/// ������ ID�� �ο��ϰ�, WorldStateSaver�� ���/�����ϴ� ������ �մϴ�.
/// </summary>
public class SavableEntity : MonoBehaviour
{
    // === �ʵ� ===
    [SerializeField]
    private string uniqueID;

    // �� �ʵ�� �ν����Ϳ� ������� ������, �ٸ� ��ũ��Ʈ���� ������ �� �ֽ��ϴ�.
    private string prefabID;

    // === �Ӽ� (Properties) ===
    /// <summary>
    /// ������Ʈ�� ���� ID�� �����ɴϴ�.
    /// </summary>
    public string UniqueID => uniqueID;

    /// <summary>
    /// ������Ʈ�� ������ ��Ÿ���� ������ ID�� �����ɴϴ�.
    /// </summary>
    public string PrefabID => prefabID;

    // === MonoBehaviour �޼��� ===
    private void Awake()
    {
        // ��. ������Ʈ�� ������ �� uniqueID�� ���ٸ� ���� �����մϴ�.
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            // ������ ��忡�� ��������� �����մϴ�.
            EditorUtility.SetDirty(this);
#endif
        }

        // ��. �� ������Ʈ�� ������ ID�� �����մϴ�.
        SetPrefabID();
    }

    private void OnEnable()
    {
        // ������Ʈ�� Ȱ��ȭ�Ǹ� ���� ��Ͽ� �ڽ��� ����մϴ�.
        WorldStateSaver.Register(this);
    }

    private void OnDisable()
    {
        // ������Ʈ�� ��Ȱ��ȭ�ǰų� �ı��Ǹ� ���� ��Ͽ��� �ڽ��� �����մϴ�.
        WorldStateSaver.Deregister(this);
    }

    // === ���� �޼��� (Public Methods) ===
    /// <summary>
    /// �ε�� �����ͷκ��� uniqueID�� �����ϴ� �޼����Դϴ�.
    /// �� �޼���� ���ο� ������Ʈ�� �ε��� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="id">�ε�� �������� uniqueID</param>
    public void SetUniqueID(string id)
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = id;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    // ���� �۾����� ����� �޼����Դϴ�.
    /// <summary>
    /// �� ������Ʈ�� ������ ID�� �����մϴ�.
    /// �� ID�� ���� �� � ������ ���������� �ĺ��ϴ� �� ���˴ϴ�.
    /// </summary>
    public void SetPrefabID()
    {
        // ������Ʈ�� �̸��� ������ ID�� ����մϴ�.
        // ���� ���, Cube��� �̸��� �������̶�� prefabID�� "Cube"�� �˴ϴ�.
        prefabID = gameObject.name.Replace("(Clone)", "").Trim();
    }
}