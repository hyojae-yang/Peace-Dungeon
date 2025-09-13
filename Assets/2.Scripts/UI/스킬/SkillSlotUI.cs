using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro�� ����Ѵٸ� �ʿ��մϴ�.

// �� ��ũ��Ʈ�� ��Ƽ�� ��ų ���� �гο� �ִ� ���� ������ UI�� ����մϴ�.
// ��ų ���/���� �� �̹����� �ؽ�Ʈ�� ������Ʈ�ϸ�, ���� ��Ÿ�� �����̴� ���� �����մϴ�.
public class SkillSlotUI : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    [Tooltip("��ų �̹����� ǥ���� Image ������Ʈ�� �Ҵ��ϼ���.")]
    public Image skillImage;
    [Tooltip("��ų�� ��ϵ��� �ʾ��� �� ǥ���� �⺻ ���� ��������Ʈ�� �Ҵ��ϼ���.")]
    public Sprite defaultSlotSprite; // <-- �⺻ ��������Ʈ ���� �߰�

    // �� ��ũ��Ʈ�� ������ ��ü�� �������� �ʰ�, ���� �����ͷ� UI�� ������Ʈ�մϴ�.
    private SkillData currentSkillData;

    /// <summary>
    /// �ܺ�(SlotSelectionPanel)���� ȣ��Ǿ� ������ UI�� ������Ʈ�մϴ�.
    /// </summary>
    /// <param name="data">���Կ� ����� ��ų ������. ���� �ÿ��� null�� �����մϴ�.</param>
    public void UpdateUI(SkillData data)
    {
        // ���� ���Կ� ��ϵ� ��ų �����͸� ������Ʈ�մϴ�.
        currentSkillData = data;

        // �����Ͱ� ��ȿ���� Ȯ���ϰ� UI�� �����մϴ�.
        if (currentSkillData != null)
        {
            // ��ų�� ��ϵ� ���: �̹����� Ȱ��ȭ�ϰ�, ��������Ʈ�� �����մϴ�.
            skillImage.enabled = true;
            skillImage.sprite = currentSkillData.skillImage;
        }
        else
        {
            // ��ų�� ������ ��� (data�� null):
            // �̹����� Ȱ��ȭ ���·� �����ϵ�, �⺻ ��������Ʈ�� �����մϴ�.
            skillImage.enabled = true; // �̹����� ���̵��� ����
            skillImage.sprite = defaultSlotSprite; // <-- null ��� �⺻ ��������Ʈ �Ҵ�

            // ���� �⺻ ��������Ʈ�� ���ٸ� �̹����� ��Ȱ��ȭ�� �� �ֽ��ϴ�.
            // if (defaultSlotSprite == null)
            // {
            //     skillImage.enabled = false;
            // }
        }
    }
}