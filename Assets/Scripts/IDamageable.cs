/// <summary>
/// Anything that can receive damage implements this (Dwarf, Dragon, Player, etc).
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
