
public static class SaveData
{
    public static bool[] Weapons= new bool[3];
    public static bool W2VNCompleted = false;
    public static bool W3VNCompleted = false;
    public static int SceneNum = -1;
    public static int W3EnemyNum = 0;
    public static int[] Deaths = new int[6];
    public static string[] WeaponName = {"", "Grey Zone", "Blue Star", "Senbonzakura", "Hibana", "Meteor"};
    public static string[] WeaponDialogue = {"", 
        "Low damage. High energy efficiency and rate of fire. Lower energy leads to higher damage.",
        "Moderate damage. Phases through enemies and bounces off walls. Useful against large numbers.",
        "Higher damage at close range. Charging weapon increases range, spread, and damage.",
        "High damage, but high energy usage and movement penalty. Damage nonlinear to charge time; energy use is linear.",
        "High damage. Missiles track cursor. Missile count and damage depend on energy. High energy cost and long cooldown."
    };


}
