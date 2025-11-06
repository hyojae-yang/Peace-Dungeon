using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameGuidePanel : MonoBehaviour
{
    // ★★★ 유니티 에디터에서 직접 할당할 변수들 ★★★
    [Header("Page References")]
    [Tooltip("순서대로 하위 설명 패널(페이지)들을 할당해주세요.")]
    [SerializeField]
    private List<GameObject> guidePages; // 설명 패널(페이지) 리스트

    [Header("UI Controls")]
    [Tooltip("페이지 번호를 표시할 텍스트 컴포넌트")]
    [SerializeField]
    private TextMeshProUGUI pageText; // 현재 페이지 / 전체 페이지 텍스트 (예: 1/7)

    [Tooltip("이전 페이지 버튼")]
    [SerializeField]
    private Button previousButton; // 왼쪽 버튼

    [Tooltip("다음 페이지 버튼")]
    [SerializeField]
    private Button nextButton; // 오른쪽 버튼

    // ★★★ 내부 로직 변수 ★★★
    private int currentPageIndex = 0; // 현재 활성화된 패널의 인덱스

    void Awake()
    {
        // 버튼에 메서드 연결 (코드에서 연결하는 것이 안정적입니다)
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(PreviousPage);
        }
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextPage);
        }
    }

    void OnEnable()
    {
        // 이 패널이 활성화될 때마다 첫 페이지로 초기화합니다.
        currentPageIndex = 0;
        ShowCurrentPage();
    }

    void OnDisable()
    {
        // 패널이 닫힐 때 게임 시간을 다시 정상화할 필요가 있다면 여기에 추가할 수 있습니다.
        // 예: Time.timeScale = 1f;
    }

    private void ShowCurrentPage()
    {
        if (guidePages == null || guidePages.Count == 0)
        {
            Debug.LogError("설명 페이지(guidePages) 리스트가 비어있습니다. 페이지를 할당해주세요!");
            return;
        }

        // 1. 모든 페이지 비활성화
        for (int i = 0; i < guidePages.Count; i++)
        {
            if (guidePages[i] != null)
            {
                guidePages[i].SetActive(false);
            }
        }

        // 2. 현재 인덱스에 해당하는 페이지 활성화
        if (currentPageIndex >= 0 && currentPageIndex < guidePages.Count)
        {
            guidePages[currentPageIndex].SetActive(true);

            // 3. UI 텍스트 및 버튼 상태 업데이트
            UpdatePageText();
            UpdateButtonStates();
        }
        else
        {
            Debug.LogError("잘못된 페이지 인덱스입니다: " + currentPageIndex);
            // 인덱스가 범위를 벗어나면 첫 페이지로 강제 이동 (예외 처리)
            currentPageIndex = 0;
            ShowCurrentPage();
        }
    }

    private void UpdatePageText()
    {
        if (pageText != null)
        {
            // 고객님 요청 방식 (현재 인덱스 + 1 / 전체 갯수)
            pageText.text = $"{currentPageIndex + 1}/{guidePages.Count}";
        }
    }

    private void UpdateButtonStates()
    {
        // 왼쪽 버튼 (이전) 활성화/비활성화 결정: 첫 페이지(인덱스 0)일 때 비활성화
        if (previousButton != null)
        {
            previousButton.interactable = currentPageIndex > 0;
        }

        // 오른쪽 버튼 (다음) 활성화/비활성화 결정: 마지막 페이지일 때 비활성화
        if (nextButton != null)
        {
            nextButton.interactable = currentPageIndex < guidePages.Count - 1;
        }
    }

    // 다음 페이지로 이동하는 메서드 (오른쪽 버튼 클릭 시 호출)
    public void NextPage()
    {
        // 마지막 페이지가 아닌 경우에만 이동
        if (currentPageIndex < guidePages.Count - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();
        }
    }

    // 이전 페이지로 이동하는 메서드 (왼쪽 버튼 클릭 시 호출)
    public void PreviousPage()
    {
        // 첫 페이지가 아닌 경우에만 이동
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowCurrentPage();
        }
    }
}