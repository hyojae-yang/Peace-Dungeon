using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 상태에 따라 마우스 커서의 모양을 전역적으로 관리하는 싱글턴 스크립트입니다.
/// </summary>
public class CursorManager : MonoBehaviour
{
    // === 싱글턴 인스턴스 ===
    public static CursorManager Instance { get; private set; }

    // === 커서 상태 열거형 정의 ===
    public enum CursorState
    {
        Normal,  // 평상시 (기본 상태)
        Combat,  // 전투 시 (타겟팅, 칼 모양 등)
        Shop,    // 상점 이용 시 (돈, 구매 모양 등)
        Cooking, // 요리 및 제작 시 (손, 도구 모양 등)
        Clickable // UI 버튼 등 클릭 가능 영역에 마우스를 올렸을 때
    }

    // === 필드 설정 ===
    [Header("커서 이미지 설정")]
    [Tooltip("각 상태에 맞는 Texture2D를 할당해 주세요.")]
    public Texture2D normalCursor;
    public Texture2D combatCursor;
    public Texture2D shopCursor;
    public Texture2D cookingCursor;
    public Texture2D clickableCursor; // UI 진입/탈출 시 사용

    [Tooltip("커서 이미지의 핫스팟 (클릭 지점)")]
    public Vector2 hotSpot = Vector2.zero;

    // 현재 커서 상태 저장 변수
    private CursorState _currentState = CursorState.Normal; // 현재 게임의 전역 상태 저장
    private Dictionary<CursorState, Texture2D> _cursorMap; // 룩업 테이블

    private void Awake()
    {
        // 1. 싱글턴 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않도록 설정 (선택 사항)
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. 룩업 테이블 초기화 (O(1) 검색을 위한 SOLID 원칙 준수)
        InitializeCursorMap();
    }

    private void Start()
    {
        // 게임 시작 시 기본 커서로 설정
        SetCursorState(CursorState.Normal);
    }

    /// <summary>
    /// Awake에서 커서 상태와 텍스처를 연결하여 룩업 테이블을 생성합니다.
    /// </summary>
    private void InitializeCursorMap()
    {
        _cursorMap = new Dictionary<CursorState, Texture2D>
        {
            { CursorState.Normal, normalCursor },
            { CursorState.Combat, combatCursor },
            { CursorState.Shop, shopCursor },
            { CursorState.Cooking, cookingCursor },
            { CursorState.Clickable, clickableCursor }
        };
    }

    /// <summary>
    /// 게임의 전역 상태에 따라 마우스 커서 모양을 변경합니다.
    /// 이 메서드는 다른 스크립트에서 커서 상태를 변경할 때 사용됩니다.
    /// </summary>
    /// <param name="newState">변경할 커서 상태입니다.</param>
    public void SetCursorState(CursorState newState)
    {
        // 같은 상태면 중복 호출 방지 (UI 복귀 로직에서는 이 검사를 사용하지 않음)
        if (_currentState == newState) return;

        // 전역 상태를 업데이트합니다.
        _currentState = newState;

        if (_cursorMap.TryGetValue(newState, out Texture2D targetTexture))
        {
            // 원하는 텍스처로 커서 설정
            Cursor.SetCursor(targetTexture, hotSpot, CursorMode.Auto);
            Debug.Log($"[CursorManager] 커서 전역 상태 변경: {newState.ToString()}");
        }
        else
        {
            Debug.LogError($"'{newState.ToString()}' 상태에 할당된 커서 텍스처를 찾을 수 없습니다! 기본 커서로 복귀합니다.");
            // 오류 발생 시 Normal 상태로 복귀 시도
            SetCursorState(CursorState.Normal);
        }
    }

    /// <summary>
    /// 현재 설정된 전역 상태를 무시하고, UI 상호작용을 위해 임시로 'Clickable' 커서를 설정합니다.
    /// (UI Event Trigger의 PointerEnter에 연결)
    /// </summary>
    public void SetClickableCursor()
    {
        // UI 커서를 위한 별도의 Texture2D를 룩업 테이블에서 가져와 직접 설정합니다.
        if (_cursorMap.TryGetValue(CursorState.Clickable, out Texture2D clickableTexture))
        {
            // SetCursorState를 통하지 않고 직접 커서를 설정하여 _currentState를 변경하지 않습니다.
            Cursor.SetCursor(clickableTexture, hotSpot, CursorMode.Auto);
            Debug.Log("[CursorManager] 임시 커서 변경: Clickable");
        }
    }

    /// <summary>
    /// 임시로 설정했던 'Clickable' 커서를 해제하고, 원래의 전역 상태 커서로 복귀합니다.
    /// (UI Event Trigger의 PointerExit에 연결)
    /// </summary>
    public void RevertCursorState()
    {
        // 핵심 수정: SetCursorState의 중복 검사 로직을 우회하고, 
        // 저장된 _currentState의 텍스처를 가져와 커서를 강제 재설정합니다.
        if (_cursorMap.TryGetValue(_currentState, out Texture2D targetTexture))
        {
            Cursor.SetCursor(targetTexture, hotSpot, CursorMode.Auto);
            Debug.Log($"[CursorManager] 커서 복귀: {_currentState.ToString()}");
        }
        else
        {
            // 예외 상황 대비 (텍스처가 할당되지 않은 경우)
            Debug.LogError($"현재 전역 상태({_currentState.ToString()})의 텍스처를 찾을 수 없어 복귀 실패.");
        }
    }
}