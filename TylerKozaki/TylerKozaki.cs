using BepInEx;
using BepInEx.Logging;
using FP2Lib.Badge;
using FP2Lib.Item;
using FP2Lib.Player;
using FP2Lib.Vinyl;
using HarmonyLib;
using System.IO;
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
            logSource = base.Logger;

            string assetPath = Path.Combine(Path.GetFullPath("."), "mod_overrides\\TylerMod");
            dataBundle = AssetBundle.LoadFromFile(Path.Combine(assetPath, "playabletyler.assets"));
            //tutorialScene = AssetBundle.LoadFromFile(Path.Combine(assetPath, "tutorialtyler.scene"));

            if (dataBundle == null) //|| tutorialScene == null)
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
            BadgeHandler.RegisterBadge("kubo.tylerrunner", "Shade Runner", "Beat any stage's par time as Tyler.", null, FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.tylerspeedrunner", "Shadow Speedrunner", "Beat any stage as Tyler in less than half of the par time.", null, FPBadgeType.SILVER);
            BadgeHandler.RegisterBadge("kubo.tylermaster", "Umbral Master", "Beat the par times in all stages as Tyler.", null, FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylercomplete", "Avalice’s Own War Dog", "Finish the game as Tyler.", null, FPBadgeType.GOLD);

            BadgeHandler.RegisterBadge("kubo.tylertailkill", "Eat my Tail", "Beat a boss with Umbral Tail Spin.", null, FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylershadowkill", "Shadow Arts", "Beat a boss with Umbral Throw special (Kunai, Blade, or Eclipse Bomb).", null, FPBadgeType.GOLD);
            BadgeHandler.RegisterBadge("kubo.tylerboostkill", "Howling Shadows", "Beat a boss with umbral boost or the umbral boost overdrive.", null, FPBadgeType.GOLD);

            //Add Items
            ItemHandler.RegisterItem("kubo.tylermemento", "Kozaki Family Bracelet", null, "A priceless heirloom passed down from Tyler's family.\nThe item grants him a second chance during a stage.\nDespite the odds, he shall endure.", IAddToShop.None,0,0,0);
            familyBraceletID = (FPPowerup)ItemHandler.GetItemDataByUid("kubo.tylermemento").itemID;
            

            //Load character select object
            PlayableChara tylerChar = new PlayableChara()
            {
                uid = "com.kuborro.tyler",
                Name = "Tyler",
                TutorialScene = "Tutorial1",
                characterType = "SHADOW Type",
                skill1 = "Fly",
                skill2 = "Double Jump",
                skill3 = "Shoot",
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
                powerupStartDescription = "Start the stage with your Family Bracelet ready.",
                profilePic = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler_Profile")[0],
                keyArtSprite = dataBundle.LoadAsset<Sprite>("Tyler_KeyArt"),
                endingKeyArtSprite = dataBundle.LoadAsset<Sprite>("Tyler_EndingArt"),
                charSelectName = dataBundle.LoadAsset<Sprite>("Tyler-File-Select"),
                piedSprite = (Sprite)dataBundle.LoadAssetWithSubAssets("Tyler_Pie")[0],
                piedHurtSprite = (Sprite)dataBundle.LoadAssetWithSubAssets("Tyler_Pie")[1],
                itemFuel = dataBundle.LoadAsset<Sprite>("ItemFuelCrystal"),
                worldMapPauseSprite = dataBundle.LoadAsset<Sprite>("tyler_Pause"),
                zaoBaseballSprite = dataBundle.LoadAsset<Sprite>("TylerZLBall"),
                livesIconAnim = dataBundle.LoadAssetWithSubAssets<Sprite>("Tyler_Stock"),
                sagaBlock = dataBundle.LoadAsset<RuntimeAnimatorController>("SagaTyler"),
                sagaBlockSyntax = dataBundle.LoadAsset<RuntimeAnimatorController>("Saga2Tyler"),
                resultsTrack = tylerClear,
                endingTrack = tylerTheme,
                playerBoss = null,
                menuPhotoPose = new MenuPhotoPose(),
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
            harmony.PatchAll(typeof(PatchFPHudMaster));
            harmony.PatchAll(typeof(PatchFPPlayer));
            harmony.PatchAll(typeof(PatchFPResultsMenu));
            harmony.PatchAll(typeof(PatchFPSaveManager));
            harmony.PatchAll(typeof(PatchMenuClassic));
            harmony.PatchAll(typeof(PatchMenuWorldMap));
        }
    }
}
