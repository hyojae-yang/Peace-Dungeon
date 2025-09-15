using UnityEngine;
using System.Collections.Generic;

// IDetectable과 IDamageable 인터페이스를 구현합니다.
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // PlayerStats 스크립트는 이제 싱글턴으로 접근하므로 변수가 필요 없습니다.
    // private PlayerStats playerStats;

    void Start()
    {
        // 게임 시작 시, PlayerStats 싱글턴 인스턴스가 존재하는지 확인합니다.
        // GetComponent를 통해 가져올 필요가 없습니다.
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats 인스턴스가 존재하지 않습니다. 게임 시작 시 PlayerStats를 가진 게임 오브젝트가 씬에 있는지 확인해 주세요.");
        }
    }

    // IDetectable 인터페이스의 메서드 구현

    /// <summary>
    /// 플레이어가 감지 가능한 상태인지 확인합니다.
    /// </summary>
    public bool IsDetectable()
    {
        // 플레이어가 살아있다면 감지 가능하도록 true를 반환합니다.
        if (PlayerStats.Instance != null)
        {
            return PlayerStats.Instance.health > 0;
        }
        return false;
    }

    /// <summary>
    /// 이 오브젝트의 트랜스폼을 반환합니다.
    /// </summary>
    public Transform GetTransform()
    {
        return this.transform;
    }

    // IDamageable 인터페이스의 메서드 구현 (오버로딩)

    /// <summary>
    /// 순수 데미지 값을 받는 메서드입니다. (추후 사용 가능)
    /// </summary>
    /// <param name="amount">입을 데미지량</param>
    public void TakeDamage(float amount)
    {
        // PlayerStats 인스턴스가 유효한지 다시 한번 확인합니다.
        if (PlayerStats.Instance == null) return;

        // PlayerStats의 health 변수에 직접 접근하여 데미지를 적용합니다.
        PlayerStats.Instance.health -= amount;

        Debug.Log("플레이어가 데미지를 입었습니다! 남은 체력: " + PlayerStats.Instance.health);

        // 체력이 0보다 작거나 같아지면 죽음 처리
        if (PlayerStats.Instance.health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지 타입에 따라 플레이어의 방어력을 적용하여 피해를 계산하는 메서드입니다.
    /// </summary>
    /// <param name="amount">입을 데미지량</param>
    /// <param name="type">데미지 타입 (물리, 마법, 고정 피해 등)</param>
    public void TakeDamage(float amount, DamageType type)
    {
        // PlayerStats 인스턴스가 유효한지 다시 한번 확인합니다.
        if (PlayerStats.Instance == null) return;

        float finalDamage = amount;

        // 데미지 타입에 따라 방어력 적용
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - PlayerStats.Instance.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - PlayerStats.Instance.magicDefense, 0);
                break;
            case DamageType.True:
                // 고정 피해는 방어력을 무시합니다.
                break;
        }

        PlayerStats.Instance.health -= finalDamage;

        Debug.Log($"플레이어가 {finalDamage}의 {type} 피해를 입었습니다! 남은 체력: {PlayerStats.Instance.health}");

        if (PlayerStats.Instance.health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 플레이어가 죽었을 때 호출될 메서드
    /// </summary>
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");

        // 여기에 게임 오버, 플레이어 오브젝트 파괴 등 추가 로직을 구현합니다.
        Destroy(gameObject);
    }
}