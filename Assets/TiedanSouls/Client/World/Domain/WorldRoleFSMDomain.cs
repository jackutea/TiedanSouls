using UnityEngine;
using TiedanSouls.Infra.Facades;
using TiedanSouls.Client.Facades;
using TiedanSouls.Client.Entities;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Domain {

    public class WorldRoleFSMDomain {

        InfraContext infraContext;
        WorldContext worldContext;
        WorldRootDomain rootDomain;

        public WorldRoleFSMDomain() { }

        public void Inject(InfraContext infraContext, WorldContext worldContext, WorldRootDomain worldDomain) {
            this.infraContext = infraContext;
            this.worldContext = worldContext;
            this.rootDomain = worldDomain;
        }

        public void TickFSM(int curFieldTypeID, float dt) {
            worldContext.RoleRepo.Foreach_AI(curFieldTypeID, (role) => {
                var fsm = role.FSMCom;
                if (fsm.IsExited) return;

                if (fsm.StateFlag != StateFlag.Dying) {
                    role.AIStrategy.Tick(dt);
                }

                TickFSM(role, dt);
            });

            var playerRole = worldContext.RoleRepo.PlayerRole;
            if (playerRole != null) {
                TickFSM(playerRole, dt);
            }
        }

        #region [角色状态 - Tick]

        void TickFSM(RoleEntity role, float dt) {
            var fsm = role.FSMCom;
            if (fsm.IsExited) return;

            // - 1. Tick 状态
            var stateFlag = fsm.StateFlag;
            if (stateFlag.Contains(StateFlag.Idle)) Tick_Idle(role, fsm, dt);
            if (stateFlag.Contains(StateFlag.Cast)) Tick_Cast(role, fsm, dt);
            if (stateFlag.Contains(StateFlag.KnockUp)) Tick_KnockUp(role, fsm, dt);
            if (stateFlag.Contains(StateFlag.KnockBack)) Tick_KnockBack(role, fsm, dt);
            if (stateFlag.Contains(StateFlag.Dying)) Tick_Dying(role, fsm, dt);
            Tick_AnyState(role, fsm, dt);

            // - 2. Apply 各项处理
            Apply_Locomotion(role, fsm, dt);    // 移动
            Apply_RealseSkill(role, fsm, dt);   // 释放技能
        }

        /// <summary>
        /// 任意状态
        /// </summary>
        void Tick_AnyState(RoleEntity role, RoleFSMComponent fsm, float dt) {
            if (fsm.StateFlag == StateFlag.Dying) return;

            var roleDomain = rootDomain.RoleDomain;

            // 任意状态下的死亡判定
            if (roleDomain.IsRoleDead(role)) {
                roleDomain.TearDownRole(role);
            }

            // 任意状态下的Idle设置判定
            if (fsm.NeedSetIdle()) {
                fsm.SetIdle();
            }
        }

        /// <summary>
        /// 闲置状态
        /// </summary>
        void Tick_Idle(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var stateModel = fsm.IdleModel;
            if (stateModel.IsEntering) {
                stateModel.SetIsEntering(false);
                role.RendererModCom.Anim_PlayIdle();
            }

            var roleDomain = rootDomain.RoleDomain;

            // 拾取武器
            var inputCom = role.InputCom;
            if (inputCom.HasInput_Basic_Pick) {
                roleDomain.TryPickUpSomethingFromField(role);
            }

            // 面向移动方向
            roleDomain.FaceToMoveDir(role);
        }

        /// <summary>
        /// 释放技能状态
        /// </summary>
        void Tick_Cast(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var stateModel = fsm.CastingModel;
            var skillTypeID = stateModel.CastingSkillTypeID;
            var isCombo = stateModel.IsCombo;
            var skillSlotCom = role.SkillSlotCom;
            _ = skillSlotCom.TryGet(skillTypeID, isCombo, out var castingSkill);
            var roleDomain = rootDomain.RoleDomain;

            if (stateModel.IsEntering) {
                stateModel.SetIsEntering(false);
                roleDomain.FaceTo_Horizontal(role, stateModel.ChosedPoint);
                role.WeaponSlotCom.Weapon.PlayAnim(castingSkill.WeaponAnimName);
            }

            // 技能效果器
            if (castingSkill.TryGet_ValidSkillEffectorModel(out var skillEffectorModel)) {
                var triggerFrame = skillEffectorModel.triggerFrame;
                var effectorTypeID = skillEffectorModel.effectorTypeID;
                var effectorDomain = this.rootDomain.EffectorDomain;
                if (!effectorDomain.TrySpawnEffectorModel(effectorTypeID, out var effectorModel)) {
                    Debug.LogError($"请检查配置! 效果器没有找到! 类型ID {effectorTypeID}");
                    return;
                }

                var summoner = role.IDCom.ToArgs();
                var baseRot = role.LogicRotation;
                var summonPos = role.LogicPos + baseRot * skillEffectorModel.offsetPos;

                this.rootDomain.SpawnBy_EntitySummonModelArray(summonPos, baseRot, summoner, effectorModel.entitySummonModelArray);
                this.rootDomain.DestroyBy_EntityDestroyModelArray(summoner, effectorModel.entityDestroyModelArray);
            }

            // 技能逻辑迭代
            if (!castingSkill.TryMoveNext(role.LogicPos, role.LogicRotation)) {
                fsm.RemoveCast();
            }
        }

        /// <summary>
        /// 被击退状态
        /// </summary>
        void Tick_KnockBack(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var stateModel = fsm.KnockBackModel;
            if (stateModel.IsEntering) {
                stateModel.SetIsEntering(false);
            }

            stateModel.curFrame++;
            var curFrame = stateModel.curFrame;

            var moveCom = role.MoveCom;
            var beHitDir = stateModel.beHitDir;
            var knockBackSpeedArray = stateModel.knockBackSpeedArray;
            var len = knockBackSpeedArray.Length;
            bool canKnockBack = curFrame < len;
            if (canKnockBack) {
                beHitDir = beHitDir.x > 0 ? Vector2.right : Vector2.left;
                moveCom.Set_Horizontal(beHitDir * knockBackSpeedArray[curFrame]);
            } else if (curFrame == len) {
                moveCom.Stop_Horizontal();
                fsm.RemoveKnockBack();
            }
        }

        /// <summary>
        /// 被击飞状态
        /// </summary>
        void Tick_KnockUp(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var stateModel = fsm.KnockUpModel;
            if (stateModel.IsEntering) {
                stateModel.SetIsEntering(false);
            }

            stateModel.curFrame++;
            var curFrame = stateModel.curFrame;

            var moveCom = role.MoveCom;

            var roleDomain = rootDomain.RoleDomain;
            var knockUpSpeedArray = stateModel.knockUpSpeedArray;
            var len = knockUpSpeedArray.Length;
            bool canKnockUp = curFrame < len;
            if (canKnockUp) {
                var newV = moveCom.Velocity;
                newV.y = knockUpSpeedArray[curFrame];
                moveCom.Set_Vertical(newV);
            } else if (curFrame == len) {
                moveCom.Stop_Vertical();
                fsm.RemoveKnockUp();
            } else {
                roleDomain.Fall(role, dt);
            }

        }

        /// <summary>
        /// 死亡状态
        /// </summary>
        void Tick_Dying(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var stateModel = fsm.DyingModel;

            if (stateModel.IsEntering) {
                stateModel.SetIsEntering(false);

                role.HudSlotCom.HideHUD();
                role.RendererModCom.Anim_Play_Dying();
                role.MoveCom.Stop();
            }

            var roleDomain = rootDomain.RoleDomain;
            roleDomain.Fall(role, dt);

            stateModel.maintainFrame--;
            if (stateModel.maintainFrame <= 0) {
                roleDomain.TearDownRole(role);
            }
        }

        #endregion

        #region [角色各项处理] 

        /// <summary>
        /// 处理 运动状态
        /// </summary>
        void Apply_Locomotion(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var roleDomain = rootDomain.RoleDomain;
            if (fsm.CanMove()) roleDomain.MoveByInput(role);
            if (fsm.CanJump()) roleDomain.JumpByInput(role);
            if (fsm.CanFall()) roleDomain.Fall(role, dt);
        }

        /// <summary>
        /// 处理 技能释放
        /// </summary>
        void Apply_RealseSkill(RoleEntity role, RoleFSMComponent fsm, float dt) {
            var roleDomain = rootDomain.RoleDomain;

            // 普通技能
            if (fsm.CanCast_NormalSkill()) {
                _ = roleDomain.TryCastSkillByInput(role);
            }

            // TODO: 觉醒技能........
        }

        #endregion

        #region [角色状态 - 进入]

        public void Role_EnterDying(RoleEntity role) {
            var roleRepo = worldContext.RoleRepo;
            var fsm = role.FSMCom;
            fsm.AddDying(30);
        }

        #endregion

    }
}