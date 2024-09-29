using BepInEx;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace RefurbishedRoboMando
{
    public static class RoboMandoToggle
    {
        public static ConfigEntry<bool> Enable_Icon_Change;
        public static ConfigEntry<bool> Enable_Logbook_Change;
        public static ConfigEntry<bool> Enable_Skin_Disable;
        public static ConfigEntry<bool> Enable_Skill_Stat_Changes;
        public static ConfigEntry<bool> Enable_Body_Stat_Changes;
        public static ConfigEntry<bool> Enable_Token_Changes;
    }
    public static class RoboMandoStats
    {
        // Icon
        public static ConfigEntry<bool> Icon_Direction;

        // Skill Stats
        public static ConfigEntry<float> Shoot_Damage;
        public static ConfigEntry<float> Shoot_Coefficient;

        public static ConfigEntry<float> Zap_Damage;
        public static ConfigEntry<float> Zap_Coefficient;
        public static ConfigEntry<float> Zap_Cooldown;

        public static ConfigEntry<float> Dive_Splat_Duration;
        public static ConfigEntry<float> Dive_Cooldown;

        public static ConfigEntry<float> Hack_Duration;
        public static ConfigEntry<float> Hack_Cooldown;
        public static ConfigEntry<float> Hack_NoTarget_Cooldown;

        // Body Stats
        public static ConfigEntry<int> Health_Shield_Percent;
    }
    public static class Configs
    {
        private static BaseUnityPlugin staticPlugin;
        public static void SetUp(BaseUnityPlugin plugin)
        {
            staticPlugin = plugin;

            // Skill Modification Configs
            RoboMandoToggle.Enable_Icon_Change = staticPlugin.Config.Bind(
                "! General !",
                "Replace ROBOMANDO Survivor Icon?", true,
                "[ True = Replaced | False = Original ]\nReplaces ROBOMANDO's icon"
            );
            RoboMandoToggle.Enable_Logbook_Change = staticPlugin.Config.Bind(
                "! General !",
                "Replace ROBOMANDO Logbook Model?", true,
                "[ True = Replaced | False = Original ]\nReplaces ROBOMANDO's Logbook model"
            );
            RoboMandoToggle.Enable_Skin_Disable = staticPlugin.Config.Bind(
                "! General !",
                "Disable Non Whitelisted Skins?", true,
                "[ True = Whitelist | False = Original ]\nRemoves non whitelisted skins, and adds a configurable whitelist"
            );
            RoboMandoToggle.Enable_Skill_Stat_Changes = staticPlugin.Config.Bind(
                "! General !",
                "Enable Skill Stat Reworks?", true,
                "[ True = Reworked | False = Original ]\nAllows configurable stats to ROBOMANDO's skills"
            );
            RoboMandoToggle.Enable_Body_Stat_Changes = staticPlugin.Config.Bind(
                "! General !",
                "Enable Body Stat Reworks?", true,
                "[ True = Reworked | False = Original ]\nAllows minor configurations to ROBOMANDO's base stats"
            );
            RoboMandoToggle.Enable_Token_Changes = staticPlugin.Config.Bind(
                "! General !",
                "Enable Description Rewrite?", true,
                "[ True = Rewritten | False = Original ]\nChanges ROBOMANDO's descriptive texts, required for visible skill stat changes"
            );

            if (RoboMandoToggle.Enable_Icon_Change.Value)
            {
                RoboMandoStats.Icon_Direction = staticPlugin.Config.Bind(
                    "Survivor Icon",
                    "Icon Direction", true,
                    "[ True = Face Left | False = Face Right ]\nWhat direction the icon faces"
                );
            }

            if (RoboMandoToggle.Enable_Skill_Stat_Changes.Value)
            {
                string skillPrefix = "Skill Statistics";

                // Primary
                RoboMandoStats.Shoot_Damage = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Primary Damage Modifier", 75f,
                    "[ 75.0 = 75% Damage | Original = 80% ]\nDamage per primary shot"
                );
                RoboMandoStats.Shoot_Coefficient = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Primary Coefficient Modifier", 100f,
                    "[ 100.0 = 100% Coefficient | Original = 100% ]\nChance to proc per primary shot"
                );

                // Secondary
                RoboMandoStats.Zap_Damage = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Secondary Damage Modifier", 220f,
                    "[ 220.0 = 220% Damage | Original = 180% ]\nDamage per secondary shot"
                );
                RoboMandoStats.Zap_Coefficient = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Secondary Coefficient Modifier", 200f,
                    "[ 200.0 = 200% Coefficient | Original = 300% ]\nChance to proc per secondary shot"
                );
                RoboMandoStats.Zap_Cooldown = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Secondary Cooldown", 3f,
                    "[ 3.0 = 3 Seconds | Original = 2 Seconds ]\nSecondary cooldown timer"
                );

                // Utility
                RoboMandoStats.Dive_Splat_Duration = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Utility Splat Duration", 0.75f,
                    "[ 0.75 = 0.75 Seconds | Original = 2 Seconds ]\nTimer on falling after utility"
                );
                RoboMandoStats.Dive_Cooldown = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Utility Cooldown", 4f,
                    "[ 4.0 = 4 Seconds | Original = 4 Seconds ]\nUtility cooldown timer"
                );

                // Special
                RoboMandoStats.Hack_Duration = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Special Duration", 3f,
                    "[ 3.0 = 3 Seconds | Original = 3.33 Seconds ]\nHacking duration"
                );
                RoboMandoStats.Hack_Cooldown = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Special Cooldown", 5f,
                    "[ 5.0 = 5 Seconds | Original = 8 Seconds ]\nSpecial cooldown timer"
                );
                RoboMandoStats.Hack_NoTarget_Cooldown = staticPlugin.Config.Bind(
                    skillPrefix,
                    "Special Fail Cooldown", 0.5f,
                    "[ 0.5 = 0.5 Seconds | Original = 2 Seconds ]\nSpecial cooldown timer when no target"
                );
            }
            if (RoboMandoToggle.Enable_Body_Stat_Changes.Value)
            {
                string bodyPrefix  = "Body Statistics";

                // Health to Shield proportion
                RoboMandoStats.Health_Shield_Percent = staticPlugin.Config.Bind(
                    bodyPrefix,
                    "Health to Shield Percent", 0,
                    new ConfigDescription(
                        "[ 0.0 = 0% Conversion ]\nBase Health to Shield percent",
                        new AcceptableValueRange<int>(0, 100)
                    )
                );
            }

            if (RoboMandoToggle.Enable_Skin_Disable.Value)
            {
                string allowedSkins = staticPlugin.Config.Bind(
                    "Skin Whitelist",
                    "Allowed Skins",
                    "NOODLEGEMO_SKIN_ROBOMANDO_NAME",
                    "List of ROBOMANDO skins that will be ignored when disabling, comma seperated." +
                    "\n-\nExample: NOODLEGEMO_SKIN_ROBOMANDO_NAME, DEFAULT_SKIN, ..."
                ).Value;

                FindSkinDefs(allowedSkins);
            }
        }
        public static void SkinDefList(List<string> allSkins)
        {
            string allSkinDefs = "\n";
            foreach (string skinToken in allSkins) { allSkinDefs += ( skinToken + ", " );}

            _ = staticPlugin.Config.Bind(
                "Skin Whitelist",
                "All Skin Tokens",
                "",
                string.Format("Just for the sake of printing every single Skin Token for disabling / enabling.\n-{0}", allSkinDefs)
            );
        } 
        private static void FindSkinDefs(string allowedDefs)
        {
            string[] strings = allowedDefs.Split(',');
            foreach (string result in strings)
            {
                if (string.IsNullOrEmpty(result)) continue;
                if (string.IsNullOrEmpty(result.Trim())) continue;
                RefurbishedRoboMando.whitelistSkins.Add(result.Trim());
            }
        }
    }
}
