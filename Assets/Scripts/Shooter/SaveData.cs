
using static Boss3AI.ExecutionEnum;

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
    (204, 0), (206, 0), (208, 0), (210, 0), (212, 0), (214, 1), /*(219, 4), (223, 4), (227, 2)*///
    (243, 5), /*(263, 4), (267, 4), (271, 4), (275, 4), (279, 4), (283, 4), (287, 0), (291, 0)*/ (295, 1),//
    (299, 3), (303, 3), (307, 33), (311, 1), (316, 7), (320, 7), (324, 7), (326, 5), (328, 7), (332, 7),
    (336, 7), (340, 7), (343, 3), (347, 4), (351, 1), (355, 0), (359, 1), (363, 33), (367, 66), (371, 66),
    (375, 66), (379, 1), (383, 2), (387, 1), (391, 4), (395, 1), (399, 4), (403, 5), (407, 3)};
    public static (float X, Boss3AI.ExecutionEnum Y) [] bAttacks3 = {(8, CQ_RING), (12, CQ_RING), (16, CQ_RING), (20, CQ_RING), (24, CQ_RING), (28, CQ_RING),
    (32, CQ_RING), (36, EXPANDING_RING_1), (38, EXPANDING_RING_1), (40, EXPANDING_RING_1), (42, EXPANDING_RING_1), (44, EXPANDING_RING_1), (46, EXPANDING_RING_1), (48, EXPANDING_RING_1), (52, CQ_RING), (56, EXPANDING_RING_1), (60, EXPANDING_RING_1),
    (64, EXPANDING_RING_2), (64, FIRST_PIANO_BOXES), (68, EXPANDING_RING_2), (72, EXPANDING_RING_2), (76, EXPANDING_RING_2), (78, EXPANDING_RING_2), (80, EXPANDING_RING_2), (82, EXPANDING_RING_2), (85, CQ_RING), (89, CQ_RING), 
    (93, 0), (99, INTIMIDATION_TRAIL_1), (101, INTIMIDATION_TRAIL_1), (103, INTIMIDATION_TRAIL_1), (105, INTIMIDATION_TRAIL_1), (107, INTIMIDATION_TRAIL_1), (109, INTIMIDATION_TRAIL_1), (111, INTIMIDATION_TRAIL_1), (113, INTIMIDATION_TRAIL_1), (115, CQ_RING), (117, INTIMIDATION_TRAIL_1), (119, CQ_RING), (121, INTIMIDATION_TRAIL_1), (123, CQ_RING), (125, INTIMIDATION_TRAIL_1), (127, INTIMIDATION_TRAIL_1), (129, CQ_RING), 
    (132, MINI_CIRCLE_EXPLODE_1), (132.5f, MINI_CIRCLE_EXPLODE_1), (133, MINI_CIRCLE_EXPLODE_1), (133.5f, MINI_CIRCLE_EXPLODE_1), (134, MINI_CIRCLE_EXPLODE_1), (134.5f, MINI_CIRCLE_EXPLODE_1), (135, MINI_CIRCLE_EXPLODE_1), (135.5f, MINI_CIRCLE_EXPLODE_1), (136, MINI_CIRCLE_EXPLODE_1), (136.5f, MINI_CIRCLE_EXPLODE_1), (137, MINI_CIRCLE_EXPLODE_1), (137.5f, MINI_CIRCLE_EXPLODE_1), (138, MINI_CIRCLE_EXPLODE_1), (138.5f, MINI_CIRCLE_EXPLODE_1),
    (139, MINI_CIRCLE_EXPLODE_1), (139.5f, MINI_CIRCLE_EXPLODE_1), (140, MINI_CIRCLE_EXPLODE_1), (140.5f, MINI_CIRCLE_EXPLODE_1), (141, MINI_CIRCLE_EXPLODE_1), (141.5f, MINI_CIRCLE_EXPLODE_1), (142, MINI_CIRCLE_EXPLODE_1), (142.5f, MINI_CIRCLE_EXPLODE_1), (143, MINI_CIRCLE_EXPLODE_1), (143.5f, MINI_CIRCLE_EXPLODE_1), (144, MINI_CIRCLE_EXPLODE_1), (144.5f, MINI_CIRCLE_EXPLODE_1), (145, MINI_CIRCLE_EXPLODE_1), (145.5f, MINI_CIRCLE_EXPLODE_1),
    (146, MINI_CIRCLE_EXPLODE_1), (146.5f, MINI_CIRCLE_EXPLODE_1), (147, MINI_CIRCLE_EXPLODE_1), (147.5f, MINI_CIRCLE_EXPLODE_1), (148, MINI_CIRCLE_EXPLODE_1), (148, FOLLOW_TRAIL_1), (148.5f, MINI_CIRCLE_EXPLODE_2), (149, MINI_CIRCLE_EXPLODE_2), (149.5f, MINI_CIRCLE_EXPLODE_2), (150, MINI_CIRCLE_EXPLODE_2), (150.5f, MINI_CIRCLE_EXPLODE_2), (151, MINI_CIRCLE_EXPLODE_2), (151.5f, MINI_CIRCLE_EXPLODE_2), (152, MINI_CIRCLE_EXPLODE_2), (153.5f, MINI_CIRCLE_EXPLODE_2),
    (154, MINI_CIRCLE_EXPLODE_2), (154.5f, MINI_CIRCLE_EXPLODE_2), (155, MINI_CIRCLE_EXPLODE_2), (155.5f, MINI_CIRCLE_EXPLODE_2), (156, MINI_CIRCLE_EXPLODE_2), (156.5f, MINI_CIRCLE_EXPLODE_2), (157, MINI_CIRCLE_EXPLODE_2), (157.5f, MINI_CIRCLE_EXPLODE_2), (158, MINI_CIRCLE_EXPLODE_2), (158.5f, MINI_CIRCLE_EXPLODE_2), (159, MINI_CIRCLE_EXPLODE_2), (159.5f, MINI_CIRCLE_EXPLODE_2), (160, MINI_CIRCLE_EXPLODE_2), (160.5f, MINI_CIRCLE_EXPLODE_2),
    (161, MINI_CIRCLE_EXPLODE_2), (161.5f, MINI_CIRCLE_EXPLODE_2), (162, MINI_CIRCLE_EXPLODE_2), (162.5f, MINI_CIRCLE_EXPLODE_2), (163, MINI_CIRCLE_EXPLODE_2), (163.5f, MINI_CIRCLE_EXPLODE_2), (164, MINI_CIRCLE_EXPLODE_2),
    (168, FIRE_BALL_1), (172, FIRE_BALL_2), (176, FIRE_BALL_1), (180, FIRE_BALL_2), (184, FIRE_BALL_1),
    (176, 0), (178, 0), (180, 0), (182, 0), (184, 0), (186, 0), (188, 0),
    (196, 0), (300, 0), (304, 0), (308, 0),
    (344, 0), (364, 0), (368, 0), (370, 0), (372, 0), (374, 0), (376, 0), (408, 0)};


}
