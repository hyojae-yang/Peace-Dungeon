// RootHitbox.cs 스크립트 전문

using UnityEngine;

/// <summary>
/// Root Visual 오브젝트에 부착되어 실제 공격 판정(Hitbox) 및 피해 처리를 담당합니다.
/// ForestBoss.cs에서 콜라이더 활성화/비활성화 및 피해량 설정을 제어합니다. (단일 책임 원칙 준수)
/// </summary>
[RequireComponent(typeof(Collider))] // 콜라이더 필수 요구
public class RootHitbox : MonoBehaviour
{
    // === 피해 설정 변수 ===
    private int _damage = 10;
    private bool _isHit = false; // 한 공격 주기당 한 번의 타격만 허용하는 플래그
    private Collider _collider; // 콜라이더 컴포넌트 캐싱

    private void Awake()
    {
        // Collider 컴포넌트 캐싱 및 초기 설정
        _collider = GetComponent<Collider>();
        if (_collider != null && !_collider.isTrigger)
        {
            Debug.LogWarning($"RootHitbox: {gameObject.name}의 Collider가 Is Trigger로 설정되어 있지 않습니다. 자동으로 설정합니다.");
            _collider.isTrigger = true;
        }
        // 초기에는 콜라이더를 비활성화해 둡니다.
        _collider.enabled = false;
    }

    /// <summary>
    /// 공격 주기 시작 시 ForestBoss가 호출하여 피해량 설정 및 히트박스를 활성화합니다.
    /// (ForestBoss.cs의 StartStrike() 호출에 대응하여 메서드 이름 변경)
    /// </summary>
    /// <param name="damageAmount">적용할 피해량</param>
    public void StartStrike(int damageAmount) // <--- 메서드 이름 수정 (PrepareHitbox -> StartStrike)
    {
        // 1. 피해량 설정
        _damage = damageAmount;

        // 2. 플래그 초기화: 새로운 공격 주기 시작
        _isHit = false;

        // 3. 콜라이더 활성화: 이제부터 플레이어에게 피해를 줄 수 있습니다.
        if (_collider != null)
        {
            _collider.enabled = true;
        }
    }

    /// <summary>
    /// 공격 주기 종료 시 ForestBoss가 호출하여 히트박스를 비활성화합니다.
    /// </summary>
    public void EndStrike() // <--- EndStrike 메서드 추가
    {
        // 1. 콜라이더 비활성화: 더 이상 피해를 줄 수 없습니다.
        if (_collider != null)
        {
            _collider.enabled = false;
        }
    }

    /// <summary>
    /// Collider가 활성화된 상태에서 다른 콜라이더와 접촉 시 호출됩니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 1. 이미 플레이어를 타격했거나, 콜라이더가 비활성화 상태였다면 (안전 장치)
        if (_isHit || (_collider != null && !_collider.enabled)) return;

        // 2. 플레이어 태그 확인 (이 게임의 플레이어가 "Player" 태그를 사용한다고 가정)
        if (other.CompareTag("Player"))
        {
            _isHit = true;
            other.GetComponent<IDamageable>()?.TakeDamage(_damage,DamageType.Physical);
        }
    }
}