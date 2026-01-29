using UnityEngine;

namespace TylerKozaki.Objects
{
    internal class PlayerBossTyler : PlayerBoss
    {
        public static int classID = -1;

        [Header("Boss Settings")]
        public FPHitBox walkRange;
        public float pursuitRange;
        public FPBaseObject targetToPursue;
        public RuntimeAnimatorController kunaiProjectile;

        public float kunaiDamage;

        private int nextMotion;
        private int comboState;
        private int kunaiAngle;

        //Generic boss stuff
        public override void ResetStaticVars()
        {
            base.ResetStaticVars();
            classID = -1;
        }

        public void Action_FacePlayer()
        {
            if (targetPlayer != null)
            {
                if (position.x > targetPlayer.position.x)
                {
                    direction = FPDirection.FACING_LEFT;
                }
                else if (position.x < targetPlayer.position.x)
                {
                    direction = FPDirection.FACING_RIGHT;
                }
            }
        }

        private void CheckBoundaries()
        {
            if (faction != "Player")
            {
                FPPlayer fPPlayer = FPStage.FindNearestPlayer(this, 640f);
                if (fPPlayer != null)
                {
                    if (invincibility > 0f)
                    {
                        if (position.x > fPPlayer.position.x + 10f)
                        {
                            direction = FPDirection.FACING_LEFT;
                        }
                        else if (position.x < fPPlayer.position.x - 10f)
                        {
                            direction = FPDirection.FACING_RIGHT;
                        }
                    }
                    else if (state == new FPObjectState(State_Running))
                    {
                        if (position.x > fPPlayer.position.x + pursuitRange)
                        {
                            direction = FPDirection.FACING_LEFT;
                        }
                        else if (position.x < fPPlayer.position.x - pursuitRange)
                        {
                            direction = FPDirection.FACING_RIGHT;
                        }
                    }
                }
            }
            else if (targetToPursue != null)
            {
                if (invincibility > 0f)
                {
                    if (position.x > targetToPursue.position.x + 10f)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else if (position.x < targetToPursue.position.x - 10f)
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                }
                else if (state == new FPObjectState(State_Running))
                {
                    if (position.x > targetToPursue.position.x + pursuitRange)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else if (position.x < targetToPursue.position.x - pursuitRange)
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                }
            }
            if (walkRange.enabled && position.x > start.x + walkRange.right)
            {
                direction = FPDirection.FACING_LEFT;
            }
            else if (walkRange.enabled && position.x < start.x + walkRange.left)
            {
                direction = FPDirection.FACING_RIGHT;
            }
            if (colliderWall != null)
            {
                if (onGround)
                {
                    groundVel = 0f - groundVel;
                }
                else
                {
                    velocity.x = 0f - prevVelocity.x;
                }
                direction ^= FPDirection.FACING_RIGHT;
            }
            if (direction == FPDirection.FACING_RIGHT)
            {
                input.right = true;
                input.left = false;
            }
            else
            {
                input.left = true;
                input.right = false;
            }

        }

        private void InteractWithObjects()
        {
            if (health <= healthToFlinch)
            {
                healthToFlinch -= 25f;
                invincibility = 60f;
                Action_Hurt();
            }
            if (state != new FPObjectState(base.State_KO) && health <= 0f)
            {
                if (frozen)
                {
                    defrostTimer = 0f;
                    DestroyIceBlock();
                    animator.SetSpeed(1f);
                    frozen = false;
                }
                Action_PlaySound(sfxKO);
                Action_PlayVoice(vaKO);
                if (bgmBoss != null)
                {
                    FPAudio.StopMusic();
                }
                gscrSlowdownOnKO = FPStage.SetRequestGameSpeedChange(this, 0.25f, 20f, GameSpeedChangeRequest.GameSpeedChangePriority_Medium);
                velocity.x = velocity.x * 0.75f + hurtKnockbackX;
                velocity.y = 8f;
                onGround = false;
                if (velocity.x < 3f && velocity.x > -3f)
                {
                    if (hurtKnockbackX > 0f)
                    {
                        velocity.x = 3f;
                    }
                    else
                    {
                        velocity.x = -3f;
                    }
                }
                genericTimer = -20f;
                invincibility = 200f;
                hitStun = 0f;
                state = base.State_KO;
            }
            switch (DamageCheck())
            {
                case 1:
                    flashTime = 2f;
                    break;
                case 2:
                    activationMode = FPActivationMode.ALWAYS_ACTIVE;
                    velocity.y = 4.5f;
                    break;
                case 4:
                    state = State_Frozen;
                    break;
            }
            cannotBeFrozen = false;
        }

        private void Update()
        {
            if (!FPStage.objectsRegistered)
            {
                return;
            }
            PlayerBossUpdate();
            if (targetPlayer == null)
            {
                targetPlayer = FPStage.FindNearestPlayer(this, 100000f);
            }
            if (state == new FPObjectState(base.State_Init))
            {
                SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                if (!bossActivated)
                {
                    state = State_Default;
                }
                else
                {
                    state = State_Running;
                }
            }
            else if (state == new FPObjectState(base.State_Guard))
            {
                CheckBoundaries();
                Process360Movement();
            }
            if (!(state != new FPObjectState(State_Frozen)) || !(state != new FPObjectState(base.State_KO)))
            {
                return;
            }
            if (onGround)
            {
                attackKnockback.x = groundVel * 0.5f;
            }
            else
            {
                attackKnockback.x = velocity.x * 0.5f;
            }
            attackKnockback.y = velocity.y * 0.5f;
            if (!FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
            {
                return;
            }
            FPBaseObject objRef = null;
            while (FPStage.ForEach(FPPlayer.classID, ref objRef))
            {
                FPPlayer fPPlayer = (FPPlayer)objRef;
                if (fPPlayer.invincibilityTime <= 0f && faction != fPPlayer.faction && FPCollision.CheckOOBB(this, hbAttack, objRef, fPPlayer.hbTouch))
                {
                    if (fPPlayer.guardTime <= 15f)
                    {
                        fPPlayer.hitStun = 4f;
                    }
                    fPPlayer.hurtKnockbackX = attackKnockback.x;
                    fPPlayer.hurtKnockbackY = attackKnockback.y;
                    fPPlayer.healthDamage += 0.5f;
                    fPPlayer.Action_HitSpark(this);
                }
            }
        }

        private new void LateUpdate()
        {
            if (FPStage.objectsRegistered)
            {
                PlayerBossLateUpdate();
                if (frozen && state != new FPObjectState(State_Frozen))
                {
                    defrostTimer = 0f;
                    DestroyIceBlock();
                    animator.SetSpeed(1f);
                    frozen = false;
                }
                if (energy < 100f)
                {
                    energy += 0.4f * FPStage.deltaTime;
                }
            }
        }

        private new void Start()
        {
            healthToFlinch = health - 25f;
            base.Start();
            classID = FPStage.RegisterObjectType(this, GetType(), 0);
            objectID = classID;
            if (FPSaveManager.character != TylerKozaki.currentTylerID)
            {
                return;
            }
            SpriteRenderer component = GetComponent<SpriteRenderer>();
            if (component != null)
            {
                component.color = new Color(0f, 1f, 1f);
            }
            SpriteOutline component2 = GetComponent<SpriteOutline>();
            if (component2 != null)
            {
                component2.enabled = true;
            }
            if (!(tail != null))
            {
                return;
            }
        }

        //Generic boss stuff ends here
        private void State_Default()
        {
            SetPlayerAnimation("FightStance");
            spriteRenderer.sprite = null;
            if (!bossActivated && FPStage.timeEnabled && targetPlayer != null && targetPlayer.position.x > position.x - bossActivation.x && targetPlayer.position.x < position.x + bossActivation.x && targetPlayer.position.y > position.y - bossActivation.y && targetPlayer.position.y < position.y + bossActivation.y)
            {
                if (bgmBoss != null)
                {
                    FPAudio.PlayMusic(bgmBoss);
                }
                Action_PlayVoice(vaStart[Random.Range(0, vaStart.Length - 1)]);
                isTalking = true;
                voiceTimer = 240f;
                bossActivated = true;
                FPBossHud component = GetComponent<FPBossHud>();
                if (component != null)
                {
                    component.MoveIn();
                }
                state = State_Running;
            }
            if (!FPStage.ConfirmClassWithPoolTypeID(typeof(FPPlayer), FPPlayer.classID))
            {
                bossActivated = true;
                FPBossHud component2 = GetComponent<FPBossHud>();
                if (component2 != null)
                {
                    component2.MoveIn();
                }
                state = State_Running;
            }
        }

        public void State_Frozen()
        {
            if (!frozen && freezeTimer > 0f)
            {
                iceBlockBack = Object.Instantiate(FPResources.childSprite[1]);
                iceBlockBack.parentObject = this;
                iceBlockBack.yOffset = 6f;
                iceBlock = Object.Instantiate(FPResources.childSprite[0]);
                iceBlock.parentObject = this;
                iceBlock.yOffset = 6f;
                frozen = true;
            }
            if (defrostTimer < 60f)
            {
                defrostTimer += FPStage.deltaTime;
                animator.SetSpeed(0f);
            }
            else
            {
                defrostTimer = 0f;
                DestroyIceBlock();
                animator.SetSpeed(1f);
                frozen = false;
                state = base.State_Init;
            }
            InteractWithObjects();
        }

        private new void State_Idle()
        {
            InteractWithObjects();
            Process360Movement();
            input.left = false;
            input.right = false;
            if (onGround)
            {
                ApplyGroundForces();
                angle = groundAngle;
                ApplyGroundAnimation();
                if (nextAttack <= 4 && FPStage.timeEnabled)
                {
                    SetPlayerAnimation("GuardRun");
                    genericTimer = 0f;
                    guardTime = 30f;
                    invincibility = Mathf.Max(invincibility, 20f);
                    GuardFlash guardFlash = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, position.x, position.y);
                    guardFlash.parentObject = this;
                    return;
                }
                if (genericTimer > 40f)
                {
                    state = State_Running;
                    if (!FPStage.timeEnabled)
                    {
                        genericTimer = -30f;
                    }
                    return;
                }
            }
            else
            {
                if (nextAttack <= 4)
                {
                    SetPlayerAnimation("GuardAir");
                    genericTimer = 0f;
                    guardTime = 30f;
                    invincibility = Mathf.Max(invincibility, 20f);
                    GuardFlash guardFlash2 = (GuardFlash)FPStage.CreateStageObject(GuardFlash.classID, position.x, position.y);
                    guardFlash2.parentObject = this;
                    return;
                }
                ApplyAirForces();
                ApplyGravityForce();
                if (currentAnimation == "Walking" || currentAnimation == "Running" || currentAnimation == "TopSpeed" || currentAnimation == "Hit1" || currentAnimation == "Hit2")
                {
                    SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    animator.SetSpeed(1f);
                }
            }
            ApplyGroundAnimation();
            genericTimer += FPStage.deltaTime;
        }

        private void State_Jumping()
        {
            InteractWithObjects();
            Process360Movement();
            RotatePlayerUpright();
            genericTimer += FPStage.deltaTime;
            if (onGround)
            {
                ApplyGroundForces();
                state = State_Running;
                return;
            }
            SetPlayerAnimation("Jumping");
            animator.SetSpeed(1f);
            ApplyAirForces();
            ApplyGravityForce();
            if (genericTimer > 0f && nextAttack == 2)
            {
                nextAttack = Random.Range(0, 1);
                comboState = 0;
                SetPlayerAnimation("AirAttack");
                animator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(velocity.x * 0.05f)));
                childAnimator.SetSpeed(Mathf.Max(1f, 0.7f + Mathf.Abs(velocity.x * 0.05f)));
                //state = State_HairWhip;
                combo = false;
                Action_StopSound();
            }
            else
            {
                if (!(genericTimer > 20f) || !(velocity.y < 0f))
                {
                    return;
                }
                genericTimer = 0f;
                switch (nextAttack)
                {
                    case 0:
                        SetPlayerAnimation("Rolling");
                        //state = State_DivekickPt1;
                        //Action_PlaySoundUninterruptable(sfxDivekick1);
                        nextAttack = Random.Range(0, 3);
                        break;
                    case 1:
                        velocity.y = Mathf.Max(velocity.y, 5f);
                        genericTimer = 0f;
                        SetPlayerAnimation("Cyclone");
                        //state = State_Cyclone;
                        jumpAbilityFlag = true;
                        //attackStats = base.AttackStats_Cyclone;
                        //Action_PlaySound(sfxCyclone);
                        if (voiceTimer <= 0f)
                        {
                            voiceTimer = 600f;
                            Action_PlayVoiceArray("SpecialA");
                        }
                        nextAttack = Random.Range(0, 3);
                        break;
                }
            }
        }

        private void State_Running()
        {
            InteractWithObjects();
            Process360Movement();
            if (!FPStage.timeEnabled)
            {
                energy = 0f;
            }
            if (onGround)
            {
                ApplyGroundForces();
                angle = groundAngle;
                ApplyGroundAnimation();
                if (energy >= 100f && genericTimer >= 0f)
                {
                    if (position.x >= start.x)
                    {
                        direction = FPDirection.FACING_LEFT;
                    }
                    else
                    {
                        direction = FPDirection.FACING_RIGHT;
                    }
                    genericTimer = 0f;
                    specialAttackDirection = 2;
                    //state = State_DragonBoostPt1;
                    //Action_PlaySoundUninterruptable(sfxBoostCharge);
                }
                else
                {
                    if (genericTimer > 40f && !FPStage.timeEnabled)
                    {
                        genericTimer = Random.Range(-60f, -30f);
                        state = State_Idle;
                        return;
                    }
                    if (genericTimer > 40f)
                    {
                        genericTimer = 0f;
                        velocity.y = 10f;
                        onGround = false;
                        state = State_Jumping;
                        Action_PlaySound(sfxJump);
                        return;
                    }
                }
            }
            else
            {
                ApplyAirForces();
                ApplyGravityForce();
                if (currentAnimation == "Walking" || currentAnimation == "Running" || currentAnimation == "TopSpeed" || currentAnimation == "Hit1" || currentAnimation == "Hit2")
                {
                    SetPlayerAnimation("Jumping", 0.5f, 0.5f);
                    animator.SetSpeed(1f);
                }
            }
            if (faction == "Player" && FPStage.FindNearestEnemy(this, pursuitRange, string.Empty) == null)
            {
                energy -= 0.4f * FPStage.deltaTime;
            }
            else
            {
                genericTimer += FPStage.deltaTime;
            }
            CheckBoundaries();
        }

        private void Action_Tyler_Kunai()
        {
            int kunaiNum = 1;
            if (!onGround) kunaiNum = 5;

            energy -= 10f;
            for (int i = 0; i < kunaiNum; i++)
            {

                float throwAngle = angle;
                if (!onGround)
                    throwAngle = angle - (kunaiAngle - 2) * 10f;


                FPAudio.PlaySfx(attackSfx);
                ProjectileBasic basicShot;
                if (direction == FPDirection.FACING_LEFT)
                {
                    basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x - Mathf.Cos(0.017453292f * throwAngle) * 32f + Mathf.Sin(0.017453292f * throwAngle) * (float)8f, position.y + Mathf.Cos(0.017453292f * throwAngle) * (float)8f - Mathf.Sin(0.017453292f * throwAngle) * 32f);
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * -16f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * -16f;
                }
                else
                {
                    basicShot = (ProjectileBasic)FPStage.CreateStageObject(ProjectileBasic.classID, position.x + Mathf.Cos(0.017453292f * throwAngle) * 32f + Mathf.Sin(0.017453292f * throwAngle) * (float)8f, position.y + Mathf.Cos(0.017453292f * throwAngle) * (float)8f + Mathf.Sin(0.017453292f * throwAngle) * 32f);
                    basicShot.velocity.x = Mathf.Cos(0.017453292f * throwAngle) * 16f;
                    basicShot.velocity.y = Mathf.Sin(0.017453292f * throwAngle) * 16f;
                }
                basicShot.animatorController = kunaiProjectile;
                basicShot.animator = basicShot.GetComponent<Animator>();
                basicShot.animator.runtimeAnimatorController = basicShot.animatorController;
                basicShot.attackPower = kunaiDamage;
                basicShot.direction = direction;
                if (direction == FPDirection.FACING_LEFT)
                    basicShot.direction = FPDirection.FACING_LEFT;
                else
                    basicShot.direction = FPDirection.FACING_RIGHT;
                basicShot.angle = angle;
                basicShot.damageElementType = -1;
                basicShot.explodeType = FPExplodeType.WHITEBURST;
                basicShot.ignoreTerrain = false;
                basicShot.explodeTimer = 50f;
                basicShot.destroyOnHit = true;
                basicShot.terminalVelocity = 0f;
                basicShot.gravityStrength = 0;
                basicShot.sfxExplode = null;
                basicShot.parentObject = this;
                basicShot.faction = faction;
                basicShot.timeBeforeCollisions = 0f;
                basicShot.halfHeight = 2;
                basicShot.halfWidth = 8;

                kunaiAngle++;
            }
            kunaiAngle = 0;
        }

        private void State_Tyler_Roll()
        {
            SetPlayerAnimation("Rolling");
            attackStats = AttackStats_CarolRoll;
            genericTimer += FPStage.deltaTime;
            if (onGround)
            {
                animator.SetSpeed(Mathf.Abs(groundVel) * 0.15f);
                if (input.jumpPress)
                {
                    Action_SoftJump();
                    animator.SetSpeed(2f);
                }
                else
                {
                    ApplyGroundForces();
                    angle = groundAngle;
                }
            }
            else
            {
                ApplyAirForces();
                ApplyGravityForce();
                if (!input.jumpHold && jumpReleaseFlag)
                {
                    jumpReleaseFlag = false;
                    if (velocity.y > jumpRelease)
                    {
                        velocity.y = jumpRelease;
                    }
                }
            }
            if (genericTimer > 15f && (!input.down || (onGround && Mathf.Abs(groundVel) < 2f) || (!onGround && velocity.y < 0f)))
            {
                if (onGround)
                {
                    state = State_Running;
                }
                else
                {
                    state = State_Jumping;
                }
            }
        }
    }

}
