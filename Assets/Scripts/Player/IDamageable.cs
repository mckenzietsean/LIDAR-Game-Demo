using UnityEngine;

public interface IDamageable
{
    void Heal(int heal);
    void TakeDamage(int damage);
    Transform GetTransform();
}
