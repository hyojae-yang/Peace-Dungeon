// ���ظ� ���� �� �ִ� ��� ����� �����ؾ� �ϴ� �������̽��Դϴ�.
public interface IDamageable
{
    // ����, ���� �� Ÿ�Կ� ������� ���� �������� ������ �޼���
    void TakeDamage(float damage);

    // ���� Ÿ��(����/����)�� ���� �ٸ� ������ �����ϴ� �޼���
    void TakeDamage(float damage, DamageType type);
}