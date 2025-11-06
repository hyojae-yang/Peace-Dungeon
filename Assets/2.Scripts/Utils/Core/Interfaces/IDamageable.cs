// 피해를 입을 수 있는 모든 대상이 구현해야 하는 인터페이스입니다.
public interface IDamageable
{
    // 물리, 마법 등 타입에 관계없이 순수 데미지만 입히는 메서드
    void TakeDamage(float damage);

    // 공격 타입(물리/마법)에 따라 다른 방어력을 적용하는 메서드
    void TakeDamage(float damage, DamageType type);
}