using System;

/// <summary>
/// 이 클래스는 저장될 데이터와 그 데이터의 '타입 이름'을 함께 담는 단순한 그릇(컨테이너)입니다.
/// 이 클래스가 있어야 SaveManager가 여러 데이터들을 서로 헷갈리지 않고 올바른 스크립트로 전달할 수 있습니다.
/// </summary>
[Serializable]
public class SaveDataContainer
{
    // '이름표' 역할을 하는 변수입니다. 여기에 데이터의 원래 타입 이름이 저장됩니다.
    public string typeName;

    // '내용물' 역할을 하는 변수입니다. 여기에 실제 저장될 데이터가 들어갑니다.
    public object data;
}