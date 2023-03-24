using UnityEngine;
using TiedanSouls.Infra.Facades;
using TiedanSouls.Client.Facades;
using TiedanSouls.Client.Entities;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Domain {

    public class WorldPhysicsDomain {

        InfraContext infraContext;
        WorldContext worldContext;
        WorldRootDomain rootDomain;

        public WorldPhysicsDomain() {
        }

        public void Inject(InfraContext infraContext, WorldContext worldContext, WorldRootDomain worldDomain) {
            this.infraContext = infraContext;
            this.worldContext = worldContext;
            this.rootDomain = worldDomain;
        }

        public void Tick(float dt) {
            Physics2D.Simulate(dt);

            var collisionEventRepo = worldContext.CollisionEventRepo;
            while (collisionEventRepo.TryPick_Enter(out var ev)) {
                var idArgs1 = ev.A;
                var idArgs2 = ev.B;
                _ = rootDomain.TryGetEntityObj(idArgs1, out var entity1);
                _ = rootDomain.TryGetEntityObj(idArgs2, out var entity2);
                HandleTriggerEnter(entity1, entity2);
            }

            while (collisionEventRepo.TryPick_Exit(out var ev)) {
                var idArgs1 = ev.A;
                var idArgs2 = ev.B;
                _ = rootDomain.TryGetEntityObj(idArgs1, out var entity1);
                _ = rootDomain.TryGetEntityObj(idArgs2, out var entity2);
                HandleTriggerExit(entity1, entity2);
            }
        }

        #region [物理方法]


        #endregion

        #region [碰撞事件处理 Enter]

        void HandleTriggerEnter(IEntity entityA, IEntity entityB) {
            // 技能 & 角色 
            if (entityA is SkillEntity skillEntity && entityB is RoleEntity roleEntity) {
                HandleTriggerEnter_Skill_Role(skillEntity, roleEntity);
                return;
            }
            if (entityA is RoleEntity roleEntity2 && entityB is SkillEntity skillEntity2) {
                HandleTriggerEnter_Skill_Role(skillEntity2, roleEntity2);
                return;
            }

            // 子弹 & 角色
            if (entityA is BulletEntity bulletEntity && entityB is RoleEntity roleEntity5) {
                HandleTriggerEnter_Bullet_Role(bulletEntity, roleEntity5);
                return;
            }
            if (entityA is RoleEntity roleEntity6 && entityB is BulletEntity bulletEntity2) {
                HandleTriggerEnter_Bullet_Role(bulletEntity2, roleEntity6);
                return;
            }

            // 子弹 & 技能
            if (entityA is BulletEntity bulletEntity3 && entityB is SkillEntity skillEntity3) {
                HandleTriggerEnter_Bullet_Skill(bulletEntity3, skillEntity3);
                return;
            }
            if (entityA is SkillEntity skillEntity4 && entityB is BulletEntity bulletEntity4) {
                HandleTriggerEnter_Bullet_Skill(bulletEntity4, skillEntity4);
                return;
            }

            // 角色 & 角色
            if (entityA is RoleEntity roleEntity3 && entityB is RoleEntity roleEntity4) {
                HandleTriggerEnter_Role_Role(roleEntity3, roleEntity4);
                return;
            }

            // 子弹 & 子弹
            if (entityA is BulletEntity bulletEntity5 && entityB is BulletEntity bulletEntity6) {
                HandleTriggerEnter_BulletNBullet(bulletEntity5, bulletEntity6);
                return;
            }

            TDLog.Error($"未处理的碰撞事件<Trigger - Enter>:\n{entityA.IDCom}\n{entityB.IDCom}");
        }

        void HandleTriggerEnter_Skill_Role(SkillEntity skill, RoleEntity role) {
            if (!skill.TryGet_ValidCollisionTriggerModel(out var collisionTriggerModel)) {
                return;
            }

            _ = rootDomain.TryGetEntityObj(skill.IDCom.Father, out var fatherEntity);
            var casterRole = fatherEntity as RoleEntity;
            var casterPos = casterRole.GetPos_Logic();
            var rolePos = role.GetPos_Logic();
            var beHitDir = rolePos - casterPos;
            beHitDir.Normalize();

            var hitDomain = rootDomain.HitDomain;
            hitDomain.Role_BeHit(role, collisionTriggerModel, skill.CurFrame, beHitDir);
        }

        void HandleTriggerEnter_Bullet_Role(BulletEntity bullet, RoleEntity role) {
            if (!bullet.TryGet_ValidCollisionTriggerModel(out var collisionTriggerModel)) {
                return;
            }

            var rolePos = role.GetPos_Logic();
            var beHitDir = rolePos - bullet.Pos;
            beHitDir.Normalize();

            var hitDomain = rootDomain.HitDomain;
            hitDomain.Role_BeHit(role, collisionTriggerModel, bullet.FSMCom.ActivatedModel.curFrame, beHitDir);
        }

        void HandleTriggerEnter_Bullet_Skill(BulletEntity bullet, SkillEntity skill) {
            if (!bullet.TryGet_ValidCollisionTriggerModel(out var collisionTriggerModel)) {
                return;
            }

            var hitDomain = rootDomain.HitDomain;
            hitDomain.Skill_BeHit(skill, collisionTriggerModel, bullet.FSMCom.ActivatedModel.curFrame);
        }

        void HandleTriggerEnter_BulletNBullet(BulletEntity bullet1, BulletEntity bullet2) {
            if (!bullet1.TryGet_ValidCollisionTriggerModel(out var collisionTriggerModel1)) {
                return;
            }

            if (!bullet2.TryGet_ValidCollisionTriggerModel(out var collisionTriggerModel2)) {
                return;
            }

            var hitDomain = rootDomain.HitDomain;
            hitDomain.Bullet_BeHit(bullet1, collisionTriggerModel1, bullet2.FSMCom.ActivatedModel.curFrame);
            hitDomain.Bullet_BeHit(bullet2, collisionTriggerModel2, bullet1.FSMCom.ActivatedModel.curFrame);
        }

        #endregion

        #region [碰撞事件处理 Exit]
        void HandleTriggerExit(IEntity entityA, IEntity entityB) {
            // 角色 & 角色
            if (entityA is RoleEntity roleEntity3 && entityB is RoleEntity roleEntity4) {
                HandleTriggerExit_Role_Role(roleEntity3, roleEntity4);
                return;
            }

            // 子弹 & 子弹
            if (entityA is BulletEntity bulletEntity5 && entityB is BulletEntity bulletEntity6) {
                HandleTriggerExit_Bullet_Bullet(bulletEntity5, bulletEntity6);
                return;
            }

            // 技能 & 角色
            if (entityA is SkillEntity skillEntity && entityB is RoleEntity roleEntity) {
                HandleTriggerExit_Skill_Role(skillEntity, roleEntity);
                return;
            }
            if (entityA is RoleEntity roleEntity2 && entityB is SkillEntity skillEntity2) {
                HandleTriggerExit_Skill_Role(skillEntity2, roleEntity2);
                return;
            }

            // 子弹 & 角色
            if (entityA is BulletEntity bulletEntity && entityB is RoleEntity roleEntity5) {
                HandleTriggerExit_Bullet_Role(bulletEntity, roleEntity5);
                return;
            }
            if (entityA is RoleEntity roleEntity6 && entityB is BulletEntity bulletEntity2) {
                HandleTriggerExit_Bullet_Role(bulletEntity2, roleEntity6);
                return;
            }

            // 子弹 & 技能
            if (entityA is BulletEntity bulletEntity3 && entityB is SkillEntity skillEntity3) {
                HandleTriggerExit_Bullet_Skill(bulletEntity3, skillEntity3);
                return;
            }
            if (entityA is SkillEntity skillEntity4 && entityB is BulletEntity bulletEntity4) {
                HandleTriggerExit_Bullet_Skill(bulletEntity4, skillEntity4);
                return;
            }

            TDLog.Error($"未处理的碰撞事件<Trigger - Exit>:\n{entityA.IDCom}\n{entityB.IDCom}");
        }

        void HandleTriggerEnter_Role_Role(RoleEntity role1, RoleEntity role2) {
            // TDLog.Log($"碰撞事件<Trigger - Enter>:\n{role1.IDCom}\n{role2.IDCom}");
        }

        void HandleTriggerExit_Bullet_Bullet(BulletEntity bullet1, BulletEntity bullet2) {
            // TDLog.Log($"碰撞事件<Trigger - Exit>:\n{bullet1.IDCom}\n{bullet2.IDCom}");
        }

        void HandleTriggerExit_Skill_Role(SkillEntity skill, RoleEntity role) {
            // TDLog.Log($"碰撞事件<Trigger - Exit>:\n{skill.IDCom}\n{role.IDCom}");
        }

        void HandleTriggerExit_Role_Role(RoleEntity role1, RoleEntity role2) {
            // TDLog.Log($"碰撞事件<Trigger - Exit>:\n{role1.IDCom}\n{role2.IDCom}");
        }

        void HandleTriggerExit_Bullet_Role(BulletEntity bullet, RoleEntity role) {
            // TDLog.Log($"碰撞事件<Trigger - Exit>:\n{bullet.IDCom}\n{role.IDCom}");
        }

        void HandleTriggerExit_Bullet_Skill(BulletEntity bullet, SkillEntity skill) {
            // TDLog.Log($"碰撞事件<Trigger - Exit>:\n{bullet.IDCom}\n{skill.IDCom}");
        }

        #endregion

    }

}