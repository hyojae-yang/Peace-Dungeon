using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 모든 저장 가능한 게임 오브젝트에 부착되는 스크립트입니다.
/// 고유한 ID를 부여하고, WorldStateSaver에 등록/해제하는 역할을 합니다.
/// </summary>
public class SavableEntity : MonoBehaviour
{
    // === 필드 ===
    [SerializeField]
    private string uniqueID;

    // 이 필드는 인스펙터에 노출되지 않지만, 다른 스크립트에서 설정할 수 있습니다.
    private string prefabID;

    // === 속성 (Properties) ===
    /// <summary>
    /// 오브젝트의 고유 ID를 가져옵니다.
    /// </summary>
    public string UniqueID => uniqueID;

    /// <summary>
    /// 오브젝트의 종류를 나타내는 프리팹 ID를 가져옵니다.
    /// </summary>
    public string PrefabID => prefabID;

    // === MonoBehaviour 메서드 ===
    private void Awake()
    {
        // ①. 오브젝트가 생성될 때 uniqueID가 없다면 새로 생성합니다.
        if (string.IsNullOrEmpty(uniqueID))
        {
            uniqueID = Guid.NewGuid().ToString();
#if UNITY_EDITOR
            // 에디터 모드에서 변경사항을 저장합니다.
            EditorUtility.SetDirty(this);
#endif
        }

        // ②. 이 오브젝트의 프리팹 ID를 설정합니다.
        SetPrefabID();
    }

    private void OnEnable()
    {
        // 오브젝트가 활성화되면 저장 목록에 자신을 등록합니다.
        WorldStateSaver.Register(this);
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화되거나 파괴되면 저장 목록에서 자신을 해제합니다.
        WorldStateSaver.Deregister(this);
    }

    // === 공개 메서드 (Public Methods) ===
    /// <summary>
    /// 로드된 데이터로부터 uniqueID를 설정하는 메서드입니다.
    /// 이 메서드는 새로운 오브젝트를 로드할 때 호출됩니다.
    /// </summary>
    /// <param name="id">로드된 데이터의 uniqueID</param>
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

    // 다음 작업에서 사용할 메서드입니다.
    /// <summary>
    /// 이 오브젝트의 프리팹 ID를 설정합니다.
    /// 이 ID는 저장 시 어떤 종류의 프리팹인지 식별하는 데 사용됩니다.
    /// </summary>
    public void SetPrefabID()
    {
        // 오브젝트의 이름을 프리팹 ID로 사용합니다.
        // 예를 들어, Cube라는 이름의 프리팹이라면 prefabID도 "Cube"가 됩니다.
        prefabID = gameObject.name.Replace("(Clone)", "").Trim();
    }
}