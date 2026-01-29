using BepInEx;
using BepInEx.Logging;
using FP2Lib.Badge;
using FP2Lib.Item;
using FP2Lib.Player;
using FP2Lib.Vinyl;
using HarmonyLib;
using System.IO;
using System.Linq;
using TylerKozaki.Patches;
using UnityEngine;

namespace TylerKozaki
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class TylerKozaki : BaseUnityPlugin
    {
        internal static ManualLogSource logSource;

        public static AssetBundle dataBundle;
        public static AssetBundle tutorialScene;

        internal static FPCharacterID currentTylerID;
        internal static FPPowerup familyBraceletID;

        private void Awake()
        {
            // Plugin startup logic
            logSource = Logger;

            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides\\TylerKozaki");
            dataBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "tylerKozaki.assets"));

            if (dataBundle == null)
            {
                logSource.LogError("Failed to load AssetBundles! This mod cannot work without them, exiting. Please reinstall it.");
                return;
            }

            //Initialise music
            AudioClip tylerClear = dataBundle.LoadAsset<AudioClip>("m_results_tyler");
            AudioClip tylerTheme = dataBundle.LoadAsset<AudioClip>("m_theme_tyler");

            //Add Vinyls
            VinylHandler.RegisterVinyl("kubo.m_clear_tyler", "Results - Tyler", tylerClear, VAddToShop.Naomi);
            VinylHandler.RegisterVinyl("kubo.m_theme_tyler", "Tyler's Theme", tylerTheme, VAddToShop.Fawnstar);

            //Add Badges
            BadgeHandler.RegisterBadge("kubo.tylerrunner", "Shade Runner", "Beat any stage's par time as Tyler.", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[4], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.tylerspeedrunner", "Shadow Speedrunner", "Beat any stage as Tyler in less than half of the par time.", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[5], FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.tylermaster", "Umbral Master", "Beat the par times in all stages as Tyler.", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[6], FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylercomplete", "Avalice’s Own War Dog", "Finish the game as Tyler.", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[7], FPBadgeType.GOLD);

            BadgeHandler.RegisterBadge("kubo.tylertailkill", "Eat my Tail", "Beat a boss with Umbral Tail Spin.", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[2], FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylershadowkill", "Shadow Arts", "Beat a boss with Umbral Throw special (Kunai, Blade, or Eclipse Bomb).", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[1], FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylerboostkill", "Howling Shadows", "Beat a boss with Umbral Boost", dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's badges")[0], FPBadgeType.GOLD);

            //Some special cases
            Sprite[] worldMapIdle = [null];
            Sprite[] worldMapWalk = [null];
            Sprite[] worldMapAnims = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's adventure mode chibi");

            if (worldMapAnims != null && worldMapAnims.Length > 7)
            {
                worldMapIdle = [.. worldMapAnims.Take(7)];
                worldMapWalk = [.. worldMapAnims.Skip(7)];
            }

            MenuPhotoPose menuPhotoPose = new MenuPhotoPose();
            menuPhotoPose.groundSprites = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's photo mode");
            menuPhotoPose.airSprites = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's air Photo mode");

            //Load character select object
            PlayableChara tylerChar = new PlayableChara()
            {
                uid = "com.kuborro.tyler",
                Name = "Tyler",
                TutorialScene = "Tutorial1",
                characterType = "SHADOW Type",
                skill1 = "Umbral Boost",
                skill2 = "Tail Spin",
                skill3 = "Attack",
                skill4 = "Guard",
                airshipSprite = 1,
                useOwnCutsceneActivators = false,
                enabledInAventure = false,
                enabledInClassic = true,
                AirMoves = PatchFPPlayer.Action_Tyler_AirMoves,
                GroundMoves = PatchFPPlayer.Action_Tyler_GroundMoves,
                ItemFuelPickup = PatchFPPlayer.Action_Tyler_FuelPickup,
                eventActivatorCharacter = FPCharacterID.LILAC,
                Gender = CharacterGender.MALE,
                element = CharacterElement.FIRE,
                powerupStartDescription = "A priceless heirloom passed down from Tyler's family.\nThe item grants him a second chance during a stage.",
                profilePic = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's dialogue box icon")[0],
                keyArtSprite = dataBundle.LoadAssetWithSubAssets<Sprite>("character select and game clear")[0],
                endingKeyArtSprite = dataBundle.LoadAssetWithSubAssets<Sprite>("character select and game clear")[1],
                charSelectName = dataBundle.LoadAsset<Sprite>("Tylers_character_select_name"),
                piedSprite = dataBundle.LoadAsset<Sprite>("Tyler's stuck in pie"),
                piedHurtSprite = dataBundle.LoadAsset<Sprite>("Tyler's pie break"),
                itemFuel = dataBundle.LoadAsset<Sprite>("Wolf dragon pendent"),
                worldMapPauseSprite = dataBundle.LoadAsset<Sprite>("TylerPauseIdle"),
                zaoBaseballSprite = dataBundle.LoadAsset<Sprite>("Tyler's baseball hold on sprite"),
                livesIconAnim = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler's lifes counter icon"),
                sagaBlock = dataBundle.LoadAsset<RuntimeAnimatorController>("SagaTyler"),
                sagaBlockSyntax = dataBundle.LoadAsset<RuntimeAnimatorController>("Saga2Tyler"),
                worldMapIdle = worldMapIdle,
                worldMapWalk = worldMapWalk,
                resultsTrack = tylerClear,
                endingTrack = tylerTheme,
                playerBoss = dataBundle.LoadAsset<GameObject>("Boss Tyler").GetComponent<PlayerBoss>(),
                menuPhotoPose = menuPhotoPose,
                characterSelectPrefab = dataBundle.LoadAsset<GameObject>("Menu CS Character Tyler"),
                menuInstructionPrefab = dataBundle.LoadAsset<GameObject>("MenuInstructionsTyler"),
                prefab = dataBundle.LoadAsset<GameObject>("Player Tyler"),
                dataBundle = dataBundle
            };

            if (PlayerHandler.RegisterPlayableCharacterDirect(tylerChar))
            {
                currentTylerID = (FPCharacterID)PlayerHandler.GetPlayableCharaByUid(tylerChar.uid).id;
            }
            else
            {
                logSource.LogError("Something went very wrong when registering the character! Things are broken!");
            }

            Harmony harmony = new Harmony("com.kuborro.plugins.fp2.playabletyler");
            harmony.PatchAll(typeof(PatchAnimatorPreInitializer));
            harmony.PatchAll(typeof(PatchFPEventSequence));
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchFPBossHud));
            harmony.PatchAll(typeof(PatchFPBaseEnemy));
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchMenuClassic));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
            harmony.PatchAll(typeof(PatchProjectileBasic));
            harmony.PatchAll(typeof(PatchItemFuel));
            harmony.PatchAll(typeof(PatchZLBaseballFlyer));
        }
    }
}
