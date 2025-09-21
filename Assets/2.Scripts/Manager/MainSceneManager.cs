using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ���� �ֿ� UI �гε��� �߾ӿ��� �����ϴ� �Ŵ��� Ŭ�����Դϴ�.
/// Ư�� �˾� �г��� Ȱ��ȭ�Ǹ� PlayerCanvas�� ��Ȱ��ȭ�ϰ�,
/// ��� �˾� �г��� ��Ȱ��ȭ�Ǹ� PlayerCanvas�� �ٽ� Ȱ��ȭ�մϴ�.
/// SOLID: ����-��� ��Ģ (���ο� �˾� �г� �߰� �� �� ��ũ��Ʈ�� �ڵ� ���� �ʿ� ����)
/// </summary>
public class MainSceneManager : MonoBehaviour
{
    // MainSceneManager�� �̱��� �ν��Ͻ�
    public static MainSceneManager Instance { get; private set; }

    [Header("UI Group References")]
    [Tooltip("���� �÷��� �� �׻� Ȱ��ȭ�Ǿ�� �ϴ� ���� UI ĵ�����Դϴ�.")]
    [SerializeField]
    private GameObject playerCanvas;

    [Tooltip("Ư�� �̺�Ʈ�� ���� Ȱ��ȭ�Ǿ� PlayerCanvas�� ���� �˾� �гε��Դϴ�.")]
    [SerializeField]
    private List<GameObject> popUpPanels = new List<GameObject>();

    [Tooltip("���� ĵ������ ���� �Ҵ��մϴ�. ���� ���¸� �����ϴ� �� ���˴ϴ�.")]
    [SerializeField]
    private GameObject dungeonCanvas;

    [Header("UI ���� ���� ����")]
    [Tooltip("���� ĵ������ ���� Ȱ��ȭ�Ǿ� �ִ��� ���θ� ��Ÿ���ϴ�.")]
    public bool isDungeonCanvasActive = false;

    /// <summary>
    /// ��ũ��Ʈ �ν��Ͻ��� �ε�� �� ȣ��Ǿ� �̱����� �����ϰ� �̺�Ʈ �����ʸ� ����մϴ�.
    /// </summary>
    private void Awake()
    {
        // 1. �̱��� �ν��Ͻ� �ʱ�ȭ
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("���� �̹� �ٸ� MainSceneManager �ν��Ͻ��� �����մϴ�. ���ο� �ν��Ͻ��� �ı��մϴ�.");
            Destroy(gameObject);
        }

        // 2. UIEventHandler�� �� �̺�Ʈ�� ��� ����
        UIEventHandler.OnPanelActivated += HandlePanelActivation;
        UIEventHandler.OnPanelDeactivated += HandlePanelDeactivation;
    }

    /// <summary>
    /// �̺�Ʈ�� ���� �г� Ȱ��ȭ ��ȣ�� ������ ȣ��Ǵ� �޼����Դϴ�.
    /// Ȱ��ȭ�� �г��� �˾� �г��̸� PlayerCanvas�� ��Ȱ��ȭ�ϰ�, ���� ĵ������� ���� ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="activatedPanel">Ȱ��ȭ�� �г��� ���� ������Ʈ�Դϴ�.</param>
    private void HandlePanelActivation(GameObject activatedPanel)
    {
        // Ȱ��ȭ�� �г��� �˾� �г� ����Ʈ�� ���ԵǾ� �ִ��� Ȯ���մϴ�.
        if (popUpPanels.Contains(activatedPanel))
        {
            // PlayerCanvas�� �̹� ��Ȱ��ȭ ���°� �ƴ� ��쿡�� ��Ȱ��ȭ�մϴ�.
            if (playerCanvas.activeInHierarchy)
            {
                playerCanvas.SetActive(false);
            }
        }

        // Ȱ��ȭ�� �г��� �Ҵ�� ���� ĵ�������� Ȯ���ϰ� ������ ������Ʈ�մϴ�.
        if (activatedPanel == dungeonCanvas)
        {
            isDungeonCanvasActive = true;
        }
    }

    /// <summary>
    /// �̺�Ʈ�� ���� �г� ��Ȱ��ȭ ��ȣ�� ������ ȣ��Ǵ� �޼����Դϴ�.
    /// ��� �˾� �г��� ������ ���� PlayerCanvas�� �ٽ� Ȱ��ȭ�ϰ�, ���� ĵ������� ���� ������ ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="deactivatedPanel">��Ȱ��ȭ�� �г��� ���� ������Ʈ�Դϴ�.</param>
    private void HandlePanelDeactivation(GameObject deactivatedPanel)
    {
        // ��Ȱ��ȭ�� �г��� �˾� �г� ����Ʈ�� ���ԵǾ� �ִ��� Ȯ���մϴ�.
        if (popUpPanels.Contains(deactivatedPanel))
        {
            // LINQ�� ����Ͽ� ���� Ȱ��ȭ�� �˾� �г��� �ִ��� Ȯ���մϴ�.
            bool anyPopUpPanelIsActive = popUpPanels.Any(panel => panel.activeInHierarchy);

            // Ȱ��ȭ�� �˾� �г��� �� �̻� ���� ��쿡�� PlayerCanvas�� Ȱ��ȭ�մϴ�.
            if (!anyPopUpPanelIsActive)
            {
                playerCanvas.SetActive(true);
            }
        }

        // ��Ȱ��ȭ�� �г��� �Ҵ�� ���� ĵ�������� Ȯ���ϰ� ������ ������Ʈ�մϴ�.
        if (deactivatedPanel == dungeonCanvas)
        {
            isDungeonCanvasActive = false;
        }
    }

    /// <summary>
    /// ���� ������Ʈ�� �ı��� �� ȣ��Ǿ� �̺�Ʈ �����ʸ� �����մϴ�.
    /// �޸� ������ �����ϱ� ���� �ʼ� �۾��Դϴ�.
    /// </summary>
    private void OnDestroy()
    {
        UIEventHandler.OnPanelActivated -= HandlePanelActivation;
        UIEventHandler.OnPanelDeactivated -= HandlePanelDeactivation;
    }
}