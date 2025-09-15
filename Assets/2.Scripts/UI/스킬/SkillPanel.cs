using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �� ��ũ��Ʈ�� ��ų �г� UI�� ��ȣ�ۿ��� �����մϴ�.
// ��ư Ŭ���� ���� �г��� Ȱ��ȭ/��Ȱ��ȭ�ϰ�, ��ų ����Ʈ �ý��۰� �����ϴ� ����� ����մϴ�.
public class SkillPanel : MonoBehaviour
{
    [Header("UI ��� ����")]
    [Tooltip("��ų �г�1 (��: ��Ƽ�� ��ų) GameObject�� �Ҵ��ϼ���.")]
    public GameObject panel1;
    [Tooltip("��ų �г�2 (��: �нú� ��ų) GameObject�� �Ҵ��ϼ���.")]
    public GameObject panel2;

    [Tooltip("�г�1�� Ȱ��ȭ�ϴ� ��ư�Դϴ�.")]
    public Button button1;
    [Tooltip("�г�2�� Ȱ��ȭ�ϴ� ��ư�Դϴ�.")]
    public Button button2;

    [Header("��ų ����Ʈ �ý���")]
    // SkillPointManager�� ���� �̱������� �����ϹǷ� ������ �ʿ� �����ϴ�.
    // [Tooltip("SkillPointManager ��ũ��Ʈ�� �Ҵ��ϼ���. �ν����Ϳ��� ���� �����ϼ���.")]
    // public SkillPointManager skillPointManager;
    [Tooltip("���� ��ų ����Ʈ�� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI skillPointText;

    [Tooltip("���� ������ �����ϴ� ��ư�� �Ҵ��ϼ���.")]
    public Button saveButton;
    [Tooltip("���� ������ ����ϴ� ��ư�� �Ҵ��ϼ���.")]
    public Button cancelButton;

    // ��ų �г��� Ȱ��ȭ ���θ� �����ϴ� ����
    private bool isPanelActive = false;

    void Awake()
    {
        // SkillPointManager �̱��� �ν��Ͻ��� �����ϴ��� Ȯ���ϰ� �̺�Ʈ ������ �����մϴ�.
        if (SkillPointManager.Instance != null)
        {
            // ��ų ����Ʈ�� ����� ������ UI�� ������Ʈ�ϴ� �̺�Ʈ�� �޼��带 ����մϴ�.
            SkillPointManager.Instance.OnSkillPointsChanged += UpdateSkillPointUI;
        }
        else
        {
            // �ν��Ͻ��� �������� ���� ��� ��� �޽����� ���ϴ�.
            Debug.LogError("SkillPointManager �ν��Ͻ��� �������� �ʽ��ϴ�. ���� �ش� ������Ʈ�� �ִ��� Ȯ���� �ּ���.");
        }

        // ���� �� ��� ��ư�� Ŭ�� �̺�Ʈ�� �����մϴ�.
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClick);
        }
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }
    }

    /// <summary>
    /// �� �޼���� ��ų �г��� ��ü�� Ȱ��ȭ/��Ȱ��ȭ�� �� ȣ��˴ϴ�.
    /// �г��� Ȱ��ȭ�� �� SkillPointManager�� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="active">�г� Ȱ��ȭ ����</param>
    public void SetPanelActive(bool active)
    {
        // �г� Ȱ��ȭ ���¸� ������Ʈ�մϴ�.
        isPanelActive = active;
        gameObject.SetActive(active);

        // �г��� Ȱ��ȭ�� ���� SkillPointManager�� �ʱ�ȭ�մϴ�.
        // �̷��� �ϸ� �Ź� �г��� �� ������ �ӽ� �����Ͱ� �ʱ� ���·� ���ư��ϴ�.
        if (active && SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.InitializePoints();
        }
    }

    /// <summary>
    /// ù ��° ��ų �гθ� Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ShowPanel1()
    {
        // ù ��° �г��� Ȱ��ȭ�մϴ�.
        if (panel1 != null)
        {
            panel1.SetActive(true);
        }

        // ������ �гε��� ��Ȱ��ȭ�մϴ�.
        if (panel2 != null)
        {
            panel2.SetActive(false);
        }
    }

    /// <summary>
    /// �� ��° ��ų �гθ� Ȱ��ȭ�ϰ� �������� ��Ȱ��ȭ�մϴ�.
    /// </summary>
    public void ShowPanel2()
    {
        // �� ��° �г��� Ȱ��ȭ�մϴ�.
        if (panel2 != null)
        {
            panel2.SetActive(true);
        }

        // ������ �гε��� ��Ȱ��ȭ�մϴ�.
        if (panel1 != null)
        {
            panel1.SetActive(false);
        }
    }

    /// <summary>
    /// SkillPointManager���� ȣ���ϴ� �̺�Ʈ�� ��ϵǾ� ��ų ����Ʈ�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="currentPoints">���� ��ų ����Ʈ</param>
    private void UpdateSkillPointUI(int currentPoints)
    {
        if (skillPointText != null)
        {
            skillPointText.text = $"��ų����Ʈ: \n{currentPoints}";
        }
    }

    /// <summary>
    /// '����' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// SkillPointManager.Instance�� ApplyChanges �޼��带 ȣ���Ͽ� ���� ������ ���� �����մϴ�.
    /// </summary>
    private void OnSaveButtonClick()
    {
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.ApplyChanges();
        }
    }

    /// <summary>
    /// '���' ��ư Ŭ�� �� ȣ��˴ϴ�.
    /// SkillPointManager.Instance�� DiscardChanges �޼��带 ȣ���Ͽ� ���� ������ �ǵ����ϴ�.
    /// </summary>
    private void OnCancelButtonClick()
    {
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.DiscardChanges();
        }
    }

    private void OnDestroy()
    {
        // ��ũ��Ʈ�� �ı��� �� �̺�Ʈ ������ �����Ͽ� �޸� ������ �����մϴ�.
        if (SkillPointManager.Instance != null)
        {
            SkillPointManager.Instance.OnSkillPointsChanged -= UpdateSkillPointUI;
        }
    }
}