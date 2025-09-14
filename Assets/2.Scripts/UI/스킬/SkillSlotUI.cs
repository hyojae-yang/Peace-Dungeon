using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro�� ����Ѵٸ� �ʿ��մϴ�.

// �� ��ũ��Ʈ�� ��Ƽ�� ��ų ���� �гο� �ִ� ���� ������ UI�� ����մϴ�.
// ��ų ���/���� �� �̹����� �ؽ�Ʈ�� ������Ʈ�ϸ�, ���� ��Ÿ�� �����̴� ���� �����մϴ�.
public class SkillSlotUI : MonoBehaviour
{
    // === UI ������Ʈ ===
    [Header("UI ������Ʈ")]
    [Tooltip("��ų �̹����� ǥ���� Image ������Ʈ�� �Ҵ��ϼ���.")]
    public Image skillImage;
    [Tooltip("��ų�� ��ϵ��� �ʾ��� �� ǥ���� �⺻ ���� ��������Ʈ�� �Ҵ��ϼ���.")]
    public Sprite defaultSlotSprite; // <-- �⺻ ��������Ʈ ���� �߰�
    [Tooltip("��ų�� ���� �Ҹ��� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI manaCostText;
    [Tooltip("��ų�� ���� ��Ÿ���� ǥ���� TextMeshProUGUI ������Ʈ�� �Ҵ��ϼ���.")]
    public TextMeshProUGUI cooldownText; // <-- ��Ÿ�� �ؽ�Ʈ ���� �߰�
    [Tooltip("��Ÿ�� ���� ��Ȳ�� �ð������� ǥ���� �����̴� ������Ʈ�� �Ҵ��ϼ���.")]
    public Slider cooldownSlider; // <-- ��Ÿ�� �����̴� �߰�

    // �� ��ũ��Ʈ�� ������ ��ü�� �������� �ʰ�, ���� �����ͷ� UI�� ������Ʈ�մϴ�.
    private SkillData currentSkillData;

    /// <summary>
    /// �ܺ�(SlotSelectionPanel)���� ȣ��Ǿ� ������ UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="data">���Կ� ����� ��ų ������. ���� �ÿ��� null�� �����մϴ�.</param>
    /// <param name="manaCost">ǥ���� ��ų�� ���� �Ҹ�.</param>
    public void UpdateUI(SkillData data, float manaCost)
    {
        currentSkillData = data;

        if (currentSkillData != null)
        {
            skillImage.enabled = true;
            skillImage.sprite = currentSkillData.skillImage;

            if (manaCostText != null)
            {
                manaCostText.text = manaCost.ToString();
            }

            // ��ų�� ��ϵǸ� �����̴��� �ʱ�ȭ�մϴ�.
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
        else
        {
            skillImage.enabled = true;
            skillImage.sprite = defaultSlotSprite;

            if (manaCostText != null)
            {
                manaCostText.text = string.Empty;
            }

            // ��ų�� �����Ǹ� ��Ÿ�� �ؽ�Ʈ�� �����̴��� ���ϴ�.
            if (cooldownText != null)
            {
                cooldownText.text = string.Empty;
            }
            if (cooldownSlider != null)
            {
                cooldownSlider.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// ���� ��Ÿ�� ���� �޾� UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="remainingCooldown">���� ��Ÿ�� �ð� (��)</param>
    /// <param name="maxCooldown">��ų�� �ִ� ��Ÿ�� �ð� (��)</param>
    public void UpdateCooldownUI(float remainingCooldown, float maxCooldown)
    {
        // ��Ÿ�� �ؽ�Ʈ�� �����̴��� ��� �Ҵ�Ǿ����� Ȯ���մϴ�.
        if (cooldownText == null || cooldownSlider == null) return;

        // ��Ÿ���� ���Ҵٸ� �ؽ�Ʈ�� �����̴��� ǥ���ϰ�, �ƴϸ� ���ϴ�.
        if (remainingCooldown > 0f)
        {
            cooldownText.text = remainingCooldown.ToString("F1"); // �Ҽ��� ù° �ڸ����� ǥ��
            cooldownSlider.gameObject.SetActive(true);

            // �����̴��� ���� ������Ʈ�մϴ�.
            cooldownSlider.maxValue = maxCooldown;
            cooldownSlider.value = remainingCooldown;
        }
        else
        {
            cooldownText.text = string.Empty;
            cooldownSlider.gameObject.SetActive(false);
        }
    }
}