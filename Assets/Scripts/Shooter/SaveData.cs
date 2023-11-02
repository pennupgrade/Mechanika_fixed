
public static class SaveData
{
    public static bool[] Weapons= new bool[3];
    public static bool W2VNCompleted = false;
    public static bool W3VNCompleted = false;
    public static int SceneNum = -1;
    public static int[] Deaths = new int[6];
    public static string[] WeaponName = {"", "Cepheid", "DISC", "Senbonzakura", "NOVA", "Meteor"};
    public static string[] WeaponDialogue = {"", 
        "Low damage. High energy efficiency and rate of fire. Lower energy leads to higher damage.",
        "Moderate damage. Phases through enemies and bounces off walls. Useful against large numbers.",
        "High damage at close range. Charging weapon increases range, spread, and damage.",
        "Very high damage, but high energy usage and movement penalty. Damage, and energy use linear to charge.",
        "Very high damage. Missiles track cursor and stop homing when close. High energy usage and long cooldown."
    };


}
