
public static class SaveData
{
    public static bool[] Weapons= new bool[3];
    public static bool W2VNCompleted = false;
    public static bool W3VNCompleted = false;
    public static int SceneNum = -1;
    public static int W3EnemyNum = 0;
    public static bool[] RoomsFinished = new bool[4];
    public static int[] Deaths = new int[8];
    public static string[] WeaponName = {"", "Grey Zone", "Blue Star", "Senbonzakura", "Hibana", "Meteor"};
    public static string[] WeaponDialogue = {"", 
        "Low damage. High energy efficiency and rate of fire. Lower energy leads to higher damage.",
        "Moderate damage. Phases through enemies and bounces off walls. Useful against large numbers.",
        "Higher damage at close range. Charging weapon increases range, spread, and damage.",
        "High damage, but high energy usage and movement penalty. Damage nonlinear to charge time; energy use is linear.",
        "High damage. Missiles track cursor. Missile count and damage depend on energy. High energy cost and long cooldown."
    };

    public static (float X, int Y) [] rAttacks3 = {(5, 4), (9, 4), (13, 4), (17, 4), (21, 4), (25, 4), (29, 4),
    (51, 1), (55, 0), (59, 0), (63, 0), (65, 1), (84, 3), (88, 3), (92, 33), (99, 6), (103, 6), (107, 0), (111, 6),
    (115, 5), (119, 6), (123, 6), (127, 5), (128, 1), (131, 3), (139, 3), (147, 33), (155, 4), (163, 3), 
    (168, 2), (172, 2), (182, 0), (186, 0), (190, 1), (195, 3), (199, 5), 
    (204, 0), (206, 0), (208, 0), (210, 0), (212, 0), (214, 1), (219, 4), (223, 4), (227, 2)};
    public static (float X, int Y) [] bAttacks3 = {};


}
