// ���ϸ�: RecipeItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// �丮 UI�� ���� ������ �������� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// </summary>
public class RecipeItemUI : MonoBehaviour
{
    // === UI ���� ===
    [Header("UI References")]
    [Tooltip("�ϼ� ������ �̹����� ǥ���ϴ� UI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private Image resultImage;
    [Tooltip("�ϼ� �������� �̸��� ǥ���ϴ� UI ������Ʈ�Դϴ�.")]
    [SerializeField]
    private TextMeshProUGUI resultText;

    // ��� �ؽ�Ʈ ������Ʈ���� ����Ʈ�� �����Ͽ� �������� ���Դϴ�.
    [Tooltip("��� �������� �̸��� ǥ���ϴ� UI ������Ʈ���Դϴ�.")]
    [SerializeField]
    private List<TextMeshProUGUI> ingredientTexts = new List<TextMeshProUGUI>(4);

    //private Button craftButton; // �� ���� �����մϴ�.

    private const string EMPTY_SLOT_EMOJI = "-"; // �� ���Կ� ǥ���� �̸�Ƽ��

    // === ������ �Ҵ� �޼��� ===
    /// <summary>
    /// RecipeSO �����͸� �޾ƿͼ� UI�� ������Ʈ�ϴ� �޼����Դϴ�.
    /// </summary>
    /// <param name="recipeSO">ǥ���� ������ �������Դϴ�.</param>
    public void SetData(RecipeSO recipeSO)
    {
        // 1. �ϼ� ������ ���� ����
        if (recipeSO.resultItem != null)
        {
            resultImage.sprite = recipeSO.resultItem.itemIcon;
            resultText.text = recipeSO.resultItem.itemName;
        }

        // 2. ��� ������ ���� ����
        // ��� �ؽ�Ʈ ������Ʈ���� ��ȸ�ϸ� ������ �Ҵ�
        for (int i = 0; i < ingredientTexts.Count; i++)
        {
            // ������ ��ᰡ �����ϴ��� Ȯ��
            if (i < recipeSO.ingredients.Count)
            {
                var ingredient = recipeSO.ingredients[i];
                ingredientTexts[i].text = $"{ingredient.item.itemName} x{ingredient.quantity}";
            }
            else
            {
                // ��ᰡ ���� ������ �̸�Ƽ������ ä��ϴ�.
                ingredientTexts[i].text = EMPTY_SLOT_EMOJI;
            }
        }
    }
}