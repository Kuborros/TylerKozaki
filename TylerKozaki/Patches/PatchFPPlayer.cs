using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Random;
using Random = UnityEngine.Random;

namespace TylerKozaki.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;
        //public static PlayerShadow playerShadow;

        public static AudioClip kunaiSfx;
        //public static AudioClip bladeThrowSfx;
        //public static AudioClip umbralBombSfx;
        //public static AudioClip chargeSfx;

        internal static float guardBuffer;
        internal static float jumpMultiplier;
        internal static float speedMultiplier;

        private static float attackCharge = 0f;
        private static float wallClingTimer = 0f;
        private static float blinkTimer = 0f;
        private static float energyRecoveryBaseSpeed = 0.4f;

        internal static float overCharge = 0f;
        internal static float throwCharge = 0f;
        internal static float throwDelay = 0f;
        internal static float chargeThrowDelay = 0f;

        private static readonly float kunaiDamage = 4f;
        private static readonly float bladeThrowDamage = 6f;
        private static readonly float umbralBombDamage = 8f;

        internal static bool blinkState = false;
        internal static bool burnoutState = false;
        internal static bool combo = false;

        private static int kunaiAngle = 0;
        private static int lastDamageType;

        private static RuntimeAnimatorController kunaiProjectile;
        //private static RuntimeAnimatorController bladeThrowProjectile;
        //private static RuntimeAnimatorController umbralBombProjectile;

        private static RuntimeAnimatorController darkSparkle;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Tyler_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Tyler_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Tyler_GroundMoves());

        //Actions

        internal static void Action_Tyler_ResetKunaiAngle()
        {
            kunaiAngle = 0;
        }

        internal static void Action_Tyler_FuelPickup()
        {
            player.hasSpecialItem = true;
        }

        internal static void Action_Tyler_AirMoves()
        {
            //Tail Spin
            if (player.input.jumpPress && player.state != new FPObjectState(State_Tyler_TailSpin) && ((player.targetWaterSurface == null && !player.jumpAbilityFlag) || (player.targetWaterSurface != null && player.input.down)))
            {
                if (player.input.down)
                {
                    player.velocity.y = 0f;
                }
                else if (player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                    player.velocity.y = Mathf.Max(player.velocity.y, 5f);
                }
                else
                {
                    player.velocity.y = Mathf.Max(player.velocity.y, 5f);
                }
                player.genericTimer = 0f;
                player.SetPlayerAnimation("Jumping");
                player.SetPlayerAnimation("TailSpin");
                player.state = State_Tyler_TailSpin;
                player.jumpAbilityFlag = true;
                player.attackStats = AttackStats_Tyler_TailSpin;
                player.Action_PlaySound(player.sfxCyclone);
                if (player.voiceTimer <= 0f)
                {
                    player.voiceTimer = 900f;
                    player.Action_PlayVoiceArray("SpecialA");
                }
            }
            //Guard
            else if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                player.SetPlayerAnimation("GuardAir");
                player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                //player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            //Air melee
            else if (player.state != new FPObjectState(State_Tyler_ClawDive) &&
            ((player.state != new FPObjectState(State_Tyler_EclipseFang) && ((FPSaveManager.holdToAttack >= 1 && player.input.attackHold) || player.input.attackPress)) || player.input.attackPress))
            {
                //Claw Dive (D)
                if (player.input.down)
                {
                    if (player.direction == FPDirection.FACING_LEFT)
                    {
                        player.velocity.x = Mathf.Min(player.velocity.x - 2f, -6f);
                    }
                    else
                    {
                        player.velocity.x = Mathf.Max(player.velocity.x + 2f, 6f);
                    }
                    player.velocity.y -= 5f;
                    player.genericTimer = 0f;
                    player.SetPlayerAnimation("ClawDive");
                    player.state = State_Tyler_ClawDive;
                    player.Action_StopSound();
                    player.Action_PlayVoiceArray("HeavyAttack");
                    player.Action_PlaySoundUninterruptable(player.sfxDivekick2);
                }
                //Eclipse Fang (U)
                else if (player.input.up && player.state != new FPObjectState(State_Tyler_EclipseFang))
                {
                    player.SetPlayerAnimation("EclipseFang");
                    player.state = State_Tyler_EclipseFang;
                    player.angle = 0f;
                    player.velocity.y += 5f;
                    player.jumpAbilityFlag = true;
                    player.Action_StopSound();
                    player.Action_PlayVoiceArray("HardAttack");
                }
                //Air Kick (LR)
                else
                {
                    player.SetPlayerAnimation("SnapKick");
                    player.state = State_Tyler_TailSwipe;
                    player.angle = 0f;
                    //player.velocity.y += 5f;
                    player.jumpAbilityFlag = true;
                    player.Action_StopSound();
                    player.Action_PlayVoiceArray("Attack");
                }
            }
            //Air Special
            else if (player.input.specialPress && player.state != new FPObjectState(State_Tyler_BoostP2) && player.energy == 100f)
            {
                player.velocity.x *= 0.5f;
                player.velocity.y *= 0.25f;
                player.genericTimer = 0f;
                player.specialAttackDirection = 2;
                player.boostCount++;
                player.damageToBadgeUnlock = 0f;
                player.recoveryTimer = player.velocity.x;
                player.state = State_Tyler_BoostP1;
                player.Action_PlaySoundUninterruptable(player.sfxBoostCharge);
            }
        }


        internal static void Action_Tyler_GroundMoves()
        {
            //Ground Guard
            if ((player.guardTime <= 0f || player.cancellableGuard) && (player.input.guardPress || (guardBuffer > 0f && player.input.guardHold)))
            {
                if (Mathf.Abs(player.groundVel) < 3f)
                {
                    player.SetPlayerAnimation("Guard", null, null, false, true);
                    player.idleTimer = Mathf.Min(player.idleTimer, 0f);
                    player.groundVel = 0f;
                }
                else
                {
                    player.SetPlayerAnimation("GuardRun", null, null, false, true);
                    player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                }
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                //player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            //Blink (Hold guard long)

            //Ground Melee
            //Base (EclipseCombo)
            else if (!player.input.down && !player.input.up && !player.input.jumpPress && player.state != new FPObjectState(State_Tyler_TailSwipe) &&
            ((player.state != new FPObjectState(State_Tyler_TailSpin) && ((FPSaveManager.holdToAttack >= 1 && player.input.attackHold) || player.input.attackPress)) || player.input.attackPress))
            {

            }
            //TailSwipe
            else if (player.input.down && player.input.attackHold && player.state == new FPObjectState(player.State_Crouching) && player.currentAnimation != "TailSwipe")
            {
                player.SetPlayerAnimation("TailSwipe");
                player.state = State_Tyler_TailSwipe;
                combo = false;
                player.idleTimer = 0f - player.fightStanceTime;
                player.genericTimer = -20f;
                player.Action_PlayVoiceArray("Attack");
            }
            //Uppercut (EclipseFang)
            else if (player.input.up && !player.input.jumpPress && player.state != new FPObjectState(State_Tyler_TailSwipe) &&
            ((player.state != new FPObjectState(State_Tyler_TailSpin) && ((FPSaveManager.holdToAttack >= 1 && player.input.attackHold) || player.input.attackPress)) || player.input.attackPress))
            {

            }
            //Ground Special (UmbralBoost/Kunai)
            else if (player.input.specialPress && player.state != new FPObjectState(State_Tyler_BoostP2) && player.energy == 100f)
            {
                player.genericTimer = 0f;
                player.specialAttackDirection = 2;
                player.damageToBadgeUnlock = 0f;
                player.recoveryTimer = player.groundVel;
                player.state = State_Tyler_BoostP1;
                player.Action_PlaySoundUninterruptable(player.sfxBoostCharge);
            }
        }

        internal static void Action_Tyler_Blink()
        {
            //Blink state includes fancy effect, insteads of basic flashing. It also disables attack-related inputs.
            blinkState = true;
            player.invincibilityTime = 60f;
        }

        internal static void Action_Tyler_BoostBreaker()
        {
            burnoutState = true;
            overCharge = 100f;
        }


        internal static void Action_Tyler_Kunai()
        {
            int kunaiNum = 1;
            if (!player.onGround) kunaiNum = 5;

            for (int i = 0; i < kunaiNum; i++)
            {

                float throwAngle = player.angle;
                if (!player.onGround)
                    throwAngle = player.angle - kunaiAngle * 10f;


                float spawnX = 8f;
                if (player.currentAnimation == "CrouchAttack_Loop")
                {
                    spawnX = -8f;
                }
                FPAudio.PlaySfx(kunaiSfx);
                ProjectileBasic basicShot;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * throwAngle) * 32f + Mathf.Sin(0.017453292f * throwAngle) * spawnX, player.position.y + Mathf.Cos(0.017453292f * throwAngle) * spawnX - Mathf.Sin(0.017453292f * throwAngle) * 32f);
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * -10f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * -10f;
                }
                else
                {
                    basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * throwAngle) * 32f + Mathf.Sin(0.017453292f * throwAngle) * spawnX, player.position.y + Mathf.Cos(0.017453292f * throwAngle) * spawnX + Mathf.Sin(0.017453292f * throwAngle) * 32f);
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * 10f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * 10f;
                }
                basicShot.animatorController = kunaiProjectile;
                basicShot.animator = basicShot.GetComponent<Animator>();
                basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
                basicShot.attackPower = (kunaiDamage * player.GetAttackModifier()) / kunaiNum;
                basicShot.direction = player.direction;
                if (player.direction == FPDirection.FACING_LEFT)
                    basicShot.direction = FPDirection.FACING_LEFT;
                else
                    basicShot.direction = FPDirection.FACING_RIGHT;
                basicShot.angle = player.angle;
                basicShot.damageElementType = -1;
                basicShot.explodeType = FPExplodeType.WHITEBURST;
                basicShot.ignoreTerrain = false;
                basicShot.explodeTimer = 50f;
                basicShot.terminalVelocity = 0f;
                basicShot.gravityStrength = 0;
                basicShot.sfxExplode = null;
                basicShot.parentObject = player;
                basicShot.faction = player.faction;
                basicShot.timeBeforeCollisions = 0f;
                basicShot.halfHeight = 2;
                basicShot.halfWidth = 8;

                kunaiAngle++;
            }
            Action_Tyler_ResetKunaiAngle();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Action_Hurt", MethodType.Normal)]
        static void PatchActionHurt(FPPlayer __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                //Guard active, or autoguard also active
                if (__instance.guardTime > 15 || (FPSaveManager.assistGuard == 1 && !player.IsPowerupActive(FPPowerup.NO_GUARDING) && player.guardTime <= 0f && (player.state == new FPObjectState(player.State_Ground)
                    || player.state == new FPObjectState(player.State_InAir) || player.state == new FPObjectState(player.State_LookUp) || player.state == new FPObjectState(player.State_Crouching)
                    || player.state == new FPObjectState(player.State_Swimming))))
                {
                    if (!__instance.guardEffectFlag)
                    {
                        GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, __instance.position.x, __instance.position.y);
                        guardFlash.render.material = FPResources.material[0];
                        FPAudio.PlaySfx(16);
                        __instance.guardEffectFlag = true;
                        //Ensure the guard later on doesnt trigger
                        __instance.guardTime = 14.9f;
                        __instance.flashTime = Mathf.Max(__instance.flashTime, __instance.guardTime);
                        __instance.hitStun = Mathf.Max(__instance.hitStun, 5f);
                    }
                    //Reduce damage taken by half
                    __instance.healthDamage /= 2;
                }

                //Is the hit lethal (even if halfed)? If so, do we have the revive item?
                //Drowning is excluded, per request. (tbh i wouldve given the player the water shield)
                if (__instance.hasSpecialItem && __instance.oxygenLevel > 0)
                {
                    if ((__instance.health - __instance.healthDamage) <= 0f)
                    {
                        //Set health to barely alive, and zero out the damage
                        __instance.health = 0.1f;
                        __instance.healthDamage = 0f;
                        //No knockback
                        __instance.hurtKnockbackX = 0f;
                        __instance.hurtKnockbackY = 0f;
                        //Spawn appropriate shield
                        if (lastDamageType < 0 || lastDamageType > 4)
                        {
                            //If damage type was 'normal' spawn Wood shield instead, as there is no valid 'Normal' shield type.
                            //Also engage wooden shield for any non-standard damage types
                            __instance.shieldID = 0;
                        }
                        else __instance.shieldID = (byte)lastDamageType;
                        __instance.shieldHealth = 2;
                        //Take away the item
                        __instance.hasSpecialItem = false;
                        __instance.genericTimer = 0f;
                        __instance.invincibilityTime = 50f;
                        lastDamageType = -1;
                        FPAudio.PlaySfx(FPAudio.SFX_GLASSBREAK);
                        __instance.state = State_Tyler_CharmRevive;
                        return;
                    }
                }
            }
        }

        //States

        internal static void State_Tyler_Wall()
        {
            //Drop from wall
            if (player.input.down && ((player.direction == FPDirection.FACING_LEFT && !player.input.left) || (player.direction == FPDirection.FACING_RIGHT && !player.input.right)))
            {
                wallClingTimer = 0f;
            }
            //Out of wall cling time
            if (wallClingTimer <= 0f || player.colliderWall == null)
            {
                ApplyAirForces(player, false);
                ApplyGravityForce(player);
                RotatePlayerUpright(player);
                if (!player.airDrag)
                {
                    player.airDrag = true;
                }
                player.genericTimer = 0f;
                wallClingTimer = 0f;
                player.pushTimer = 0f;
                if (player.velocity.y < 0f)
                {
                    player.SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 0f, 0f);
                }
                player.state = player.State_InAir;
                return;
            }

            bool left = player.input.left;
            bool right = player.input.right;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.input.left = true;
                player.input.right = false;
            }
            else if (player.direction == FPDirection.FACING_RIGHT)
            {
                player.input.right = true;
                player.input.left = false;
            }
            ApplyAirForces(player, false);
            ApplyGravityForce(player);
            RotatePlayerUpright(player);
            player.input.left = left;
            player.input.right = right;
            if (!player.airDrag)
            {
                player.airDrag = true;
            }
            player.genericTimer += FPStage.deltaTime;
            player.velocity.y = 0;

            //Want to jump off
            if (player.input.jumpPress)
            {
                player.Action_Jump();
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    if (player.input.right)
                    {
                        player.velocity.x = 6f;
                    }
                    else
                    {
                        player.velocity.x = 5f;
                    }
                }
                else if (player.direction == FPDirection.FACING_RIGHT)
                {
                    if (player.input.left)
                    {
                        player.velocity.x = -6f;
                    }
                    else
                    {
                        player.velocity.x = -5f;
                    }
                }
                player.genericTimer = 0f;
                wallClingTimer = 0f;
                player.state = player.State_InAir;
                player.SetPlayerAnimation("Jumping", 0f, 0f);
                //Tyler "Jump 1/2" lines go here
                //UnityEngine.Random.Range() is a funny creature, first value is inclusive, but the second is exclusive. So, "0, 2" is a range from 0 to 1.
                player.audioChannel[0].PlayOneShot(player.vaExtra[UnityEngine.Random.Range(0, 2)]);
            }
            //On ground somehow, while also wall clinging. Force grounded state
            else if (player.onGround)
            {
                player.genericTimer = 0f;
                wallClingTimer = 0f;
                player.state = player.State_Ground;
            }
            //Player pressed other button, disengage
            else
            {
                if ((player.direction == FPDirection.FACING_LEFT && !player.input.left) || (player.direction == FPDirection.FACING_RIGHT && !player.input.right))
                {
                    wallClingTimer -= FPStage.deltaTime;
                }
                else if ((player.direction == FPDirection.FACING_LEFT && player.input.left) || (player.direction == FPDirection.FACING_RIGHT && player.input.right))
                {
                    wallClingTimer = 9f;
                }
                if (player.input.attackPress || player.input.guardPress)
                {
                    Action_Tyler_AirMoves();
                }
            }
        }

        internal static void State_Tyler_TailSpin()
        {
            if (player.onGround)
            {
                ApplyGroundForces(player, false);
                player.angle = player.groundAngle;
                player.state = player.State_Ground;

                Action_Tyler_GroundMoves();
                player.jumpAbilityFlag = false;
                player.State_Ground();
            }
            else
            {
                ApplyAirForces(player, false);
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                }
                RotatePlayerUpright(player);
                Action_Tyler_AirMoves();
                //player.Process360Movement();
            }
            if (Mathf.Repeat(player.genericTimer, 8f) < 1f)
            {
                FPStage.CreateStageObject(Sparkle.classID, player.position.x + global::UnityEngine.Random.Range(-32f, 32f), player.position.y + global::UnityEngine.Random.Range(-24f, 24f));
            }
            if (player.velocity.x < 0f)
            {
                player.attackKnockback.x = -player.attackKnockback.x;
            }
            if (!player.input.down || player.targetWaterSurface != null)
            {
                player.velocity.y = player.velocity.y + (player.gravityStrength + 0.225f) * FPStage.deltaTime;
            }
            else
            {
                player.velocity.y = player.velocity.y + player.gravityStrength * FPStage.deltaTime;
            }
            if (player.velocity.y < -24f)
            {
                player.velocity.y = -24f;
            }
            player.quadrant = 0;
            if (player.state == new FPObjectState(State_Tyler_TailSpin))
            {
                if (player.genericTimer < 80f)
                {
                    player.genericTimer += FPStage.deltaTime;
                    player.SetPlayerAnimation("TailSpin", null, null, false, true);
                    if (player.animator.speed > 0.25f)
                    {
                        player.animator.SetSpeed(player.animator.GetSpeed() - FPStage.deltaTime * 0.01f);
                    }
                }
                else
                {
                    player.genericTimer = 0f;
                    player.SetPlayerAnimation("Jumping", new float?(0.5f), new float?(0.5f), false, true);
                    player.state = new FPObjectState(player.State_InAir);
                }
            }
        }

        internal static void State_Tyler_TailSwipe()
        {
            if (player.onGround)
            {
                if (player.input.jumpPress)
                {
                    player.Action_SoftJump();
                }
                else
                {
                    ApplyGroundForces(player, false);
                    player.angle = player.groundAngle;
                }
                player.jumpAbilityFlag = false;
            }
            else
            {
                ApplyAirForces(player, false);
                ApplyGravityForce(player);
                RotatePlayerUpright(player);
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                }
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                    player.velocity.y += 0.3f * FPStage.deltaTime;
                    if (player.velocity.y < -4.5f)
                    {
                        player.velocity.y = -4.5f;
                    }
                }
            }
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                if (player.onGround)
                {
                    if (player.input.down && Mathf.Abs(player.groundVel) <= 3f)
                    {
                        player.state = player.State_Crouching;
                        player.SetPlayerAnimation("Crouching", 1f, 0f, true);
                    }
                    else
                    {
                        player.state = player.State_Ground;
                    }
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    player.state = player.State_InAir;
                }
            }
            player.attackStats = AttackStats_TailSwipe;
        }

        internal static void State_Tyler_EclipseCombo()
        {

        }

        internal static void State_Tyler_BoostP1()
        {
            player.invincibilityTime = Mathf.Max(player.invincibilityTime, 15f);
            if (player.onGround)
            {
                ApplyGroundForces(player, false);
                player.angle = 0f;
            }
            else
            {
                if (player.velocity.x > 0f)
                {
                    player.velocity.x -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.x < 0f)
                {
                    player.velocity.x += 0.125f * FPStage.deltaTime;
                }
                if (player.velocity.y > 0f)
                {
                    player.velocity.y -= 0.125f * FPStage.deltaTime;
                }
                else if (player.velocity.y < 0f)
                {
                    player.velocity.y += 0.125f * FPStage.deltaTime;
                }
                if (player.input.left)
                {
                    player.direction = FPDirection.FACING_LEFT;
                }
                else if (player.input.right)
                {
                    player.direction = FPDirection.FACING_RIGHT;
                }
            }
            if (player.genericTimer < 30f)
            {
                player.genericTimer += FPStage.deltaTime;
                if (player.input.up)
                {
                    player.specialAttackDirection = 0;
                }
                else if (player.input.down)
                {
                    player.specialAttackDirection = 1;
                }
                else if (player.input.left)
                {
                    player.specialAttackDirection = 2;
                }
                else if (player.input.right)
                {
                    player.specialAttackDirection = 2;
                }
                player.SetPlayerAnimation("Rolling");
                if (Mathf.Repeat(player.genericTimer, 4f) < 1f)
                {
                    FPStage.CreateStageObject(Sparkle.classID, player.position.x + Random.Range(-24f, 24f), player.position.y + Random.Range(-24f, 24f));
                }
                player.attackStats = AttackStats_Idle;
                if (player.direction == FPDirection.FACING_LEFT)
                {
                    player.attackKnockback.x = 0f - player.attackKnockback.x;
                }
                return;
            }
            switch (player.specialAttackDirection)
            {
                case 0:
                    if (player.direction == FPDirection.FACING_LEFT)
                    {
                        player.velocity.x = Mathf.Min(Mathf.Min(player.recoveryTimer, 0f) * 0.3f - 12f, player.recoveryTimer);
                    }
                    else
                    {
                        player.velocity.x = Mathf.Max(Mathf.Max(player.recoveryTimer, 0f) * 0.3f + 12f, player.recoveryTimer);
                    }
                    player.velocity.y = 12f;
                    player.onGround = false;
                    break;
                case 1:
                    if (player.direction == FPDirection.FACING_LEFT)
                    {
                        player.velocity.x = Mathf.Min(Mathf.Min(player.recoveryTimer, 0f) * 0.3f - 12f, player.recoveryTimer);
                    }
                    else
                    {
                        player.velocity.x = Mathf.Max(Mathf.Max(player.recoveryTimer, 0f) * 0.3f + 12f, player.recoveryTimer);
                    }
                    player.velocity.y = -12f;
                    player.onGround = false;
                    break;
                case 2:
                    if (player.onGround)
                    {
                        if (player.direction == FPDirection.FACING_LEFT)
                        {
                            player.groundVel = Mathf.Min(Mathf.Min(player.groundVel, 0f) * 0.5f - 15f, player.groundVel);
                        }
                        else
                        {
                            player.groundVel = Mathf.Max(Mathf.Max(player.groundVel, 0f) * 0.5f + 15f, player.groundVel);
                        }
                    }
                    else if (player.direction == FPDirection.FACING_LEFT)
                    {
                        player.velocity.x = Mathf.Min(Mathf.Min(player.velocity.x, 0f) * 0.5f - 15f, player.velocity.x);
                        player.velocity.y = 0f;
                    }
                    else
                    {
                        player.velocity.x = Mathf.Max(Mathf.Max(player.velocity.x, 0f) * 0.5f + 15f, player.velocity.x);
                        player.velocity.y = 0f;
                    }
                    break;
            }
            BoostFlame boostFlame = (BoostFlame)FPStage.CreateStageObject(BoostFlame.classID, -100f, -100f);
            boostFlame.parentObject = player;
            boostFlame.spriteRenderer.sprite = boostFlame.defaultSprite;
            player.genericTimer = 0f;
            player.Action_PlaySoundUninterruptable(player.sfxBoostLaunch);
            player.state = State_Tyler_BoostP2;
            player.attackStats = AttackStats_Tyler_Boost;
            FPCamera.stageCamera.screenShake = Mathf.Max(FPCamera.stageCamera.screenShake, 10f);
        }

        internal static void State_Tyler_BoostP2()
        {
            player.invincibilityTime = Mathf.Max(player.invincibilityTime, 15f);
            if (player.genericTimer < 34f)
            {
                if (player.hitStun <= 0f)
                {
                    player.SetPlayerAnimation("Boost_Loop");
                    player.energy -= 3.4f * FPStage.deltaTime;
                    player.genericTimer += FPStage.deltaTime;
                    if (player.hitStun <= 0f)
                    {
                        if (player.onGround)
                        {
                            if (player.direction == FPDirection.FACING_LEFT)
                            {
                                if (player.groundVel > -15f)
                                {
                                    player.groundVel = -15f;
                                }
                            }
                            else if (player.groundVel < 15f)
                            {
                                player.groundVel = 15f;
                            }
                            player.velocity.y = 0f;
                            if (player.colliderWall != null && player.onGroundLastFrame)
                            {
                                player.onGround = false;
                                player.velocity.x = 0f - player.prevGroundVel;
                                player.velocity.y = 5f;
                                player.direction ^= FPDirection.FACING_RIGHT;
                                player.SetPlayerAnimation("BoostWallBonk");
                                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);
                                player.state = State_Tyler_Wall_Bonk;
                                return;
                            }
                            player.angle = player.groundAngle;
                        }
                        else
                        {
                            if (player.colliderWall != null)
                            {
                                player.velocity.x = 0f - player.prevVelocity.x;
                                player.direction ^= FPDirection.FACING_RIGHT;
                                player.SetPlayerAnimation("BoostWallBonk");
                                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);
                                player.state = State_Tyler_Wall_Bonk;
                                return;
                            }
                            else if (player.colliderRoof != null)
                            {
                                player.velocity.x = player.prevVelocity.x;
                                player.velocity.y = 0f - player.prevVelocity.y;
                                player.Action_PlaySoundUninterruptable(player.sfxBoostRebound);
                            }
                            if (player.velocity.x != 0f && player.state == new FPObjectState(State_Tyler_BoostP2))
                            {
                                if (player.direction == FPDirection.FACING_RIGHT)
                                {
                                    player.angle = Mathf.Atan2(player.velocity.y, player.velocity.x) * 57.29578f;
                                }
                                else
                                {
                                    player.angle = Mathf.Atan2(player.velocity.y, player.velocity.x) * 57.29578f + 180f;
                                }
                                player.angle = Mathf.Repeat(player.angle + 360f, 360f);
                            }
                        }
                    }
                    if (player.hitStun <= 0f && Mathf.Repeat(player.genericTimer, 2f) < 1f)
                    {
                        BoostFlameTrail boostFlameTrail = (BoostFlameTrail)FPStage.CreateStageObject(BoostFlameTrail.classID, player.position.x + UnityEngine.Random.Range(-24f, 24f), player.position.y + UnityEngine.Random.Range(-24f, 24f));
                        boostFlameTrail.parentObject = player;
                        if (player.direction == FPDirection.FACING_LEFT)
                        {
                            boostFlameTrail.velocity.x = player.velocity.x + Mathf.Cos((float)Math.PI / 180f * player.angle) * 9f;
                            boostFlameTrail.velocity.y = player.velocity.y + Mathf.Sin((float)Math.PI / 180f * player.angle) * 9f;
                        }
                        else
                        {
                            boostFlameTrail.velocity.x = player.velocity.x - Mathf.Cos((float)Math.PI / 180f * player.angle) * 9f;
                            boostFlameTrail.velocity.y = player.velocity.y - Mathf.Sin((float)Math.PI / 180f * player.angle) * 9f;
                        }
                        boostFlameTrail.angle = player.angle;
                    }
                }
                else
                {
                    player.genericTimer = 0f;
                    player.angle = 0f;
                    player.SetPlayerAnimation("Jumping", 0.25f, 0.25f);
                    player.state = player.State_InAir;
                }
                if (player.input.specialPress)
                {
                    Action_Tyler_BoostBreaker();
                }
            }
        }

        internal static void State_Tyler_BoostBreaker()
        {




            if (player.onGround)
            {
                if (player.input.down && Mathf.Abs(player.groundVel) <= 3f)
                {
                    player.state = player.State_Crouching;
                    player.SetPlayerAnimation("Crouching", 1f, 0f, true);
                }
                else
                {
                    player.state = player.State_Ground;
                }
            }
            else
            {
                player.SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                player.state = player.State_InAir;
            }
        }

        internal static void State_Tyler_Wall_Bonk()
        {

        }

        internal static void State_Tyler_ClawDive()
        {
            ApplyAirForces(player, false);
            ApplyGravityForce(player);
            if (!player.onGround)
            {
                if (player.hitStun > 0f)
                {
                    player.SetPlayerAnimation("ClawDive");
                    player.genericTimer = 2f;
                    player.idleTimer = 0f - player.fightStanceTime;
                }
                else if (player.genericTimer > 0f)
                {
                    player.genericTimer -= FPStage.deltaTime;
                }
                else if (player.genericTimer <= 0f)
                {
                    player.SetPlayerAnimation("ClawDive");
                }
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                    if (player.velocity.y >= -1f)
                    {
                        player.state = player.State_InAir;
                    }
                }
                Action_Tyler_AirMoves();
            }
            else
            {
                player.state = State_Tyler_Roll;
                State_Tyler_Roll();
            }
        }

        internal static void State_Tyler_EclipseFang()
        {
            if (player.onGround)
            {
                ApplyGroundForces(player, false);
                player.angle = player.groundAngle;
            }
            else
            {
                ApplyAirForces(player, false);
                ApplyGravityForce(player);
                RotatePlayerUpright(player);
                Action_Tyler_AirMoves();
                player.groundAngle = 0f;
                if (player.targetWaterSurface != null)
                {
                    ApplyWaterForces(player);
                    player.velocity.y += 0.1f * FPStage.deltaTime;
                    if (player.velocity.y < -4.5f)
                    {
                        player.velocity.y = -4.5f;
                    }
                }
            }
            if (player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
            {
                if (player.onGround)
                {
                    player.state = player.State_Ground;
                }
                else
                {
                    player.SetPlayerAnimation("Jumping", 1f, 0.5f);
                    player.state = player.State_InAir;
                }
            }
            player.attackStats = AttackStats_Tyler_ClawDive;
        }

        internal static void State_Tyler_Roll()
        {
            player.SetPlayerAnimation("Rolling");
            player.attackStats = AttackStats_Tyler_Roll;
            player.genericTimer += FPStage.deltaTime;
            if (player.onGround)
            {
                player.animator.SetSpeed(Mathf.Abs(player.groundVel) * 0.15f);
                Action_Tyler_GroundMoves();
                if (player.input.jumpPress)
                {
                    player.Action_SoftJump();
                    player.animator.SetSpeed(2f);
                }
                else
                {
                    ApplyGroundForces(player, false);
                    player.angle = player.groundAngle;
                }
            }
            else
            {
                ApplyAirForces(player, false);
                ApplyGravityForce(player);
                if (!player.input.jumpHold && player.jumpReleaseFlag)
                {
                    player.jumpReleaseFlag = false;
                    if (player.velocity.y > player.jumpRelease)
                    {
                        player.velocity.y = player.jumpRelease;
                    }
                }
            }
            if (player.genericTimer > 15f && (!player.input.down || (player.onGround && Mathf.Abs(player.groundVel) < 2f) || (!player.onGround && player.velocity.y < 0f)))
            {
                if (player.onGround)
                {
                    player.state = player.State_Ground;
                }
                else
                {
                    player.state = player.State_InAir;
                }
            }
            else if (player.input.attackPress)
            {
                //player.SetPlayerAnimation("TailSwipe");
                //player.nextAttack = 1;
                //player.idleTimer = 0f - player.fightStanceTime;
                //player.state = State_Tyler_TailSwipe;
                //combo = false;
            }
        }

        internal static void State_Tyler_CharmRevive()
        {
            if (player.genericTimer < 50f)
            {
                player.velocity = Vector3.zero;
                if (player.onGround)
                {

                    ApplyGroundForces(player, true);
                    player.angle = player.groundAngle;
                    player.jumpAbilityFlag = false;
                }
                else
                {
                    ApplyAirForces(player, true);
                    ApplyGravityForce(player);
                    RotatePlayerUpright(player);
                }
                player.SetPlayerAnimation("ReviveSurge");
                player.Action_PlayVoice(player.vaRevive[Random.Range(0, player.vaRevive.Length)]);
                player.transform.GetChild(0).gameObject.SetActive(true);
                player.genericTimer += FPStage.deltaTime;
            }
            else
            {
                player.transform.GetChild(0).gameObject.SetActive(false);
                if (player.onGround)
                {
                    player.state = player.State_Ground;
                }
                else
                {
                    player.state = player.State_InAir;
                }
            }
        }

        //AttackStats
        private static void AttackStats_Idle()
        {
            player.attackPower = 2f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 5f / player.animator.speed;
            player.attackKnockback.x = 0f;
            player.attackKnockback.y = 0f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Tyler_Blink()
        {
            player.attackPower = 0f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = 0f;
            player.attackKnockback.y = 0f;
            player.attackSfx = 7;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_TailSwipe()
        {
            player.attackPower = 3f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 5f / player.animator.speed;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = 0f - player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Kick()
        {
            player.attackPower = 3f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 5f / player.animator.speed;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = 0f - player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Tyler_TailSpin()
        {
            player.attackPower = 2f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 1.5f);
            if (player.onGround)
            {
                player.attackKnockback.y = 0f;
            }
            else
            {
                player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            }
            player.attackSfx = 4;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Tyler_Roll()
        {
            player.attackPower = 2f;
            player.attackHitstun = 3f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 6f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = 0f - player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 7;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Tyler_ClawDive()
        {
            player.attackPower = 2f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 6f / player.animator.speed;
            player.attackKnockback.x = player.prevVelocity.x * 0.5f;
            player.attackKnockback.y = player.prevVelocity.y;
            player.attackSfx = 5;
            player.attackPower *= player.GetAttackModifier();
        }

        private static void AttackStats_Tyler_Boost()
        {
            player.attackPower = 4f;
            player.attackHitstun = 4f;
            player.attackEnemyInvTime = 6f;
            player.attackKnockback.x = Mathf.Max(Mathf.Abs(player.prevVelocity.x * 1.5f), 4.5f);
            if (player.direction == FPDirection.FACING_LEFT)
            {
                player.attackKnockback.x = 0f - player.attackKnockback.x;
            }
            player.attackKnockback.y = player.prevVelocity.y * 0.5f;
            player.attackSfx = 6;
            player.attackPower *= player.GetAttackModifier();
        }

        //Others
        private static void PlaySFXLooping(AudioClip clip, float volume)
        {
            //Channel 4 is used for Carol's bike, so we can repurpose it here.
            if (clip != null)
            {
                if (player.audioChannel[4].clip != clip)
                {
                    player.audioChannel[4].clip = clip;
                    player.audioChannel[4].Play();
                }
                player.audioChannel[4].volume = volume;
            }
        }

        private static void StopSFXLooping()
        {
            player.audioChannel[4].Stop();
            player.audioChannel[4].clip = null;
        }

        //Prefixes
        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "AutoGuard", MethodType.Normal)]
        static bool PatchAutoGuard(FPPlayer __instance, ref bool __result)
        {
            if (__instance.characterID == TylerKozaki.currentTylerID)
            {
                if (FPSaveManager.assistGuard == 1 && !player.IsPowerupActive(FPPowerup.NO_GUARDING) && player.guardTime <= 0f && (player.state == new FPObjectState(player.State_Ground)
                    || player.state == new FPObjectState(player.State_InAir) || player.state == new FPObjectState(player.State_LookUp) || player.state == new FPObjectState(player.State_Crouching)
                    || player.state == new FPObjectState(player.State_Swimming)))
                {
                    //Trigger normal guard effect.
                    player.input.guardPress = true;
                }
                //Disable the AutoGuard effect of just not taking damage at all.
                __result = false;
                //Skip original
                return false;
            }
            //Other character, run original code
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdatePre(FPPlayer __instance)
        {
            if (__instance.damageType >= 0) lastDamageType = __instance.damageType;
        }

        //Postfixes
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Update", MethodType.Normal)]
        static void PatchPlayerUpdate(FPPlayer __instance, float ___speedMultiplier, float ___guardBuffer, float ___jumpMultiplier)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                //Value Yeeter 1000 Lite
                player = __instance;
                guardBuffer = ___guardBuffer;
                jumpMultiplier = ___jumpMultiplier;
                speedMultiplier = ___speedMultiplier;

                //Wall cling - Carol's version also just drags her out of update loop by force.
                //Hopefully a humble postfix will do the job, otherwise the following code should be placed into a method and call to it transpiled under Carol's version of this code
                if (player.state == new FPObjectState(player.State_InAir) || player.state == new FPObjectState(player.State_Swimming))
                {
                    if (!player.onGround && player.state != new FPObjectState(State_Tyler_Wall)/* && state != new FPObjectState(State_Carol_Punch) && state != new FPObjectState(State_Carol_JumpDiscThrow) && state != new FPObjectState(State_Carol_JumpDiscWarp)*/)
                    {
                        if (player.velocity.y < 4f && ((player.input.left && player.prevVelocity.x <= 0f && player.velocity.x <= 0f) ||
                            (player.input.right && player.prevVelocity.x >= 0f && player.velocity.x >= 0f)) && player.colliderWall != null && (player.targetWaterSurface == null ||
                            (!player.input.up && !player.input.jumpHold)))
                        {
                            player.pushTimer += FPStage.deltaTime;
                        }
                        else
                        {
                            player.pushTimer = 0f;
                        }
                        if (player.pushTimer > 0f && player.inputLock <= 0f && ((player.input.left && player.prevVelocity.x <= 0f && player.velocity.x <= 0f) || (player.input.right && player.prevVelocity.x >= 0f && player.velocity.x >= 0f)))
                        {
                            player.SetPlayerAnimation("Wall", 0f, 0f);
                            player.state = State_Tyler_Wall;
                            wallClingTimer = 9f;
                            player.jumpAbilityFlag = false;
                            player.Action_PlaySoundUninterruptable(player.sfxWallCling);

                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "LateUpdate", MethodType.Normal)]
        static void PatchLateUpdate(FPPlayer __instance)
        {
            blinkTimer -= FPStage.deltaTime;
            throwDelay -= FPStage.deltaTime;
            chargeThrowDelay -= FPStage.deltaTime;

            if (overCharge > 0)
            {
                overCharge -= energyRecoveryBaseSpeed * FPStage.deltaTime;
                __instance.energyRecoverRate = 0;
            }
            if (overCharge < 0)
            {
                overCharge = 0;
                burnoutState = false;
                __instance.energyRecoverRate = energyRecoveryBaseSpeed;
            }

            __instance.invincibilityTime = Math.Max(__instance.invincibilityTime, blinkTimer);
            if (blinkState && blinkTimer < 0)
            {
                blinkState = false;
                __instance.attackStats = AttackStats_Idle;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPPlayer), "Start", MethodType.Normal)]
        static void PatchPlayerStart(FPPlayer __instance)
        {
            if (FPSaveManager.character == TylerKozaki.currentTylerID)
            {
                player = __instance;
                //Append 2 extra spare audio channels
                //Channel 4 - Looping SFX
                //Channel 5 - Things normal game logic should not mess with
                for (int i = 4; i < 6; i++)
                {
                    GameObject gameObject = new GameObject("PlayerAudioSource");
                    gameObject.transform.parent = player.gameObject.transform;
                    player.audioChannel = player.audioChannel.AddToArray(gameObject.AddComponent<AudioSource>());
                    player.audioChannel[i].volume = FPSaveManager.volumeSfx;
                    player.audioChannel[i].playOnAwake = false;
                }

                //Load projectile animations
                //darkSparkle = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("DarkSpark");
                //kunaiProjectile = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("Kunai");
                //bladeThrowProjectile = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("ThrownBlade");
                //umbralBombProjectile = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("DarkSpark");

                //Set up lower health
                //Potion Seller will then bump it back to 6 and hopefully won't break in a thousand different ways.
                __instance.healthMax--;

                //Potion seller related fix
                energyRecoveryBaseSpeed = __instance.energyRecoverRate;
            }
        }

        //Reverse Patches
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGroundForces", MethodType.Normal)]
        public static void ApplyGroundForces(FPPlayer instance, bool ignoreDirectionalInput)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyWaterForces", MethodType.Normal)]
        public static void ApplyWaterForces(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyAirForces", MethodType.Normal)]
        public static void ApplyAirForces(FPPlayer instance, bool ignoreDirectionalInput)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "ApplyGravityForce", MethodType.Normal)]
        public static void ApplyGravityForce(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "SetAnimSpeedToVelocity", MethodType.Normal)]
        public static void SetAnimSpeedToVelocity(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(FPPlayer), "RotatePlayerUpright", MethodType.Normal)]
        public static void RotatePlayerUpright(FPPlayer instance)
        {
            // Replaced at runtime with reverse patch
            throw new NotImplementedException("Method failed to reverse patch!");
        }
    }
}
