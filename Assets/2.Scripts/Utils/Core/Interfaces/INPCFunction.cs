using UnityEngine;

/// <summary>
/// NPC의 특수 기능을 정의하는 인터페이스.
/// 이 인터페이스를 구현하는 모든 클래스는 NPC와의 상호작용 시
/// UI 버튼을 생성하고 특정 기능을 실행하는 데 사용됩니다.
/// </summary>
public interface INPCFunction
{
    /// <summary>
    /// UI 버튼에 표시될 이름을 반환합니다.
    /// 이 프로퍼티는 해당 기능의 이름을 나타냅니다 (예: "상점", "대장간").
    /// </summary>
    string FunctionButtonName { get; }

    /// <summary>
    /// UI 버튼이 클릭되었을 때 호출될 함수입니다.
    /// 이 메서드는 해당 기능의 핵심 로직(예: 상점 UI 열기)을 실행합니다.
    /// </summary>
    void ExecuteFunction();
}