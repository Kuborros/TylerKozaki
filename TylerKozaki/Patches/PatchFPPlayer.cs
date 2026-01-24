using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TylerKozaki.Patches
{
    internal class PatchFPPlayer
    {

        public static FPPlayer player;
        public static PlayerShadow playerShadow;

        public static AudioClip kunaiSfx;
        public static AudioClip bladeThrowSfx;
        public static AudioClip umbralBombSfx;
        public static AudioClip chargeSfx;

        internal static float guardBuffer;
        internal static float jumpMultiplier;
        internal static float speedMultiplier;

        private static float attackCharge = 0f;
        private static float wallClingTimer = 0f;
        private static float overCharge = 0f;
        private static float blinkTimer = 0f;
        private static float energyRecoveryBaseSpeed = 0.4f;

        internal static float throwCharge = 0f;
        internal static float throwDelay = 0f;

        private static readonly float kunaiDamage = 4f;
        private static readonly float bladeThrowDamage = 6f;
        private static readonly float umbralBombDamage = 8f;

        internal static bool blinkState = false;
        internal static bool burnoutState = false;
        internal static bool combo = false;

        private static int kunaiAngle = 0;

        private static RuntimeAnimatorController kunaiProjectile;
        private static RuntimeAnimatorController bladeThrowProjectile;
        private static RuntimeAnimatorController umbralBombProjectile;

        private static RuntimeAnimatorController darkSparkle;

        internal static readonly MethodInfo m_AirMoves = SymbolExtensions.GetMethodInfo(() => Action_Tyler_AirMoves());
        internal static readonly MethodInfo m_FuelPickup = SymbolExtensions.GetMethodInfo(() => Action_Tyler_FuelPickup());
        internal static readonly MethodInfo m_GroundMoves = SymbolExtensions.GetMethodInfo(() => Action_Tyler_GroundMoves());

        private static readonly FPHitBox kunaiHitbox = new FPHitBox { left = -8, right = 8, top = 4, bottom = -4, enabled = true };
        private static readonly FPHitBox bladeThrowHitbox = new FPHitBox { left = -10, right = 10, top = 10, bottom = -10, enabled = true };
        private static readonly FPHitBox umbralBombHitbox = new FPHitBox { left = -30, right = 30, top = 14, bottom = -14, enabled = true };


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
                player.SetPlayerAnimation("GuardAir", null, null, false, true);
                player.animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(player.velocity.x * 0.05f)));
                FPAudio.PlaySfx(15);
                player.Action_Guard(0f, false);
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            //Air melee
            else if (player.input.attackPress && !blinkState)
            {

            }
            //Air Special
            else if (player.input.specialPress && !blinkState)
            {

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
                player.Action_ShadowGuard();
                GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, player.position.x, player.position.y);
                guardFlash.parentObject = player;
            }
            //Ground Melee
            else if (!player.input.down && !player.input.jumpPress && player.state != new FPObjectState(State_Tyler_Kick) &&
                ((player.state != new FPObjectState(State_Tyler_TailSpin) && ((FPSaveManager.holdToAttack >= 1 && player.input.attackHold) || player.input.attackPress)) || player.input.attackPress))
            {
                player.genericTimer = 0f;
                player.idleTimer = 0f - player.fightStanceTime;
                if (player.nextAttack > 1 && player.nextAttack < 4)
                {
                    player.SetPlayerAnimation("HairWhip" + player.nextAttack);
                    player.nextAttack++;
                }
                else
                {
                    player.SetPlayerAnimation("HairWhip1");
                    player.nextAttack = 2;
                }
                player.state = State_Tyler_Kick;
                player.Action_StopSound();
                combo = false;

            }
            else if (player.input.specialPress && !blinkState && player.energy >= 5)
            {

            }
        }

        internal static void Action_Tyler_Blink()
        {
            //Blink state includes fancy effect, insteads of basic flashing. It also disables attack-related inputs.
            blinkState = true;
            player.invincibilityTime = 60f;
        }

        internal static void Action_Tyler_Kunai()
        {
            int kunaiNum = 1;
            if (!player.onGround) kunaiNum = 5;

            for (int i = 0; i < kunaiNum; i++)
            {

                float throwAngle = player.angle;
                if (!player.onGround)
                    throwAngle = player.angle - kunaiAngle * 5f;


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
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * -20f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * -20f;
                }
                else
                {
                    basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * throwAngle) * 32f + Mathf.Sin(0.017453292f * throwAngle) * spawnX, player.position.y + Mathf.Cos(0.017453292f * throwAngle) * spawnX + Mathf.Sin(0.017453292f * throwAngle) * 32f);
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * 20f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * 20f;
                }
                basicShot.animatorController = kunaiProjectile;
                basicShot.animator = basicShot.GetComponent<Animator>();
                basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
                basicShot.attackPower = kunaiDamage * player.GetAttackModifier();
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
                basicShot.hbTouch = kunaiHitbox;
                basicShot.halfHeight = 4;
                basicShot.halfWidth = 8;

                if (player.IsPowerupActive(FPPowerup.SHADOW_GUARD))
                {

                }
                kunaiAngle++;
            }
        }

        internal static void Action_Tyler_ChargedShotFire()
        {
            float spawnX = 8f;
            if (player.currentAnimation == "CrouchAttack_Loop")
            {
                spawnX = -8f;
            }

            ProjectileBasic chargeShot;
            if (player.direction == FPDirection.FACING_LEFT)
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x - Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * spawnX, player.position.y + Mathf.Cos(0.017453292f * player.angle) * spawnX - Mathf.Sin(0.017453292f * player.angle) * 32f);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * -20f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * -20f;
            }
            else
            {
                chargeShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, player.position.x + Mathf.Cos(0.017453292f * player.angle) * 32f + Mathf.Sin(0.017453292f * player.angle) * spawnX, player.position.y + Mathf.Cos(0.017453292f * player.angle) * spawnX + Mathf.Sin(0.017453292f * player.angle) * 32f);
                chargeShot.velocity.x = Mathf.Cos(0.017453292f * player.angle) * 20f;
                chargeShot.velocity.y = Mathf.Sin(0.017453292f * player.angle) * 20f;
            }


            if (throwCharge > 90f)
            {
                chargeShot.animatorController = umbralBombProjectile;
                chargeShot.hbTouch = umbralBombHitbox;
                chargeShot.halfHeight = 14;
                chargeShot.halfWidth = 30;
            }
            else
            {
                chargeShot.animatorController = bladeThrowProjectile;
                chargeShot.hbTouch = bladeThrowHitbox;
                chargeShot.halfHeight = 10;
                chargeShot.halfWidth = 10;
            }
            chargeShot.animator = chargeShot.GetComponent<Animator>();
            chargeShot.animator.runtimeAnimatorController = chargeShot.animatorController;
            chargeShot.attackPower = (kunaiDamage + Math.Min(umbralBombDamage, throwCharge / 10)) * player.GetAttackModifier();
            if (player.direction == FPDirection.FACING_LEFT)
                chargeShot.direction = FPDirection.FACING_LEFT;
            else
                chargeShot.direction = FPDirection.FACING_RIGHT;
            chargeShot.angle = player.angle;
            chargeShot.damageElementType = 3;
            chargeShot.explodeType = FPExplodeType.EXPLOSION;
            chargeShot.ignoreTerrain = false;
            chargeShot.explodeTimer = 50f;
            chargeShot.terminalVelocity = 0f;
            chargeShot.gravityStrength = 0;
            chargeShot.sfxExplode = null;
            chargeShot.parentObject = player;
            chargeShot.faction = player.faction;
            chargeShot.timeBeforeCollisions = 0f;

            throwCharge = 0f;

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
                        __instance.shieldID = (byte)__instance.damageType;
                        __instance.shieldHealth = 2;
                        //Take away the item
                        __instance.hasSpecialItem = false;
                        __instance.Action_PlaySound(__instance.sfxShieldBlock); //Some fancier SFX maybe?
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
                player.audioChannel[0].PlayOneShot(player.vaExtra[UnityEngine.Random.Range(0, 1)]);
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

                Action_Tyler_GroundMoves();
                player.jumpAbilityFlag = false;
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
                player.Process360Movement();
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
            if (!player.onGround && player.input.up && player.velocity.y < -3f)
            {
                player.velocity.y = -3f;
            }
            if (player.state == new FPObjectState(State_Tyler_TailSpin))
            {
                if (player.genericTimer < 80f)
                {
                    player.genericTimer += FPStage.deltaTime;
                    player.SetPlayerAnimation("TailSpin", null, null, false, true);
                    player.childSprite.angle = 20f - 0.5f * player.genericTimer;
                    if (player.animator.speed > 0.25f)
                    {
                        player.animator.SetSpeed(player.animator.GetSpeed() - FPStage.deltaTime * 0.01f);
                        player.childAnimator.SetSpeed(player.animator.GetSpeed() - FPStage.deltaTime * 0.005f);
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

        internal static void State_Tyler_Kick()
        {
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
                darkSparkle = TylerKozaki.dataBundle.LoadAsset<RuntimeAnimatorController>("DarkSpark");

                //Start with the bracelet when the item is equipped
                if (__instance.powerups.Contains(TylerKozaki.familyBraceletID))
                    __instance.hasSpecialItem = true;

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
