using UnityEngine;
using System.Collections.Generic;

// IDetectable과 IDamageable 인터페이스를 구현합니다.
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // PlayerStats 스크립트의 레퍼런스를 담을 변수
    private PlayerStats playerStats;

    void Start()
    {
        // 게임 시작 시, 같은 게임 오브젝트에 부착된 PlayerStats 컴포넌트를 찾습니다.
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats 컴포넌트가 PlayerHealth와 같은 게임 오브젝트에 없습니다. PlayerStats를 먼저 부착해 주세요.");
        }
    }

    // IDetectable 인터페이스의 메서드 구현
    public bool IsDetectable()
    {
        // 플레이어가 살아있다면 감지 가능하도록 true를 반환합니다.
        if (playerStats != null)
        {
            return playerStats.health > 0;
        }
        return false;
    }

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
        if (playerStats == null) return;

        // PlayerStats의 health 변수에 직접 접근하여 데미지를 적용합니다.
        playerStats.health -= amount;

        Debug.Log("플레이어가 데미지를 입었습니다! 남은 체력: " + playerStats.health);

        // 체력이 0보다 작거나 같아지면 죽음 처리
        if (playerStats.health <= 0)
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
        if (playerStats == null) return;

        float finalDamage = amount;

        // 데미지 타입에 따라 방어력 적용
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - playerStats.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - playerStats.magicDefense, 0);
                break;
            case DamageType.True:
                // 고정 피해는 방어력을 무시합니다.
                break;
        }

        playerStats.health -= finalDamage;

        Debug.Log($"플레이어가 {finalDamage}의 {type} 피해를 입었습니다! 남은 체력: {playerStats.health}");

        if (playerStats.health <= 0)
        {
            Die();
        }
    }

    // 플레이어가 죽었을 때 호출될 메서드
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다!");

        // 여기에 게임 오버, 플레이어 오브젝트 파괴 등 추가 로직을 구현합니다.
        Destroy(gameObject);
    }
}