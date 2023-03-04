using UnityEngine;

namespace TiedanSouls.World.Entities {

    public class RoleFSMComponent {

        RoleFSMStatus status;
        public RoleFSMStatus Status => status;

        RoleIdleStateModel idleState;
        public RoleIdleStateModel IdleState => idleState;

        RoleCastingStateModel castingState;
        public RoleCastingStateModel CastingState => castingState;

        RoleBeHitStateModel beHurtState;
        public RoleBeHitStateModel BeHitState => beHurtState;

        RoleDeadStateModel deadState;
        public RoleDeadStateModel DeadState => deadState;

        public RoleFSMComponent() {
            status = RoleFSMStatus.Idle;
            idleState = new RoleIdleStateModel();
            castingState = new RoleCastingStateModel();
            beHurtState = new RoleBeHitStateModel();
            deadState = new RoleDeadStateModel();
        }

        public void Reset(){
            EnterIdle();
        }

        public void EnterIdle() {
            status = RoleFSMStatus.Idle;
            idleState.isEnter = true;
            TDLog.Log("人物状态机切换 - 待机 ");
        }

        public void EnterCasting(SkillorModel skillorModel, bool isCombo) {
            status = RoleFSMStatus.Casting;
            var stateModel = castingState;
            stateModel.castingSkillor = skillorModel;
            stateModel.SetIsEntering(true);
            TDLog.Log($"人物状态机切换 - 施放技能TypeID {skillorModel.TypeID} 连击 {isCombo}");
        }

        public void EnterBeHit(Vector2 fromPos, HitPowerModel hitPowerModel) {
            status = RoleFSMStatus.BeHit;
            var stateModel = beHurtState;
            stateModel.fromPos = fromPos;
            stateModel.knockbackForce = hitPowerModel.knockbackForce;
            stateModel.knockbackFrame = hitPowerModel.knockbackFrame;
            stateModel.hitStunFrame = hitPowerModel.hitStunFrame;
            stateModel.curFrame = 0;
            stateModel.isEnter = true;
            TDLog.Log("人物状态机切换 - 受伤 ");
        }

        public void EnterDead() {
            status = RoleFSMStatus.Dead;
            TDLog.Log("人物状态机切换 - 死亡 ");
        }

    }
}