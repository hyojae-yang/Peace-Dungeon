using UnityEngine;
using System;

/// <summary>
/// 이 스크립트가 부착된 UI 패널이 활성화되거나 비활성화될 때 이벤트를 발생시킵니다.
/// SOLID: 단일 책임 원칙 (UI 상태 이벤트 발생)
/// </summary>
public class UIEventHandler : MonoBehaviour
{
    /// <summary>
    /// UI 패널이 활성화되었을 때 호출되는 정적 이벤트입니다.
    /// 활성화된 패널의 GameObject를 인자로 전달합니다.
    /// </summary>
    public static event Action<GameObject> OnPanelActivated;

    /// <summary>
    /// UI 패널이 비활성화되었을 때 호출되는 정적 이벤트입니다.
    /// 비활성화된 패널의 GameObject를 인자로 전달합니다.
    /// </summary>
    public static event Action<GameObject> OnPanelDeactivated;

    /// <summary>
    /// 이 컴포넌트가 활성화될 때(UI 패널이 켜질 때) 호출됩니다.
    /// </summary>
    private void OnEnable()
    {
        OnPanelActivated?.Invoke(this.gameObject);
    }

    /// <summary>
    /// 이 컴포넌트가 비활성화될 때(UI 패널이 꺼질 때) 호출됩니다.
    /// </summary>
    private void OnDisable()
    {
        OnPanelDeactivated?.Invoke(this.gameObject);
    }
}