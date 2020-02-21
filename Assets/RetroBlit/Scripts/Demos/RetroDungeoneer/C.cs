namespace RetroBlitDemoRetroDungeoneer
{
    using UnityEngine;

    /// <summary>
    /// Collection of global constants and statics
    /// </summary>
    public static class C
    {
        /// <summary>
        /// Minimum room size
        /// </summary>
        public const int ROOM_MIN_SIZE = 6;

        /// <summary>
        /// Maximum room size
        /// </summary>
        public const int ROOM_MAX_SIZE = 10;

        /// <summary>
        /// Maximum amount of rooms on a single level
        /// </summary>
        public const int MAX_ROOMS = 30;

        /// <summary>
        /// Frames until key repeat starts
        /// </summary>
        public const int KEY_REPEAT_SPEED_STAGE1 = 12;

        /// <summary>
        /// Frames between keep repeats once started by stage 1
        /// </summary>
        public const int KEY_REPEAT_SPEED_STAGE2 = 6;

        /// <summary>
        /// Screen width in pixels
        /// </summary>
        public static int SCREEN_WIDTH;

        /// <summary>
        /// Screen height in pixels
        /// </summary>
        public static int SCREEN_HEIGHT;

        /// <summary>
        /// Maximum level width
        /// </summary>
        public static int MAP_WIDTH = 80;

        /// <summary>
        /// Maximum level height
        /// </summary>
        public static int MAP_HEIGHT = 45;

        /// <summary>
        /// Grid layer
        /// </summary>
        public static int LAYER_GRID = 0;

        /// <summary>
        /// Terrain layer
        /// </summary>
        public static int LAYER_TERRAIN = 1;

        /// <summary>
        /// Visibility layer
        /// </summary>
        public static int LAYER_VISIBILITY = 2;

        /// <summary>
        /// Color of menu background
        /// </summary>
        public static Color32 COLOR_MENU_BACKGROUND = new Color32(30, 30, 30, 255);

        /// <summary>
        /// Color of menu header background
        /// </summary>
        public static Color32 COLOR_MENU_HEADER_BACKGROUND = new Color32(50, 50, 50, 255);

        /// <summary>
        /// Shared fast string
        /// </summary>
        public static FastString FSTR = new FastString(8192);

        /// <summary>
        /// Secondary shared fast string
        /// </summary>
        public static FastString FSTR2 = new FastString(8192);

        /// <summary>
        /// Color to use for names of entities
        /// </summary>
        public static string STR_COLOR_DIALOG = "@AAAAAA";

        /// <summary>
        /// Color to use for names of entities
        /// </summary>
        public static string STR_COLOR_NAME = "@FFFF50";

        /// <summary>
        /// Color to use for damage text
        /// </summary>
        public static string STR_COLOR_DAMAGE = "@FF5050";

        /// <summary>
        /// Color to use for dead entity string
        /// </summary>
        public static string STR_COLOR_DEAD = "@AA2020";

        /// <summary>
        /// Color to use for corpses
        /// </summary>
        public static string STR_COLOR_CORPSE = "@C8C8C8";

        /// <summary>
        /// Field of view radius
        /// </summary>
        public static int FOV_RADIUS;

        /// <summary>
        /// Name of saved file folder
        /// </summary>
        public static string SAVE_FOLDER = "rbrl_save";

        /// <summary>
        /// Name of saved game file
        /// </summary>
        public static string SAVE_FILENAME = "saved_game.dat";

        /// <summary>
        /// Music for main menu
        /// </summary>
        public static int MUSIC_MAIN_MENU = 0;

        /// <summary>
        /// Music for game play
        /// </summary>
        public static int MUSIC_GAME = 1;

        /// <summary>
        /// Music to play upon death
        /// </summary>
        public static int MUSIC_DEATH = 2;

        /// <summary>
        /// Music to play in forest
        /// </summary>
        public static int MUSIC_FOREST = 3;

        /// <summary>
        /// Monster death sound
        /// </summary>
        public static int SOUND_MONSTER_DEATH = 0;

        /// <summary>
        /// Player death sound
        /// </summary>
        public static int SOUND_PLAYER_DEATH = 1;

        /// <summary>
        /// Player foot step sound
        /// </summary>
        public static int SOUND_FOOT_STEP = 2;

        /// <summary>
        /// Monster attack sound
        /// </summary>
        public static int SOUND_MONSTER_ATTACK = 3;

        /// <summary>
        /// Player attack sound
        /// </summary>
        public static int SOUND_PLAYER_ATTACK = 4;

        /// <summary>
        /// Inventory sound, used for pickup, drop, equip, de-equip
        /// </summary>
        public static int SOUND_INVENTORY = 5;

        /// <summary>
        /// Drink sound
        /// </summary>
        public static int SOUND_DRINK = 6;

        /// <summary>
        /// Menu open sound
        /// </summary>
        public static int SOUND_MENU_OPEN = 7;

        /// <summary>
        /// Menu close sound
        /// </summary>
        public static int SOUND_MENU_CLOSE = 8;

        /// <summary>
        /// Take stairs sound
        /// </summary>
        public static int SOUND_STAIRS = 9;

        /// <summary>
        /// Mouse pointer hovered selection changed
        /// </summary>
        public static int SOUND_POINTER_SELECT = 10;

        /// <summary>
        /// Select option sound
        /// </summary>
        public static int SOUND_SELECT_OPTION = 11;

        /// <summary>
        /// Level up jingle sound
        /// </summary>
        public static int SOUND_LEVEL_UP = 12;

        /// <summary>
        /// Fireball sound
        /// </summary>
        public static int SOUND_FIREBALL = 13;

        /// <summary>
        /// Lightning sound
        /// </summary>
        public static int SOUND_LIGHTNING = 14;

        /// <summary>
        /// Confusion sound
        /// </summary>
        public static int SOUND_CONFUSE = 15;

        /// <summary>
        /// Enter cheat mode sound
        /// </summary>
        public static int SOUND_CHEAT = 16;

        /// <summary>
        /// Aggro sound 1
        /// </summary>
        public static int SOUND_AGGRO1 = 17;

        /// <summary>
        /// Aggro sound 2
        /// </summary>
        public static int SOUND_AGGRO2 = 18;

        /// <summary>
        /// Player fall yell (for entrance effect)
        /// </summary>
        public static int SOUND_PLAYER_FALL_YELL = 19;

        /// <summary>
        /// Portal teleport
        /// </summary>
        public static int SOUND_PORTAL = 20;

        /// <summary>
        /// Sound bow shoot
        /// </summary>
        public static int SOUND_BOW_SHOOT = 21;

        /// <summary>
        /// Sound bow hit
        /// </summary>
        public static int SOUND_BOW_HIT = 22;

        /// <summary>
        /// Sound web
        /// </summary>
        public static int SOUND_WEB = 23;

        /// <summary>
        /// Portal teleport
        /// </summary>
        public static int SOUND_JUMP = 24;

        /// <summary>
        /// Teleport
        /// </summary>
        public static int SOUND_TELEPORT = 25;

        /// <summary>
        /// Slime
        /// </summary>
        public static int SOUND_SLIME = 26;

        /// <summary>
        /// Font
        /// </summary>
        public static int FONT_RETROBLIT_DROPSHADOW = 0;

        /// <summary>
        /// Small font to use
        /// </summary>
        public static int FONT_SMALL = FONT_RETROBLIT_DROPSHADOW;

        /// <summary>
        /// Shader to vignette effect
        /// </summary>
        public static int SHADER_VIGNETTE = 0;
    }
}
