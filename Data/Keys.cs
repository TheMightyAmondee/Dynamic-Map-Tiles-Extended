using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Tiles;

namespace DMT.Data
{
    public static class Keys
    {
        public const string AddLayerKey = "DMT/addLayer";
        public const string AddTilesheetKey = "DMT/addTilesheet";
        public const string ChangeIndexKey = "DMT/changeIndex";
        public const string ChangeMultipleIndexKey = "DMT/changeMultipleIndex";
        public const string ChangePropertiesKey = "DMT/changeProperties";
        public const string ChangeMultiplePropertiesKey = "DMT/changeMultipleProperties";
        public const string ExplodeKey = "DMT/explode";
        public const string ExplosionKey = "DMT/explosion";
        public const string PushKey = "DMT/push";
        public const string PushableKey = "DMT/pushable";
        public const string PushAlsoKey = "DMT/pushAlso";
        public const string PushOthersKey = "DMT/pushOthers";
        public const string SoundKey = "DMT/sound";
        public const string TeleportKey = "DMT/teleport";
        public const string TeleportTileKey = "DMT/teleportTile";
        public const string GiveKey = "DMT/give";
        public const string TakeKey = "DMT/take";
        public const string ChestKey = "DMT/chest";
        public const string MessageKey = "DMT/message";
        public const string EventKey = "DMT/event";
        public const string MailKey = "DMT/mail";
        public const string MailRemoveKey = "DMT/mailRemove";
        public const string MailBoxKey = "DMT/mailbox";
        public const string InvalidateKey = "DMT/invalidate";
        public const string MusicKey = "DMT/music";
        public const string HealthKey = "DMT/health";
        public const string StaminaKey = "DMT/stamina";
        public const string HealthPerSecondKey = "DMT/healthPerSecond";
        public const string StaminaPerSecondKey = "DMT/staminaPerSecond";
        public const string BuffKey = "DMT/buff";
        public const string SpeedKey = "DMT/speed";
        public const string MoveKey = "DMT/move";
        public const string EmoteKey = "DMT/emote";
        public const string AnimationKey = "DMT/animation";
        public const string SlipperyKey = "DMT/slippery";
        public const string WarpKey = "DMT/warp";

        public static readonly List<string> AllKeys = new List<string>() 
        {
            AddLayerKey,
            AddTilesheetKey,
            ChangeIndexKey,
            ChangeMultipleIndexKey,
            ChangePropertiesKey,
            ChangeMultiplePropertiesKey,
            ExplodeKey,
            ExplosionKey,
            PushKey,
            PushOthersKey,
            SoundKey,
            TeleportKey,
            TeleportTileKey,
            GiveKey,
            TakeKey,
            ChestKey,
            MessageKey,
            EventKey,
            MailKey,
            MailRemoveKey,
            MailBoxKey,
            InvalidateKey,
            MusicKey,
            HealthKey,
            StaminaKey,
            HealthPerSecondKey,
            StaminaPerSecondKey,
            BuffKey,
            SpeedKey,
            MoveKey,
            EmoteKey,
            AnimationKey,
            WarpKey
        };

        public static readonly HashSet<string> ModKeys = new HashSet<string>();
    }
}
