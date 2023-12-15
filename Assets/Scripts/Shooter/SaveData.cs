
public static class SaveData
{
    public static bool[] Weapons= new bool[3];
    public static bool W2VNCompleted = false;
    public static bool W3VNCompleted = false;
    public static bool MikuSong = false;
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
    (115, 5), (119, 66), (123, 66), (127, 5), (128, 1), /**/(131, 3), (139, 3), (147, 33), (155, 4), (163, 3), //
    (168, 2), (172, 2), (181, 0), (185, 0), (190, 1), (195, 3), (199, 5), 
    (204, 0), (206, 0), (208, 0), (210, 0), (212, 0), (214, 1), /**/(219, 4), (223, 4), (227, 2)/**/,//
    (243, 5), /**/(263, 4), (267, 4), (271, 4), (275, 4), (279, 4), (283, 4), (287, 0), (291, 0)/**/, (295, 1),//
    (299, 3), (303, 3), (307, 33), (311, 1), (316, 7), (320, 7), (324, 7), (326, 5), (328, 7), (332, 7),
    (336, 7), (340, 7), (343, 3), (347, 4), (351, 1), (355, 0), (359, 1), (363, 33), (367, 66), (371, 66),
    (375, 66), (379, 1), (383, 2), (387, 1), (391, 4), (395, 1), (399, 4), (403, 5), (407, 3)};
    public static (float X, int Y) [] bAttacks3 = {(8, 0), (12, 0), (16, 0), (20, 0), (24, 0), (28, 0),
    (32, 0), (36, 1), (38, 1), (40, 1), (42, 1), (44, 1), (46, 1), (48, 1), (52, 0), (56, 1), (60, 1),
    (64, 1), (68, 1), (72, 1), (76, 1), (78, 1), (80, 1), (82, 1), (85, 0), (89, 0), 
    (93, 0), (104, 0), (106, 0), (108, 0), (110, 0), (112, 0), (114, 0), (116, 0), (118, 0), (120, 0), 
    (122, 0), (124, 0), (126, 0), (140, 0), (148, 0), (164, 0),
    (176, 0), (178, 0), (180, 0), (182, 0), (184, 0), (186, 0), (188, 0),
    (196, 0), (300, 0), (304, 0), (308, 0),
    (344, 0), (364, 0), (368, 0), (370, 0), (372, 0), (374, 0), (376, 0), (408, 0)};


}
