using System;
using TiedanSouls.Generic;
using UnityEngine;

namespace TiedanSouls.Client.Entities {

    public class SkillEntity : IEntity {

        public EntityIDComponent IDCom { get; private set; }

        // - 技能类型
        SkillType skillType;
        public SkillType SkillType => this.skillType;
        public void SetSkillType(SkillType value) => this.skillType = value;

        // - 原始技能
        int originalSkillTypeID;
        public int OriginalSkillTypeID => this.originalSkillTypeID;
        public void SetOriginalSkillTypeID(int value) => this.originalSkillTypeID = value;

        // - 组合技
        SkillCancelModel[] comboSkillCancelModelArray;
        public SkillCancelModel[] ComboSkillCancelModelArray => this.comboSkillCancelModelArray;
        public void SetComboSkillCancelModelArray(SkillCancelModel[] value) => this.comboSkillCancelModelArray = value;

        // - 连招技
        SkillCancelModel[] linkSkillCancelModelArray;
        public SkillCancelModel[] LinkSkillCancelModelArray => this.linkSkillCancelModelArray;
        public void SetLinkSkillCancelModelArray(SkillCancelModel[] value) => this.linkSkillCancelModelArray = value;

        // - 效果器组
        SkillEffectorModel[] skillEffectorModelArray;
        public void SetSkillEffectorModelArray(SkillEffectorModel[] value) => this.skillEffectorModelArray = value;

        // - 技能位移组
        SkillMoveCurveModel[] skillMoveCurveModelArray;
        public void SetSkillMoveCurveModelArray(SkillMoveCurveModel[] value) => this.skillMoveCurveModelArray = value;

        // - 角色召唤组
        RoleSummonModel[] roleSummonModelArray;
        public void SetRoleSummonModelArray(RoleSummonModel[] value) => this.roleSummonModelArray = value;

        // - 弹幕生成组
        ProjectileCtorModel[] projectileCtorModelArray;
        public void SetProjectileCtorModelArray(ProjectileCtorModel[] value) => this.projectileCtorModelArray = value;

        // - Buff附加组
        BuffAttachModel[] buffAttachModelArray;
        public void SetBuffAttachModelArray(BuffAttachModel[] value) => this.buffAttachModelArray = value;

        // - 碰撞器组
        EntityColliderTriggerModel[] entityColliderTriggerModelArray;
        public EntityColliderTriggerModel[] EntityColliderTriggerModelArray => this.entityColliderTriggerModelArray;
        public void SetEntityColliderTriggerModelArray(EntityColliderTriggerModel[] value) => this.entityColliderTriggerModelArray = value;

        // - 表现层
        string weaponAnimName;
        public string WeaponAnimName => this.weaponAnimName;
        public void SetWeaponAnimName(string value) => this.weaponAnimName = value;

        // - 生命周期
        int totalFrame;
        public int TotalFrame => this.totalFrame;
        public void SetTotalFrame(int value) => this.totalFrame = value;

        int curFrame;
        public int CurFrame => this.curFrame;
        public void SetCurFrame(int value) => this.curFrame = value;

        public SkillEntity() {
            IDCom = new EntityIDComponent();
            IDCom.SetEntityType(EntityType.Skill);
        }

        public void Reset() {
            curFrame = -1;
            ResetAllColliderModel();
        }

        public void ResetAllColliderModel() {
            var colliderTriggerCount = entityColliderTriggerModelArray?.Length;
            for (int i = 0; i < colliderTriggerCount; i++) {
                var entityColliderTriggerModel = entityColliderTriggerModelArray[i];
                var entityColliderModelArray = entityColliderTriggerModel.entityColliderArray;
                var colliderCount = entityColliderModelArray.Length;
                for (int j = 0; j < colliderCount; j++) {
                    var entityColliderModel = entityColliderModelArray[j];
                    entityColliderModel.transform.position = entityColliderModel.ColliderModel.localPos;
                    entityColliderModel.transform.rotation = Quaternion.Euler(0, 0, entityColliderModel.ColliderModel.localAngleZ);
                    var localScale = entityColliderModel.ColliderModel.localScale;
                    entityColliderModel.transform.localScale = localScale;
                    entityColliderModel.Deactivate();
                }
            }
        }

        public void SetFather(in EntityIDArgs father) {
            IDCom.SetFather(father);
            var len = entityColliderTriggerModelArray.Length;
            var idArgs = IDCom.ToArgs();
            for (int i = 0; i < len; i++) {
                var triggerModel = entityColliderTriggerModelArray[i];
                var colliderModelArray = triggerModel.entityColliderArray;
                var colliderCount = colliderModelArray.Length;
                for (int j = 0; j < colliderCount; j++) {
                    var colliderModel = colliderModelArray[j];
                    colliderModel.SetFather(idArgs);
                }
            }
        }

        public bool TryApplyFrame(Vector3 rootPos, Quaternion rootRot, int frame) {
            if (frame > totalFrame) {
                return false;
            }

            // 碰撞盒控制
            Foreach_CollisionTrigger(TriggerBegin, Triggering, TriggerEnd);
            return true;

            void Foreach_CollisionTrigger(
                      Action<EntityColliderTriggerModel> action_triggerBegin,
                      Action<EntityColliderTriggerModel> action_triggering,
                      Action<EntityColliderTriggerModel> action_triggerEnd) {
                if (entityColliderTriggerModelArray != null) {
                    for (int i = 0; i < entityColliderTriggerModelArray.Length; i++) {
                        EntityColliderTriggerModel model = entityColliderTriggerModelArray[i];
                        var triggerStatus = model.GetTriggerState(frame);
                        if (triggerStatus == TriggerState.None) continue;
                        if (triggerStatus == TriggerState.Enter) action_triggerBegin(model);
                        else if (triggerStatus == TriggerState.Stay) action_triggering(model);
                        else if (triggerStatus == TriggerState.Exit) action_triggerEnd(model);
                    }
                }
            }

            void TriggerBegin(EntityColliderTriggerModel triggerModel) => ActivateAllColliderModel(triggerModel, true);
            void Triggering(EntityColliderTriggerModel triggerModel) => ActivateAllColliderModel(triggerModel, true);
            void TriggerEnd(EntityColliderTriggerModel triggerModel) => ActivateAllColliderModel(triggerModel, false);

            void ActivateAllColliderModel(EntityColliderTriggerModel triggerModel, bool active) {
                var entityColliderModelArray = triggerModel.entityColliderArray;
                if (entityColliderModelArray == null) return;
                for (int i = 0; i < entityColliderModelArray.Length; i++) {
                    var entityColliderModel = entityColliderModelArray[i];
                    entityColliderModel.transform.position = rootPos + rootRot * entityColliderModel.ColliderModel.localPos;
                    entityColliderModel.transform.rotation = rootRot * Quaternion.Euler(0, 0, entityColliderModel.ColliderModel.localAngleZ);
                    if (active) entityColliderModel.Activate();
                    else entityColliderModel.Deactivate();
                }
            }
        }

        public bool TryGetSkillMoveCurveModel(int frame, out SkillMoveCurveModel skillMoveCurveModel) {
            if (frame < 0) {
                skillMoveCurveModel = default;
                return false;
            }

            var len = skillMoveCurveModelArray?.Length;
            for (int i = 0; i < len; i++) {
                var model = skillMoveCurveModelArray[i];
                var startFrame = model.triggerFrame;
                var endFrame = startFrame + model.moveCurveModel.moveDirArray.Length - 1;

                if (startFrame <= frame && frame <= endFrame) {
                    skillMoveCurveModel = model;
                    return true;
                }
            }

            skillMoveCurveModel = default;
            return false;
        }

        public bool HasSkillMoveCurveModel(int frame) {
            if (frame < 0) {
                return false;
            }

            var len = skillMoveCurveModelArray?.Length;
            for (int i = 0; i < len; i++) {
                var model = skillMoveCurveModelArray[i];
                var startFrame = model.triggerFrame;
                var endFrame = startFrame + model.moveCurveModel.moveDirArray.Length - 1;

                if (startFrame <= frame && frame <= endFrame) {
                    return true;
                }
            }

            return false;
        }

        public bool TryGet_ValidCollisionTriggerModel(out EntityColliderTriggerModel collisionTriggerModel, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (entityColliderTriggerModelArray != null) {
                for (int i = 0; i < entityColliderTriggerModelArray.Length; i++) {
                    EntityColliderTriggerModel model = entityColliderTriggerModelArray[i];
                    var triggerStatus = model.GetTriggerState(frame);
                    if (triggerStatus != TriggerState.None) {
                        collisionTriggerModel = model;
                        return true;
                    }
                }
            }
            collisionTriggerModel = default;
            return false;
        }

        public bool TryGet_ValidSkillEffectorModel(out SkillEffectorModel effectorModel, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (skillEffectorModelArray != null) {
                for (int i = 0; i < skillEffectorModelArray.Length; i++) {
                    SkillEffectorModel model = skillEffectorModelArray[i];
                    if (model.effectorTypeID == 0) continue;
                    if (model.triggerFrame == curFrame) {
                        effectorModel = model;
                        return true;
                    }
                }
            }
            effectorModel = default;
            return false;
        }

        public bool TryGet_ValidRoleSummonModel(out RoleSummonModel roleSummonModel, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (roleSummonModelArray != null) {
                for (int i = 0; i < roleSummonModelArray.Length; i++) {
                    RoleSummonModel model = roleSummonModelArray[i];
                    if (model.triggerFrame == curFrame) {
                        roleSummonModel = model;
                        return true;
                    }
                }
            }
            roleSummonModel = default;
            return false;
        }

        public bool TryGet_ValidProjectileCtorModel(out ProjectileCtorModel projectileCtorModel, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (projectileCtorModelArray != null) {
                for (int i = 0; i < projectileCtorModelArray.Length; i++) {
                    ProjectileCtorModel model = projectileCtorModelArray[i];
                    if (model.triggerFrame == curFrame) {
                        projectileCtorModel = model;
                        return true;
                    }
                }
            }
            projectileCtorModel = default;
            return false;
        }

        public bool TryGet_ValidBuffAttachModel(out BuffAttachModel buffAttachModel, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (buffAttachModelArray != null) {
                for (int i = 0; i < buffAttachModelArray.Length; i++) {
                    BuffAttachModel model = buffAttachModelArray[i];
                    if (model.triggerFrame == curFrame) {
                        buffAttachModel = model;
                        return true;
                    }
                }
            }
            buffAttachModel = default;
            return false;
        }

        public void Foreach_CancelModel_Linked(Action<SkillCancelModel> action, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (linkSkillCancelModelArray != null) {
                for (int i = 0; i < linkSkillCancelModelArray.Length; i++) {
                    SkillCancelModel model = linkSkillCancelModelArray[i];
                    if (model.IsInTriggeringFrame(curFrame)) action(model);
                }
            }
        }

        public void Foreach_CancelModel_Combo(Action<SkillCancelModel> action, int frame = -1) {
            frame = frame == -1 ? curFrame : frame;
            if (comboSkillCancelModelArray != null) {
                for (int i = 0; i < comboSkillCancelModelArray.Length; i++) {
                    SkillCancelModel model = comboSkillCancelModelArray[i];
                    if (model.IsInTriggeringFrame(curFrame)) action(model);
                }
            }
        }

        public bool IsKeyFrame(int frame) {
            return TryGet_ValidCollisionTriggerModel(out _, frame)
            || TryGet_ValidSkillEffectorModel(out _, frame)
            || TryGet_ValidRoleSummonModel(out _, frame)
            || TryGet_ValidProjectileCtorModel(out _, frame)
            || TryGet_ValidBuffAttachModel(out _, frame);
        }

    }

}