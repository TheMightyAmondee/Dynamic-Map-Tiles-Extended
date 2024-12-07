namespace DMT.Data
{
    public static class Triggers
    {
        public const string StepOn = "On";
        public const string StepOff = "Off";
        public const string EnterLocation = "Enter";
        public const string PushTile = "Push";
        public const string PushedTile = "Pushed";
        public const string Explode = "Explode";
        public const string Action = "Action";
        public const string UseTool = "Tool{0}";
        public const string UseItem = "Item{0}";
        public const string TalkToNPC = "Talk{0}";
        public const string MonsterSlain = "MonsterSlain{0}";

        public const string UseToolRegex = @"Tool(\([A-z]{1,}\)){0,1}";
        public const string UseItemRegex = @"Item(\([\(\)_A-z0-9]{1,}(\-[0-9]{1,}){0,2}\)){0,1}"; //Item(\([\(\)_A-z0-9]{1,}((\-[0-9]{1,}){0,1}(\-[0-9]{1,}(\+{0,1}|\-{0,1}){0,1}){0,1}){0,1}\)){0,1}
        public const string TalkToNPCRegex = @"Talk(\([A-z]{1,}\)){0,1}";
        public const string MonsterSlainRegex = @"MonsterSlain(\([A-z]{1,}\)){0,1}";

        public static readonly HashSet<string> Regexes = [
            StepOn,
            StepOff,
            EnterLocation,
            PushTile,
            PushedTile,
            Explode,
            Action,
            UseToolRegex,
            UseItemRegex,
            TalkToNPCRegex,
            MonsterSlainRegex,
        ];

        public static readonly HashSet<string> GlobalTriggers = [
            EnterLocation,
            TalkToNPCRegex,
            MonsterSlainRegex,
        ];
    }
}
