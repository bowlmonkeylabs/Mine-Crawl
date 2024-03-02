namespace BML.Scripts {
    public enum DamageType
    {
        Player_Pickaxe = 1 << 0,
        Player_Bomb = 1 << 1,
        Enemy_Slime_Contact = 1 << 2,
        Enemy_Bat_Contact = 1 << 3,
        Enemy_Sentinel_Projectile = 1 << 4,
        Enemy_Zombie_Contact = 1 << 5,
        Enemy_Mimic_Projectile = 1 << 6,
        Ore_Explosive = 1 << 7,
        Enemy_Snake_Projectile = 1 << 8,
        Fall_Damage = 1 << 9,
        Player_Pickaxe_Secondary = 1 << 10,
        Enemy_Bomb_Explosion = 1 << 11,
        Trap_Spike = 1 << 12,
        Trap_Stalactite = 1 << 13,
        Enemy_Worm = 1 << 14,
        Player_Projectile = 1 << 15,
        Player_FireBomb = 1 << 16,
    }
}
