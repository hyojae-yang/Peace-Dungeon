using UnityEngine;
using TMPro;

// 이 스크립트는 스킬 툴팁 UI 패널의 내용을 관리하고,
// 아이콘의 위치에 따라 툴팁의 위치를 동적으로 조정합니다.
public class SkillTooltip : MonoBehaviour
{
    // === UI 컴포넌트 ===
    [Header("UI 컴포넌트")]
    [Tooltip("툴팁 패널의 RectTransform")]
    public RectTransform tooltipRectTransform;
    [Tooltip("스킬 이름을 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillNameText;
    [Tooltip("스킬 설명을 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillDescriptionText;
    [Tooltip("스킬 레벨을 표시할 Text 컴포넌트")]
    public TextMeshProUGUI skillLevelText;

    [Header("위치 조정 설정")]
    [Tooltip("툴팁이 아이콘에서 얼마나 떨어질지 설정하는 오프셋입니다.")]
    public float offset = 10f;

    void Awake()
    {
        // RectTransform이 할당되지 않았다면, 이 게임오브젝트의 RectTransform을 가져옵니다.
        if (tooltipRectTransform == null)
        {
            tooltipRectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// 외부에서 스킬 데이터를 받아와 툴팁을 업데이트합니다.
    /// </summary>
    /// <param name="data">이 아이콘이 대표할 스킬의 SkillData</param>
    /// <param name="currentLevel">현재 스킬의 레벨</param>
    public void SetTooltipData(SkillData data, int currentLevel)
    {
        if (data != null)
        {
            skillNameText.text = data.skillName;
            skillDescriptionText.text = data.skillDescription;
            skillLevelText.text = $"Lv. {currentLevel}"; // currentLevel 값을 직접 사용
        }
        else
        {
            Debug.LogWarning("할당된 스킬 데이터가 없습니다. 툴팁 업데이트 실패.");
        }
    }

    /// <summary>
    /// 스킬 아이콘의 위치를 기준으로 툴팁의 위치를 조정합니다.
    /// </summary>
    /// <param name="iconPosition">스킬 아이콘의 화면상 위치 (월드 좌표)</param>
    public void AdjustPosition(Vector3 iconPosition)
    {
        // 아이콘의 화면 좌표를 구합니다.
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(iconPosition);

        // 툴팁의 RectTransform을 조정합니다.
        // 스킬 아이콘이 화면의 왼쪽에 있으면 툴팁을 오른쪽에,
        // 오른쪽에 있으면 툴팁을 왼쪽에 나타나게 합니다.
        if (screenPoint.x < Screen.width / 2)
        {
            // 아이콘이 왼쪽에 있을 경우: 툴팁을 오른쪽에 표시
            tooltipRectTransform.pivot = new Vector2(0, 0.5f);
            tooltipRectTransform.anchoredPosition = new Vector2(offset, 0);
        }
        else
        {
            // 아이콘이 오른쪽에 있을 경우: 툴팁을 왼쪽에 표시
            tooltipRectTransform.pivot = new Vector2(1, 0.5f);
            tooltipRectTransform.anchoredPosition = new Vector2(-offset, 0);
        }
    }
}