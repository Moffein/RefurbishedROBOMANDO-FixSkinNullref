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
using RobomandoMod.Survivors.Robomando;
using RobomandoMod.Survivors.Robomando.SkillStates;
[assembly: HG.Reflection.SearchableAttribute.OptIn]

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
        public const string PluginVersion = "1.1.1";

        internal static RefurbishedRoboMando Instance { get; private set; }
        private static AssetBundle assetBundle;

        private static readonly string roboBodyName = "RobomandoBody";
        private static readonly string tokenPrefix = "RAT_ROBOMANDO_";

        private static GameObject roboPrefab;
        public void Awake()
        {
            Log.Init(Logger);
            Configs.SetUp(this);
            Instance = this;

            assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(Info.Location).ToString(), "robomandoasset"));
            RoR2Application.onLoad += ReplaceLast;
        }

        private static void ReplaceLast()
        {
            if (RoboMandoToggle.Enable_Icon_Change.Value) ReplaceIcon();
            if (RoboMandoToggle.Enable_Skin_Disable.Value) DisableSkins();
            if (RoboMandoToggle.Enable_Token_Changes.Value) AddLanguageTokens();
            if (RoboMandoToggle.Enable_Skill_Stat_Changes.Value) ModifySkillStats();
            if (RoboMandoToggle.Enable_Body_Stat_Changes.Value) ModifyBodyStats();
            if (RoboMandoToggle.Enable_Jury_Rework.Value) JuryRework();

            if (RoboMandoToggle.Enable_Logbook_Change.Value)
            {
                GameObject model = roboPrefab.GetComponent<ModelLocator>().modelTransform.gameObject;
                model.AddComponent<ReplaceLogbook>();
            }
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
            if (RoboMandoToggle.Enable_Skill_Stat_Changes.Value) SetVanilla();

            LanguageAPI.AddOverlay(tokenPrefix + "NAME", "ROBOMANDO");
            LanguageAPI.AddOverlay(tokenPrefix + "DESCRIPTION",
                "Your ROBOMANDO Model-7 comes equipped with everything it needs to extract resources from hostile environments.<color=#CCD3E0>" + 
                "\n\n< ! > As weak as Single Fire is, it has no fall-off damage." +
                "\n\n< ! > Despite the short range on De-Escalate, you can utilize it to pierce through multiple enemies at once, stunning them all in series." + 
                "\n\n< ! > Re-Wire allows instant access to any company sanctioned containers or drone utilities - make up your lack of strength and durability through stealing everything!" +
                "\n\n< ! > Remember that ROBOMANDO's skills aren't the most effective against enemies - use your increased movement speed to maintain distance accordingly."
            );

            LanguageAPI.AddOverlay(tokenPrefix + "LORE",
                "Order: ROBOMANDO Model-7" +
                "\r\nTracking Number: 45133554F44495351454" +
                "\r\n\r\nT  H  A  N  K    Y  O  U    F  O  R    P  U  R  C  H  A  S  I  N  G    T  H  I  S    R  O  B  O  M  A  N  D  O    M  O  D  E  L   -  7  .".Style(FontColor.cStack) +
                "\r\nI  N  S  T  R  U  C  T  I  O  N  S    A  R  E    I  N  C  L  U  D  E  D     I  N    A    S  E  P  E  R  A  T  E    P  A  C  K  A  G  E  .".Style(FontColor.cStack) +
                "\r\n\r\n... Damn."
            );

            LanguageAPI.AddOverlay(tokenPrefix + "PRIMARY_SHOT_DESCRIPTION", string.Format(
                "Agile".Style(FontColor.cIsUtility) + ". Shoot once for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                RoboMandoStats.Shoot_Damage
            ));

            LanguageAPI.AddOverlay(tokenPrefix + "SECONDARY_ZAP_DESCRIPTION", string.Format(
                "Agile".Style(FontColor.cIsUtility) + ". " + "Stunning".Style(FontColor.cIsDamage) + ". Fire a small electrical charge that " + "pierces enemies ".Style(FontColor.cIsDamage) + "for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                RoboMandoStats.Zap_Damage
            ));
            LanguageAPI.AddOverlay(tokenPrefix + "UTILITY_ROLL_DESCRIPTION", "Attempt to " + "dive forward ".Style(FontColor.cIsUtility) + "a small distance. You " + "cannot be hit ".Style(FontColor.cIsUtility) + "early in the maneuver.");
            string keyWord = RoboMandoToggle.Enable_Jury_Rework.Value ? "Sabotage" : "Jury-Rig";
            LanguageAPI.AddOverlay(tokenPrefix + "SPECIAL_HACK_DESCRIPTION", string.Format(
                "{0}".Style(FontColor.cIsUtility) + ". Re-wire a mechanical object, " + "activating it for free".Style(FontColor.cIsUtility) + ".", keyWord
            ));

            SkillLocator allSkills = roboPrefab ? roboPrefab.GetComponent<SkillLocator>() : null;
            if (!allSkills) return;

            allSkills.primary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE"];
            allSkills.secondary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE", "KEYWORD_STUNNING"];
        }
        private static void ModifySkillStats()
        {
            SkillLocator allSkills = roboPrefab ? roboPrefab.GetComponent<SkillLocator>() : null;
            if (!allSkills) return;

            RobomandoStaticValues.shootDamageCoefficient = RoboMandoStats.Shoot_Damage / 100f;
            RobomandoStaticValues.shootProcCoefficient = RoboMandoStats.Shoot_Coefficient;

            RobomandoStaticValues.zapDamageCoefficient = RoboMandoStats.Zap_Damage / 100f;
            RobomandoStaticValues.zapProcCoefficient = RoboMandoStats.Zap_Coefficient;
            RobomandoStaticValues.zapCooldown = RoboMandoStats.Zap_Cooldown;

            RobomandoStaticValues.diveCrashTime = RoboMandoStats.Dive_Splat_Duration;
            RobomandoStaticValues.diveCooldown = RoboMandoStats.Dive_Cooldown;

            RobomandoStaticValues.hackTime = RoboMandoStats.Hack_Duration;
            RobomandoStaticValues.successfullHackCooldown = RoboMandoStats.Hack_Cooldown;
            RobomandoStaticValues.unsuccessfullHackCooldown = RoboMandoStats.Hack_NoTarget_Cooldown;
            allSkills.special.skillFamily.defaultSkillDef.baseRechargeInterval = RoboMandoStats.Hack_Cooldown;
        }
        private static void ModifyBodyStats()
        {
            CharacterBody roboBody = roboPrefab.GetComponent<CharacterBody>();
            if (!roboBody) return;

            var conversionRate = RoboMandoStats.Health_Shield_Percent.Value / 100f;

            roboBody.baseMaxHealth = Math.Max(80f * (1f - conversionRate), 1);
            roboBody.levelMaxHealth = 28f * (1f - conversionRate);

            roboBody.baseMaxShield = Math.Min(80f * (conversionRate), 79);
            roboBody.levelMaxShield = 28f * (conversionRate);

            roboBody.baseRegen = 3f;
            roboBody.levelRegen = 0.3f;

            roboBody.baseDamage = 15f;
            roboBody.levelDamage = 3.2f;

            roboBody.baseArmor = 0f;

            roboBody.baseMoveSpeed = 9f;
        }
        private static void JuryRework()
        {
            LanguageAPI.AddOverlay(tokenPrefix + "KEYWORD_JURY_RIG", "Sabotage".Style(FontColor.cKeywordName) + "Used on a Printer will ".Style(FontColor.cSub) + "activate it for free".Style(FontColor.cIsUtility) + ", but ".Style(FontColor.cSub) + "break it".Style(FontColor.cIsHealth) + ". Costs ".Style(FontColor.cSub) + "X% ".Style(FontColor.cIsHealth) + "(".Style(FontColor.cSub) + "50%" + "/".Style(FontColor.cSub) + "75%".Style(FontColor.cIsHealing) + "/" + "90%".Style(FontColor.cIsHealth) + "/" + "95%".Style(FontColor.cIsDamage) + ") ".Style(FontColor.cSub) + "of your current health".Style(FontColor.cIsHealth) + ".".Style(FontColor.cSub));
            if (!RoboMandoToggle.Enable_Token_Changes.Value) LanguageAPI.AddOverlay(tokenPrefix + "SPECIAL_HACK_DESCRIPTION", "Jury-Rig".Style(FontColor.cIsUtility) + ". Re-wire a nearby mechanical object, " + "activating it for free".Style(FontColor.cIsUtility) + ".");

            new ILHook(typeof(RobomandoMod.Survivors.Robomando.SkillStates.Hack).GetMethod(nameof(RobomandoMod.Survivors.Robomando.SkillStates.Hack.HackDevice)), HackModify);
        }

        private static void HackModify(ILContext il)
        {
            var cursor = new ILCursor(il);
            var pickupIndex = -1;

            if (cursor.TryGotoNext(
                x => x.MatchStloc(out pickupIndex),
                x => x.MatchLdarg(2),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchSwitch(out _)
            )) { } else Log.Warning("Failed to hook item index");

            if (cursor.TryGotoNext(
                x => x.MatchLdarg(2),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchStloc(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchSwitch(out _)
            ))
            {
                cursor.Emit(OpCodes.Ldloc, pickupIndex);
                cursor.Emit(OpCodes.Ldarg, 0);
                cursor.Emit(OpCodes.Ldarg, 1);
                cursor.Emit(OpCodes.Ldarg, 2);

                cursor.EmitDelegate<Func<PickupIndex, GameObject, GameObject, Hack.PrinterItemType, PickupIndex>>((self, robo, interact, type) =>
                {
                    var basicIndex = PickupIndex.none;

                    if (type != Hack.PrinterItemType.NONE)
                    {
                        HealthComponent roboHealth = robo.GetComponent<HealthComponent>();
                        ShopTerminalBehavior component = interact.GetComponent<ShopTerminalBehavior>();

                        basicIndex = component.pickupIndex;
                        DamageInfo damageInfo = new()
                        {
                            damageType = (DamageType.BypassArmor | DamageType.NonLethal),
                            procCoefficient = 0
                        };

                        if (type == Hack.PrinterItemType.WHITE) damageInfo.damage = roboHealth.fullCombinedHealth * 0.5f;
                        if (type == Hack.PrinterItemType.GREEN) damageInfo.damage = roboHealth.fullCombinedHealth * 0.75f;
                        if (type == Hack.PrinterItemType.RED) damageInfo.damage = roboHealth.fullCombinedHealth * 0.9f;
                        if (type == Hack.PrinterItemType.YELLOW) damageInfo.damage = roboHealth.fullCombinedHealth * 0.95f;
                        
                        roboHealth.TakeDamage(damageInfo);
                    }

                    return basicIndex;
                });
                cursor.Emit(OpCodes.Stloc, pickupIndex);

                var previousIndex = cursor.Index;

                if (cursor.TryGotoNext(
                    x => x.MatchCallOrCallvirt(typeof(UnityEngine.Vector3), "get_zero"),
                    x => x.MatchStloc(out _)
                ))
                {
                    var skipLabel = cursor.MarkLabel();
                    cursor.Goto(previousIndex);
                    cursor.Emit(OpCodes.Br, skipLabel.Target);
                }
            }
            else
            {
                Log.Warning("Failed to hook scrap replacement");
            }
        }
        private static void SetVanilla()
        {
            RoboMandoStats.Shoot_Damage = 100 * RobomandoStaticValues.shootDamageCoefficient;
            RoboMandoStats.Zap_Damage = 100 * RobomandoStaticValues.zapDamageCoefficient;
        }

        public static List<string> whitelistSkins = [];
    }
    public class ReplaceLogbook : MonoBehaviour
    {
        private ModelSkinController component;
        private static int skinIndex;
        private void Awake()
        {
            skinIndex = 0;
            component = GetComponent<ModelSkinController>();
            CharacterBody characterBody = BodyCatalog.FindBodyPrefab("RobomandoBody").GetComponent<CharacterBody>();
            if (characterBody)
            {
                SkinDef[] skins = component.GetComponent<ModelSkinController>().skins;
                foreach (SkinDef skinDef in skins)
                {
                    if (!skinDef.nameToken.Equals("NOODLEGEMO_SKIN_ROBOMANDO_NAME")) skinIndex++;
                    break;
                }
                
            }
        }
        private void Start()
        {
            if (SceneCatalog.currentSceneDef.cachedName == "logbook") component.ApplySkin(skinIndex);
        }
    }
}
