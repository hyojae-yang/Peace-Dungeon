using UnityEngine;
using System;

/// <summary>
/// �� ��ũ��Ʈ�� ������ UI �г��� Ȱ��ȭ�ǰų� ��Ȱ��ȭ�� �� �̺�Ʈ�� �߻���ŵ�ϴ�.
/// SOLID: ���� å�� ��Ģ (UI ���� �̺�Ʈ �߻�)
/// </summary>
public class UIEventHandler : MonoBehaviour
{
    /// <summary>
    /// UI �г��� Ȱ��ȭ�Ǿ��� �� ȣ��Ǵ� ���� �̺�Ʈ�Դϴ�.
    /// Ȱ��ȭ�� �г��� GameObject�� ���ڷ� �����մϴ�.
    /// </summary>
    public static event Action<GameObject> OnPanelActivated;

    /// <summary>
    /// UI �г��� ��Ȱ��ȭ�Ǿ��� �� ȣ��Ǵ� ���� �̺�Ʈ�Դϴ�.
    /// ��Ȱ��ȭ�� �г��� GameObject�� ���ڷ� �����մϴ�.
    /// </summary>
    public static event Action<GameObject> OnPanelDeactivated;

    /// <summary>
    /// �� ������Ʈ�� Ȱ��ȭ�� ��(UI �г��� ���� ��) ȣ��˴ϴ�.
    /// </summary>
    private void OnEnable()
    {
        OnPanelActivated?.Invoke(this.gameObject);
    }

    /// <summary>
    /// �� ������Ʈ�� ��Ȱ��ȭ�� ��(UI �г��� ���� ��) ȣ��˴ϴ�.
    /// </summary>
    private void OnDisable()
    {
        OnPanelDeactivated?.Invoke(this.gameObject);
    }
}