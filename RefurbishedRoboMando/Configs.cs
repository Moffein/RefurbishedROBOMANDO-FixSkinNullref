using BepInEx.Configuration;

namespace RefurbishedRoboMando
{
    public static class RefurbishConfigEntry
    {
        public static ConfigEntry<bool> Replace_Icon;
        public static ConfigEntry<Configs.IconDirection> Icon_Direction;

        public static ConfigEntry<Configs.SkinOrder> Skin_Order;
        public static ConfigEntry<bool> Disable_Skins;

        public static ConfigEntry<bool> Replace_Tokens;

        public static ConfigEntry<bool> Replace_Logbook_Model;
        public static ConfigEntry<bool> Replace_Logbook_Lore;

        public static ConfigEntry<bool> Replace_Skill_Stats;
        public static ConfigEntry<bool> Replace_Body_Stats;
        public static ConfigEntry<int> Health_Convert;

        public static ConfigEntry<bool> Jury_Rework;
    }
    public static class PresetSkillStats
    {
        public static float Shoot_Damage = 75f;
        public static float Shoot_Coefficient = 1f;

        public static float Zap_Damage = 220f;
        public static float Zap_Coefficient = 2f;
        public static float Zap_Cooldown = 3f;

        public static float Dive_Splat_Duration = 0.5f;
        public static float Dive_Cooldown = 4f;

        public static float Hack_Duration = 3f;
        public static float Hack_Cooldown = 5f;
        public static float Hack_NoTarget_Cooldown = 0.5f;
    }
    public class Configs
    {
        public enum IconDirection { LEFT, RIGHT };
        public enum SkinOrder { FIRST, LAST };
        public Configs()
        {
            // Survivor Select Icon
            string iconPrefix = "Icon";
            RefurbishConfigEntry.Replace_Icon = AssetStatics.plugin.Config.Bind(
                iconPrefix,
                "Enable Replacement", true,
                "[ True = Replace | False = Original ]\nChanges ROBOMANDO's icon"
            );
            RefurbishConfigEntry.Icon_Direction = AssetStatics.plugin.Config.Bind(
                iconPrefix,
                "Icon Direction", IconDirection.LEFT,
                "[ Left = Risk of Rain Returns | Right = Normal ]\nChanges ROBOMANDO's icon direction"
            );

            // Skin Order
            string skinPrefix = "Skins";
            RefurbishConfigEntry.Skin_Order = AssetStatics.plugin.Config.Bind(
                skinPrefix,
                "Enable Replacement", SkinOrder.FIRST,
                "[ First = Modded Skins First | Last = Modded Skins Last ]\nChanges REFURBISHED skin order"
            );
            RefurbishConfigEntry.Disable_Skins = AssetStatics.plugin.Config.Bind(
                skinPrefix,
                "Disable Other Skins", false,
                "[ True = Disables | False = Keeps ]\nToggles non Refurbished ROBOMANDO skins"
            );

            // Logbook
            string logbookPrefix = "Logbook";
            RefurbishConfigEntry.Replace_Logbook_Model = AssetStatics.plugin.Config.Bind(
                logbookPrefix,
                "Enable Model Replacement", true,
                "[ True = Replace | False = Original ]\nChanges ROBOMANDO's Logbook model"
            );
            RefurbishConfigEntry.Replace_Logbook_Lore = AssetStatics.plugin.Config.Bind(
                logbookPrefix,
                "Enable Lore Replacement", true,
                "[ True = Replace | False = Original ]\nChanges ROBOMANDO's Logbook lore"
            );

            // Language Tokens
            string languageToken = "Rewrites";
            RefurbishConfigEntry.Replace_Tokens = AssetStatics.plugin.Config.Bind(
                languageToken,
                "Enable Rewrite", true,
                "[ True = Rewrite | False = Original ]\nChanges ROBOMANDO's rewrite text"
            );

            // Stat Changes
            string statPrefix = "Stats";
            RefurbishConfigEntry.Replace_Skill_Stats = AssetStatics.plugin.Config.Bind(
                statPrefix,
                "Enable Skill Stat Replacement", true,
                "[ True = Replace | False = Original ]\nChanges ROBOMANDO's skill stats to be closer to Risk of Rain Returns"
            );
            RefurbishConfigEntry.Replace_Body_Stats = AssetStatics.plugin.Config.Bind(
                statPrefix,
                "Enable Body Stat Replacement", true,
                "[ True = Replace | False = Original ]\nChanges ROBOMANDO's body stats to be closer to Risk of Rain Returns"
            );
            RefurbishConfigEntry.Health_Convert = AssetStatics.plugin.Config.Bind(
                statPrefix,
                "Health to Shield Conversion", 0,
                new ConfigDescription(
                    "[ 0.0 = 0% Conversion ]\nBase health to shield percent",
                    new AcceptableValueRange<int>(0, 100)
                )
            );

            // Skill Reworks
            string reworkToken = "Reworks";
            RefurbishConfigEntry.Jury_Rework = AssetStatics.plugin.Config.Bind(
                reworkToken,
                "Re-Wire - Jury Rework", true,
                "[ True = Rework | False = Original ]\nChanges Jury into Sabotage, taking the actual item from a Printer"
            );
        }
    }
}