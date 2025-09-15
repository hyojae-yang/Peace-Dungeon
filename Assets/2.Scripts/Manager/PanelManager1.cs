using UnityEngine;

// UI 패널들을 관리하고, 키 입력 및 버튼 클릭에 따라 패널을 제어하는 스크립트입니다.
public class PanelManager1 : MonoBehaviour
{
    // === 패널 할당 변수 ===
    [Tooltip("관리할 패널들을 순서대로 할당하세요.")]
    public GameObject[] panels;

    /// <summary>
    /// 지정된 인덱스의 패널만 활성화하고 나머지 패널은 비활성화합니다.
    /// 이 메서드는 UI 버튼의 OnClick() 이벤트에 연결하여 사용합니다.
    /// </summary>
    /// <param name="panelIndex">활성화할 패널의 배열 인덱스</param>
    public void ActivatePanel(int panelIndex)
    {
        // 배열 범위 유효성 검사
        if (panelIndex < 0 || panelIndex >= panels.Length)
        {
            Debug.LogError("잘못된 패널 인덱스입니다: " + panelIndex);
            return;
        }

        // 모든 패널을 순회하며 활성화 상태를 조정합니다.
        for (int i = 1; i < panels.Length; i++)
        {
            // 선택된 인덱스의 패널만 활성화합니다.
            if (i == panelIndex)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }
        }
    }
}