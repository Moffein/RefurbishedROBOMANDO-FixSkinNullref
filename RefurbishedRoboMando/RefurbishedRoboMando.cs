using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.IO;
using System;
using System.Collections.Generic;
using MonoMod.RuntimeDetour;
using static RefurbishedRoboMando.ColorCode;

namespace RefurbishedRoboMando
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency("com.rob.RobomandoMod")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    public class RefurbishedRoboMando : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "noodlegemo";
        public const string PluginName = "ROBOMANDO_Refurbished";
        public const string PluginVersion = "1.0.0";

        internal static RefurbishedRoboMando Instance { get; private set; }
        private static AssetBundle assetBundle;
        private static readonly string roboBodyName = "RobomandoBody";

        private static GameObject roboPrefab;
        public void Awake()
        {
            Log.Init(Logger);
            Configs.SetUp(this);
            Instance = this;

            RoR2Application.onLoad += ReplaceLast;
            if (RoboMandoToggle.Enable_Icon_Change.Value)
            {
                assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(Info.Location).ToString(), "Assets/robomandoasset"));
                BodyCatalog.availability.CallWhenAvailable(new Action(ReplaceIcon));
            }
        }

        private static void ReplaceLast()
        {
            if (RoboMandoToggle.Enable_Skin_Disable.Value) DisableSkins();
            if (RoboMandoToggle.Enable_Token_Changes.Value) AddLanguageTokens();
            if (RoboMandoToggle.Enable_Skill_Stat_Changes.Value) ModifySkillStats();
            if (RoboMandoToggle.Enable_Body_Stat_Changes.Value) ModifyBodyStats();
        }

        private static void ReplaceIcon()
        {
            roboPrefab = BodyCatalog.FindBodyPrefab(roboBodyName);
            CharacterBody characterBody = roboPrefab ? roboPrefab.GetComponent<CharacterBody>() : null;
            if (characterBody == null) return;

            if (RoboMandoStats.Icon_Direction.Value) characterBody.portraitIcon = assetBundle.LoadAsset<Sprite>("RobomandoIcon").texture;
            else characterBody.portraitIcon = assetBundle.LoadAsset<Sprite>("RobomandoIconAlt").texture;
        }
        private static void DisableSkins()
        {
            CharacterBody characterBody = roboPrefab ? roboPrefab.GetComponent<CharacterBody>() : null;
            if (!characterBody) return;

            List<SkinDef> allowedSkins = [];
            List<string> allTokens = [];

            foreach (SkinDef skinDef in BodyCatalog.GetBodySkins(characterBody.bodyIndex))
            {
                if (skinDef == null && skinDef.nameToken == null) continue;
                allTokens.Add(skinDef.nameToken);
                if (whitelistSkins.Contains(skinDef.nameToken)) allowedSkins.Add(skinDef);
            }

            Configs.SkinDefList(allTokens);

            BodyCatalog.skins[(int) characterBody.bodyIndex] = [.. allowedSkins];
            SkinCatalog.skinsByBody[(int)characterBody.bodyIndex] = [.. allowedSkins];

            ModelLocator modelLocator = roboPrefab.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                ModelSkinController skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
                if (skinController) skinController.skins = [.. allowedSkins];
            }
        }
        private static void AddLanguageTokens()
        {
            string tokenPrefix = "RAT_ROBOMANDO_";
            LanguageAPI.AddOverlay(tokenPrefix + "NAME", "ROBOMANDO");
            LanguageAPI.AddOverlay(tokenPrefix + "DESCRIPTION",
                "Your ROBOMANDO Model 7 comes equipped with everything it needs to extract resources from hostile environments.<color=#CCD3E0>" + 
                "\n\n< ! > As weak as Single Fire is, it has no fall-off damage." +
                "\n\n< ! > Despite the short range on De-Escalate, you can utilize it to pierce through multiple enemies at once, stunning them all in series." + 
                "\n\n< ! > Re-Wire allows instant access to any company sanctioned containers or drone utilities - make up your lack of strength and durability through stealing everything!" +
                "\n\n< ! > Remember that ROBOMANDO's skills aren't the most effective against enemies - use your increased movement speed to maintain distance accordingly."
            );

            LanguageAPI.AddOverlay(tokenPrefix + "PRIMARY_SHOT_DESCRIPTION", string.Format(
                "Agile".Style(FontColor.cIsUtility) + ". Shoot once for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                RoboMandoStats.Shoot_Damage.Value
            ));

            LanguageAPI.AddOverlay(tokenPrefix + "SECONDARY_ZAP_DESCRIPTION", string.Format(
                "Agile".Style(FontColor.cIsUtility) + ". " + "Stunning".Style(FontColor.cIsDamage) + ". Fire a small electrical charge that " + "pierces enemies ".Style(FontColor.cIsDamage) + "for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                RoboMandoStats.Zap_Damage.Value
            ));
            LanguageAPI.AddOverlay(tokenPrefix + "UTILITY_ROLL_DESCRIPTION", "Attempt to " + "dive forward ".Style(FontColor.cIsUtility) + "a small distance. You " + "cannot be hit ".Style(FontColor.cIsUtility) + "early in the maneuver.");
            LanguageAPI.AddOverlay(tokenPrefix + "SPECIAL_HACK_NAME", "Re-Wire");
            LanguageAPI.AddOverlay(tokenPrefix + "SPECIAL_HACK_DESCRIPTION", "Re-wire a mechanical object, " + "activating it for free".Style(FontColor.cIsUtility) + ".");

            SkillLocator allSkills = roboPrefab ? roboPrefab.GetComponent<SkillLocator>() : null;
            if (!allSkills) return;

            allSkills.primary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE"];
            allSkills.secondary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE", "KEYWORD_STUNNING"];
        }
        private static void ModifySkillStats()
        {
            SkillLocator allSkills = roboPrefab ? roboPrefab.GetComponent<SkillLocator>() : null;
            if (!allSkills) return;

            RobomandoMod.Survivors.Robomando.SkillStates.Shoot.damageCoefficient = RoboMandoStats.Shoot_Damage.Value / 100f;
            RobomandoMod.Survivors.Robomando.SkillStates.Shoot.procCoefficient = RoboMandoStats.Shoot_Coefficient.Value / 100f;

            RobomandoMod.Survivors.Robomando.SkillStates.Zap.damageCoefficient = RoboMandoStats.Zap_Damage.Value / 100f;
            RobomandoMod.Survivors.Robomando.SkillStates.Zap.procCoefficient = RoboMandoStats.Zap_Coefficient.Value / 100f;
            allSkills.secondary.skillFamily.defaultSkillDef.baseRechargeInterval = RoboMandoStats.Zap_Cooldown.Value;

            RobomandoMod.Survivors.Robomando.SkillStates.Roll.crashDuration = RoboMandoStats.Dive_Splat_Duration.Value;
            allSkills.utility.skillFamily.defaultSkillDef.baseRechargeInterval = RoboMandoStats.Dive_Cooldown.Value;

            RobomandoMod.Survivors.Robomando.SkillStates.Hack.BaseDuration = RoboMandoStats.Hack_Duration.Value;
            RobomandoMod.Survivors.Robomando.SkillStates.Hack.soundDuration = RoboMandoStats.Hack_Duration.Value * (2f/3f);
            allSkills.special.skillFamily.defaultSkillDef.baseRechargeInterval = RoboMandoStats.Hack_Cooldown.Value;
            allSkills.special.skillFamily.defaultSkillDef.cancelSprintingOnActivation = false;

            RoboMandoStats.Hack_NoTarget_Cooldown.Value = Math.Min(RoboMandoStats.Hack_NoTarget_Cooldown.Value, RoboMandoStats.Hack_Cooldown.Value);

            new ILHook(typeof(RobomandoMod.Survivors.Robomando.SkillStates.Hack).GetMethod(nameof(RobomandoMod.Survivors.Robomando.SkillStates.Hack.OnEnter)), ChangeHackCooldown);
            new ILHook(typeof(RobomandoMod.Survivors.Robomando.SkillStates.Hack).GetMethod(nameof(RobomandoMod.Survivors.Robomando.SkillStates.Hack.FixedUpdate)), ChangeHackFailCooldown);
        }
        private static void ModifyBodyStats()
        {
            CharacterBody roboBody = roboPrefab.GetComponent<CharacterBody>();
            if (!roboBody) return;

            var conversionRate = RoboMandoStats.Health_Shield_Percent.Value / 100f;

            roboBody.baseMaxHealth = 80f * (1f - conversionRate);
            roboBody.levelMaxHealth = 28f * (1f - conversionRate);
            // Cursed
            roboBody.baseMaxShield = 80f * (conversionRate);
            roboBody.levelMaxShield = 28f * (conversionRate);

            roboBody.baseRegen = 3f;
            roboBody.levelRegen = 0.3f;

            roboBody.baseDamage = 15f;
            roboBody.levelDamage = 3.2f;

            roboBody.baseArmor = 0f;

            roboBody.baseMoveSpeed = 9f;
        }
        private static void ChangeHackCooldown(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdcR4(out _),
                x => x.MatchStfld<GenericSkill>(nameof(GenericSkill.finalRechargeInterval))
            ))
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldc_R4, RoboMandoStats.Hack_Cooldown.Value);
            }
            else
            {
                Log.Warning("Failed to hook Change Hack Cooldown");
            }
        }
        private static void ChangeHackFailCooldown(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (cursor.TryGotoNext(
                x => x.MatchLdcR4(out _),
                x => x.MatchStfld<GenericSkill>(nameof(GenericSkill.finalRechargeInterval))
            ))
            {
                cursor.Remove();
                cursor.Emit(OpCodes.Ldc_R4, RoboMandoStats.Hack_NoTarget_Cooldown.Value);
            }
            else
            {
                Log.Warning("Failed to hook Change Hack Fail Cooldown");
            }
        }

        public static List<string> whitelistSkins = [];
    }
}
