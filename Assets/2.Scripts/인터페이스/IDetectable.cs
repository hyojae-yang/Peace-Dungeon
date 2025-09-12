using UnityEngine;

// 몬스터에게 감지될 수 있는 모든 대상이 구현해야 하는 인터페이스입니다.
public interface IDetectable
{
    // 감지 가능한지 여부를 반환하는 메서드입니다.
    bool IsDetectable();

    // 이 메서드는 몬스터가 추적할 대상의 위치를 얻기 위해 사용됩니다.
    Transform GetTransform();
}