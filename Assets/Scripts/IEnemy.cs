public interface IEnemy
{
    void Damage (int dmg, bool stun);
    void MeleeDamage (int dmg, bool stun);
}

public interface IBullet
{
    void SetValues (int dmg, float speed, float timer, float accel);
}

public interface IMissile
{
    void SetSpeed (float initSpeed, float accel, float finalSpeed);
    void SetValues (int dmg, float homingStrength, float timer, bool stun);
}