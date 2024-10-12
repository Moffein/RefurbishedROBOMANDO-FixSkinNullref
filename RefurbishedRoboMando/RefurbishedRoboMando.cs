using BepInEx;
using R2API;
using RoR2;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using RobomandoMod.Survivors.Robomando;

using static RefurbishedRoboMando.ColorCode;

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
        public const string PluginVersion = "1.2.1";
        public void Awake()
        {
            Log.Init(Logger);
            new AssetStatics(this);
            new Configs();

            BodyCatalog.availability.CallWhenAvailable(SetUpOnBodyLoad);
            RoR2Application.onLoad += SetUpLast;
        }
        private static void SetUpOnBodyLoad()
        {
            if (!AssetStatics.bodyComponent) return;

            if (RefurbishConfigEntry.Icon_Direction.Value == Configs.IconDirection.LEFT) AssetStatics.bodyComponent.portraitIcon = AssetStatics.bundle.LoadAsset<Sprite>("RobomandoIcon").texture;
            else AssetStatics.bodyComponent.portraitIcon = AssetStatics.bundle.LoadAsset<Sprite>("RobomandoIconAlt").texture;
        }
        private static void SetUpLast()
        {
            SkinOrder();
            new HiddenSkin();

            if (AssetStatics.modSkins != null)
            {
                SkinDef securedSkin = AssetStatics.modSkins.Find(self => self.nameToken.Equals(AssetStatics.skinTokens[1]));
                securedSkin.unlockableDef = RobomandoUnlockables.masterySkinUnlockableDef;
            }

            if (RefurbishConfigEntry.Replace_Logbook_Model.Value)
            {
                GameObject model = AssetStatics.bodyPrefab.GetComponent<ModelLocator>().modelTransform.gameObject;
                model.AddComponent<ReplaceCurrentSkin>();
            }
            if (RefurbishConfigEntry.Replace_Logbook_Lore.Value)
            {
                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "LORE",
                    "Order: ROBOMANDO Model-7" +
                    "\r\nTracking Number: 45133554F44495351454" +
                    "\r\n\r\nT  H  A  N  K    Y  O  U    F  O  R    P  U  R  C  H  A  S  I  N  G    T  H  I  S    R  O  B  O  M  A  N  D  O    M  O  D  E  L   -  7  .".Style(FontColor.cStack) +
                    "\r\nI  N  S  T  R  U  C  T  I  O  N  S    A  R  E    I  N  C  L  U  D  E  D     I  N    A    S  E  P  E  R  A  T  E    P  A  C  K  A  G  E  .".Style(FontColor.cStack) +
                    "\r\n\r\n... Damn."
                );
            }

            if (RefurbishConfigEntry.Replace_Tokens.Value)
            {
                if (!RefurbishConfigEntry.Replace_Skill_Stats.Value)
                {
                    PresetSkillStats.Shoot_Damage = 100 * RobomandoStaticValues.shootDamageCoefficient;
                    PresetSkillStats.Zap_Damage = 100 * RobomandoStaticValues.zapDamageCoefficient;
                };

                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "NAME", "ROBOMANDO");
                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "DESCRIPTION",
                    "Your ROBOMANDO Model-7 comes equipped with everything it needs to extract resources from hostile environments.<color=#CCD3E0>" +
                    "\n\n< ! > As weak as Single Fire is, it has no fall-off damage." +
                    "\n\n< ! > Despite the short range on De-Escalate, you can utilize it to pierce through multiple enemies at once, stunning them all in series." +
                    "\n\n< ! > Re-Wire allows instant access to any company sanctioned containers or drone utilities - make up your lack of strength and durability through stealing everything!" +
                    "\n\n< ! > Remember that ROBOMANDO's skills aren't the most effective against enemies - use your increased movement speed to maintain distance accordingly."
                );

                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "PRIMARY_SHOT_DESCRIPTION", string.Format(
                    "Agile".Style(FontColor.cIsUtility) + ". Shoot once for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                    PresetSkillStats.Shoot_Damage
                ));

                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "SECONDARY_ZAP_DESCRIPTION", string.Format(
                    "Agile".Style(FontColor.cIsUtility) + ". " + "Stunning".Style(FontColor.cIsDamage) + ". Fire a small electrical charge that " + "pierces enemies ".Style(FontColor.cIsDamage) + "for " + "{0}% damage".Style(FontColor.cIsDamage) + ".",
                    PresetSkillStats.Zap_Damage
                ));

                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "UTILITY_ROLL_DESCRIPTION", "Attempt to " + "dive forward ".Style(FontColor.cIsUtility) + "a small distance. You " + "cannot be hit ".Style(FontColor.cIsUtility) + "early in the maneuver.");

                string keyWord = RefurbishConfigEntry.Jury_Rework.Value ? "Sabotage" : "Jury-Rig";
                LanguageAPI.AddOverlay(AssetStatics.tokenPrefix + "SPECIAL_HACK_DESCRIPTION", string.Format(
                    "{0}".Style(FontColor.cIsUtility) + ". Re-wire a mechanical object, " + "activating it for free".Style(FontColor.cIsUtility) + ".", keyWord
                ));

                SkillLocator allSkills = AssetStatics.bodyPrefab ? AssetStatics.bodyPrefab.GetComponent<SkillLocator>() : null;
                if (!allSkills) return;

                allSkills.primary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE"];
                allSkills.secondary.skillFamily.defaultSkillDef.keywordTokens = ["KEYWORD_AGILE", "KEYWORD_STUNNING"];
            }

            if (RefurbishConfigEntry.Replace_Skill_Stats.Value) StatChanges();
            if (RefurbishConfigEntry.Replace_Body_Stats.Value) BodyChanges();
            if (RefurbishConfigEntry.Jury_Rework.Value) new JuryRework();
        }
        private static void SkinOrder()
        {
            if (!AssetStatics.bodyComponent) return;

            List<SkinDef> modSkins = [];
            List<SkinDef> hiddenSkins = [];
            List<SkinDef> origSkins = [];
            List<SkinDef> skinOrder = [];

            foreach (SkinDef skinDef in BodyCatalog.GetBodySkins(AssetStatics.bodyComponent.bodyIndex))
            {
                if (!skinDef) continue;

                if (RefurbishConfigEntry.Disable_Skins.Value)
                {
                    bool doSkip = false;
                    foreach (string prefix in AssetStatics.skinPrefixBlacklist)
                    {
                        if (skinDef.nameToken.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        {
                            doSkip = true;
                            break;
                        }
                    }
                    if (doSkip) continue;
                }

                if (AssetStatics.hiddenTokens.Contains(skinDef.nameToken)) hiddenSkins.Add(skinDef);
                else if (AssetStatics.skinTokens.Contains(skinDef.nameToken)) modSkins.Add(skinDef);
                else origSkins.Add(skinDef);
            }

            if (RefurbishConfigEntry.Skin_Order.Value == Configs.SkinOrder.FIRST) { skinOrder.AddRange(modSkins); skinOrder.AddRange(origSkins); }
            else { skinOrder.AddRange(origSkins); skinOrder.AddRange(modSkins); }

            BodyCatalog.skins[(int)AssetStatics.bodyComponent.bodyIndex] = [.. skinOrder];
            SkinCatalog.skinsByBody[(int)AssetStatics.bodyComponent.bodyIndex] = [.. skinOrder];

            skinOrder.AddRange(hiddenSkins);

            ModelLocator modelLocator = AssetStatics.bodyPrefab.GetComponent<ModelLocator>();
            if (modelLocator)
            {
                Log.Debug("Has model locator");
                ModelSkinController skinController = modelLocator.modelTransform.gameObject.GetComponent<ModelSkinController>();
                if (skinController) skinController.skins = [.. skinOrder];
            }
        }
        private static void StatChanges()
        {
            SkillLocator allSkills = AssetStatics.bodyPrefab ? AssetStatics.bodyPrefab.GetComponent<SkillLocator>() : null;
            if (!allSkills) return;

            RobomandoStaticValues.shootDamageCoefficient = PresetSkillStats.Shoot_Damage / 100f;
            RobomandoStaticValues.shootProcCoefficient = PresetSkillStats.Shoot_Coefficient;

            RobomandoStaticValues.zapDamageCoefficient = PresetSkillStats.Zap_Damage / 100f;
            RobomandoStaticValues.zapProcCoefficient = PresetSkillStats.Zap_Coefficient;
            RobomandoStaticValues.zapCooldown = PresetSkillStats.Zap_Cooldown;

            RobomandoStaticValues.diveCrashTime = PresetSkillStats.Dive_Splat_Duration;
            RobomandoStaticValues.diveCooldown = PresetSkillStats.Dive_Cooldown;

            RobomandoStaticValues.hackTime = PresetSkillStats.Hack_Duration;
            RobomandoStaticValues.successfullHackCooldown = PresetSkillStats.Hack_Cooldown;
            RobomandoStaticValues.unsuccessfullHackCooldown = PresetSkillStats.Hack_NoTarget_Cooldown;
            allSkills.special.skillFamily.defaultSkillDef.baseRechargeInterval = PresetSkillStats.Hack_Cooldown;
        }
        private static void BodyChanges()
        {
            if (!AssetStatics.bodyComponent) return;

            var conversionRate = RefurbishConfigEntry.Health_Convert.Value / 100f;

            AssetStatics.bodyComponent.baseMaxHealth = Math.Max(80f * (1f - conversionRate), 1);
            AssetStatics.bodyComponent.levelMaxHealth = 28f * (1f - conversionRate);

            AssetStatics.bodyComponent.baseMaxShield = Math.Min(80f * (conversionRate), 79);
            AssetStatics.bodyComponent.levelMaxShield = 28f * (conversionRate);

            AssetStatics.bodyComponent.baseRegen = 3f;
            AssetStatics.bodyComponent.levelRegen = 0.3f;

            AssetStatics.bodyComponent.baseDamage = 15f;
            AssetStatics.bodyComponent.levelDamage = 3.2f;

            AssetStatics.bodyComponent.baseArmor = 0f;

            AssetStatics.bodyComponent.baseMoveSpeed = 9f;
        }
    }
    public class ReplaceCurrentSkin : MonoBehaviour
    {
        private readonly System.Random random = new();
        private ModelSkinController component;
        private CharacterBody body;
        public bool infested;
        private void Awake()
        {
            component = GetComponent<ModelSkinController>();
            body = component.characterModel.body;
        }
        private void Start()
        {
            if (!component) return;
            if (SceneCatalog.currentSceneDef.cachedName == "logbook")
            {
                int listIndex = Util.CheckRoll(7.5f, 0, null) ? random.Next(1, AssetStatics.skinTokens.Count - 1) : 0;
                ChangeSkin(AssetStatics.skinTokens[listIndex]);
            }

            if (!body) return;
            SkinDef currentSkin = BodyCatalog.GetBodySkins(body.bodyIndex)[body.skinIndex];
            if (currentSkin.nameToken.Equals(AssetStatics.skinTokens[1]))
            {
                SetInfestedBool();
                if (infested) ChangeSkin(AssetStatics.skinTokens[2]);
            }
        }
        public void ChangeSkin(string skinToken)
        {
            int tempSkinIndex = 0;
            foreach (SkinDef skinDef in BodyCatalog.GetBodySkins(AssetStatics.bodyComponent.bodyIndex))
            {
                if (!skinDef) continue;
                if (skinDef.nameToken.Equals(skinToken)) break;
                tempSkinIndex++;
            }
            component.ApplySkin(tempSkinIndex);
        }
        private void SetInfestedBool()
        {
            CharacterBody body = component.characterModel.body;
            int itemCount = body.inventory ? body.inventory.GetItemCount(DLC1Content.Items.BearVoid) : 0;
            if (itemCount > 0) infested = true;
        }
    }
    public class AssetStatics
    {
        public static readonly string bodyInternal = "RobomandoBody";
        public static readonly string tokenPrefix = "RAT_ROBOMANDO_";

        public static BaseUnityPlugin plugin;
        public static CharacterBody bodyComponent;
        public static GameObject bodyPrefab;
        public static AssetBundle bundle;

        public static List<SkinDef> modSkins;
        public static readonly List<string> skinPrefixBlacklist = [
            "RAT_",
            "DEFAULT_SKIN"
        ];
        public static readonly List<string> skinTokens = [
            "NOODLEGEMO_SKIN_ROBOMANDO_NAME",
            "NOODLEGEMO_SKIN_MONSOONROBO_NAME",
            "NOODLEGEMO_SKIN_INFESTEDROBO_NAME"
        ];
        public static readonly List<string> hiddenTokens = [
            "NOODLEGEMO_SKIN_INFESTEDROBO_NAME"
        ];

        public AssetStatics(BaseUnityPlugin plugin)
        {
            AssetStatics.plugin = plugin;
            BodyCatalog.availability.CallWhenAvailable(SetUpOnBodyLoad);
            RoR2Application.onLoad += SetUpLast;
            bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Directory.GetParent(plugin.Info.Location).ToString(), "robomandoasset"));
        }
        private static void SetUpOnBodyLoad()
        {
            bodyPrefab = BodyCatalog.FindBodyPrefab(bodyInternal);
            bodyComponent = bodyPrefab ? bodyPrefab.GetComponent<CharacterBody>() : null;
        }
        private static void SetUpLast()
        {
            if (!bodyComponent) return;
            modSkins = [];

            foreach (SkinDef skinDef in BodyCatalog.GetBodySkins(bodyComponent.bodyIndex))
            {
                if (!skinDef) continue;
                if (skinTokens.Contains(skinDef.nameToken)) modSkins.Add(skinDef);
            }
        }
    }
}