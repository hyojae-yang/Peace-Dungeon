using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// IDetectable과 IDamageable 인터페이스를 구현하며, 플레이어의 체력 및 방어 로직을 관리합니다.
/// 이 스크립트는 더 이상 싱글턴이 아니며, PlayerCharacter의 멤버로 관리됩니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDetectable, IDamageable
{
    // 중앙 허브 역할을 하는 PlayerCharacter 인스턴스에 대한 참조입니다.
    private PlayerCharacter playerCharacter;

    void Start()
    {
        // PlayerCharacter의 인스턴스를 가져와서 참조를 확보합니다.
        playerCharacter = PlayerCharacter.Instance;
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("PlayerCharacter 또는 PlayerStats가 초기화되지 않았습니다. PlayerHealth 스크립트가 제대로 동작하지 않을 수 있습니다.");
        }
    }

    // IDetectable 인터페이스의 메서드 구현

    /// <summary>
    /// 플레이어가 감지 가능한 상태인지 확인합니다.
    /// </summary>
    public bool IsDetectable()
    {
        // PlayerCharacter 및 playerStats가 유효한지 먼저 확인합니다.
        if (playerCharacter != null && playerCharacter.playerStats != null)
        {
            // 플레이어가 살아있다면 감지 가능하도록 true를 반환합니다.
            return playerCharacter.playerStats.health > 0;
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
    /// 순수 데미지 값을 받는 메서드입니다.
    /// </summary>
    /// <param name="amount">입을 데미지량</param>
    public void TakeDamage(float amount)
    {
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. TakeDamage(float amount) 실패.");
            return;
        }

        // PlayerStats의 health 변수에 직접 접근하여 데미지를 적용합니다.
        playerCharacter.playerStats.health -= amount;


        // 체력이 0보다 작거나 같아지면 죽음 처리
        if (playerCharacter.playerStats.health <= 0)
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
        if (playerCharacter == null || playerCharacter.playerStats == null)
        {
            Debug.LogError("플레이어 스탯에 접근할 수 없습니다. TakeDamage(float amount, DamageType type) 실패.");
            return;
        }

        float finalDamage = amount;

        // 데미지 타입에 따라 방어력 적용
        switch (type)
        {
            case DamageType.Physical:
                finalDamage = Mathf.Max(amount - playerCharacter.playerStats.defense, 0);
                break;
            case DamageType.Magic:
                finalDamage = Mathf.Max(amount - playerCharacter.playerStats.magicDefense, 0);
                break;
            case DamageType.True:
                // 고정 피해는 방어력을 무시합니다.
                break;
        }

        playerCharacter.playerStats.health -= finalDamage;

        if (playerCharacter.playerStats.health <= 0)
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
        gameObject.SetActive(false);
    }
}