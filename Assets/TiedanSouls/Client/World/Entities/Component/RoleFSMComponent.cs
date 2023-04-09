using UnityEngine;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Entities {

    public class RoleFSMComponent {

        #region [状态]

        RoleFSMState fsmState;
        public RoleFSMState FSMState => fsmState;

        RoleStateModel_Idle idleStateModel;
        public RoleStateModel_Idle IdleStateModel => idleStateModel;

        RoleStateModel_JumpingUp jumpingUpStateModel;
        public RoleStateModel_JumpingUp JumpingUpStateModel => jumpingUpStateModel;

        RoleStateModel_Casting castingStateModel;
        public RoleStateModel_Casting CastingStateModel => castingStateModel;

        RoleStateModel_SkillMove skillMoveStateModel;
        public RoleStateModel_SkillMove SkillMoveModel => skillMoveStateModel;

        RoleStateModel_BeHit beHitStateModel;
        public RoleStateModel_BeHit BeHitStateModel => beHitStateModel;

        RoleStateModel_Dying dyingStateModel;
        public RoleStateModel_Dying DyingStateModel => dyingStateModel;

        #endregion

        #region [控制状态]

        RoleCtrlStatus ctrlStatus;
        public RoleCtrlStatus CtrlStatus => ctrlStatus;

        RoleStateModel_KnockBack knockBackModel;
        public RoleStateModel_KnockBack KnockBackModel => knockBackModel;

        RoleStateModel_KnockUp knockUpModel;
        public RoleStateModel_KnockUp KnockUpModel => knockUpModel;

        #endregion

        #region [角色位置状态]

        RolePositionStatus positionStatus;
        public RolePositionStatus PositionStatus => positionStatus;

        RoleStateModel_OnGround onGroundModel;
        public RoleStateModel_OnGround OnGroundModel => onGroundModel;

        RoleStateModel_OnCrossPlatform onCrossPlatformStateModel;
        public RoleStateModel_OnCrossPlatform OnCrossPlatformStateModel => onCrossPlatformStateModel;

        RoleStateModel_InWater inWaterStateModel;
        public RoleStateModel_InWater InWaterStateModel => inWaterStateModel;

        #endregion

        public RoleFSMComponent() {
            idleStateModel = new RoleStateModel_Idle();
            jumpingUpStateModel = new RoleStateModel_JumpingUp();
            castingStateModel = new RoleStateModel_Casting();
            beHitStateModel = new RoleStateModel_BeHit();
            skillMoveStateModel = new RoleStateModel_SkillMove();
            knockBackModel = new RoleStateModel_KnockBack();
            knockUpModel = new RoleStateModel_KnockUp();
            dyingStateModel = new RoleStateModel_Dying();

            onGroundModel = new RoleStateModel_OnGround();
            onCrossPlatformStateModel = new RoleStateModel_OnCrossPlatform();
            inWaterStateModel = new RoleStateModel_InWater();
        }

        public void ResetAll() {
            ResetFSMState();
            ResetCtrlStatus();
            ResetPositionStatus();
        }

        public void ResetFSMState() {
            idleStateModel.Reset();
            castingStateModel.Reset();
            beHitStateModel.Reset();
            skillMoveStateModel.Reset();
            dyingStateModel.Reset();
            fsmState = RoleFSMState.None;
        }

        public void ResetCtrlStatus() {
            knockBackModel.Reset();
            knockUpModel.Reset();
            ctrlStatus = RoleCtrlStatus.None;
        }

        public void ResetPositionStatus() {
            onGroundModel.Reset();
            onCrossPlatformStateModel.Reset();
            inWaterStateModel.Reset();
            positionStatus = RolePositionStatus.None;
        }

        #region [状态]

        public void Enter_Idle() {
            var stateModel = idleStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);
            fsmState = RoleFSMState.Idle;
            TDLog.Log($"角色 状态 - 设置 '{fsmState}'");
        }


        public void Enter_Casting(SkillEntity skill, bool isCombo, Vector2 chosedPoint) {
            var stateModel = castingStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);
            stateModel.SetCastingSkill(skill);
            stateModel.SetIsCombo(isCombo);
            stateModel.SetChosedPoint(chosedPoint);
            fsmState = RoleFSMState.Casting;
            TDLog.Log($"角色 状态 - 切换  {fsmState} {skill.IDCom.TypeID} / 是否连招 {isCombo} / 选择点 {chosedPoint}");
        }

        public void Enter_BeHit(Vector3 beHitDir, in BeHitModel beHitModel) {
            var stateModel = beHitStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);
            stateModel.SetBeHitDir(beHitDir);
            stateModel.SetMaintainFrame(beHitModel.maintainFrame);
            stateModel.SetKnockBackSpeedArray(beHitModel.knockBackSpeedArray.Clone() as float[]);
            stateModel.SetKnockUpSpeedArray(beHitModel.knockUpSpeedArray.Clone() as float[]);
            fsmState = RoleFSMState.BeHit;
            TDLog.Log($"角色 状态 - 切换 '{fsmState}'");
        }

        public void Enter_Dying(int maintainFrame) {
            var stateModel = this.dyingStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);

            stateModel.maintainFrame = maintainFrame;

            fsmState = RoleFSMState.Dying;
            TDLog.Log($"角色 状态 - 切换 '{fsmState}'");
        }

        public void Enter_JumpingDown() {
            fsmState = RoleFSMState.JumpingDown;
            TDLog.Log($"角色 状态 - 切换 '{fsmState}'");
        }

        public void Enter_JumpingUp() {
            fsmState = RoleFSMState.JumpingUp;
            var stateModel = this.jumpingUpStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);
            TDLog.Log($"角色 状态 - 切换 '{fsmState}'");
        }

        public void Enter_Falling() {
            fsmState = RoleFSMState.Falling;
            TDLog.Log($"角色 状态 - 切换 '{fsmState}'");
        }

        #endregion

        #region [控制状态]

        public void AddCtrlStatus_SkillMove(in SkillMoveCurveModel skillMoveCurveModel) {
            var stateModel = skillMoveStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);
            stateModel.SetIsFaceTo(skillMoveCurveModel.isFaceTo);
            stateModel.SetNeedWaitForMoveEnd(skillMoveCurveModel.needWaitForMoveEnd);
            stateModel.SetMoveSpeedArray(skillMoveCurveModel.moveCurveModel.moveSpeedArray);
            stateModel.SetMoveDirArray(skillMoveCurveModel.moveCurveModel.moveDirArray);
            ctrlStatus = ctrlStatus.AddStatus(RoleCtrlStatus.SkillMove);
            TDLog.Log($"角色 控制状态 - 添加  '{RoleCtrlStatus.SkillMove}'\n{ctrlStatus.GetString()}");
        }

        public void RemoveCtrlStatus_SkillMove() {
            ctrlStatus = ctrlStatus.RemoveStatus(RoleCtrlStatus.SkillMove);
            TDLog.Log($"角色 控制状态 - 移除  '{RoleCtrlStatus.SkillMove}'\n{ctrlStatus.GetString()}");
        }

        #endregion

        #region [位置状态]

        public void AddPositionStatus_OnGround() {
            var stateModel = onGroundModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);

            this.positionStatus = this.positionStatus.AddStatus(RolePositionStatus.OnGround);
            TDLog.Log($"角色 位置状态 - 添加  '{RolePositionStatus.OnGround}'\n{positionStatus.GetString()}");
        }

        public void AddPositionStatus_StandInCrossPlatform() {
            var stateModel = onCrossPlatformStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);

            this.positionStatus = this.positionStatus.AddStatus(RolePositionStatus.OnCrossPlatform);
            TDLog.Log($"角色 位置状态 - 添加  '{RolePositionStatus.OnCrossPlatform}'\n{positionStatus.GetString()}");
        }

        public void AddPositionStatus_StandInWater() {
            var stateModel = inWaterStateModel;
            stateModel.Reset();
            stateModel.SetIsEntering(true);

            this.positionStatus = this.positionStatus.AddStatus(RolePositionStatus.InWater);
            TDLog.Log($"角色 位置状态 - 添加  '{RolePositionStatus.InWater}'\n{positionStatus.GetString()}");
        }

        public void RemovePositionStatus_OnGround() {
            this.positionStatus = positionStatus.RemoveStatus(RolePositionStatus.OnGround);
            TDLog.Log($"角色 位置状态 - 移除  '{RolePositionStatus.OnGround}'\n{positionStatus.GetString()}");
        }

        public void RemovePositionStatus_StandInCrossPlatform() {
            this.positionStatus = positionStatus.RemoveStatus(RolePositionStatus.OnCrossPlatform);
            TDLog.Log($"角色 位置状态 - 移除  '{RolePositionStatus.OnCrossPlatform}'\n{positionStatus.GetString()}");
        }

        public void RemovePositionStatus_StandInWater() {
            this.positionStatus = positionStatus.RemoveStatus(RolePositionStatus.InWater);
            TDLog.Log($"角色 位置状态 - 移除  '{RolePositionStatus.InWater}'\n{positionStatus.GetString()}");
        }

        #endregion

        #region [Locomotion 判断]

        /// <summary>
        /// 是否可以移动
        /// </summary>
        public bool Can_Move() {
            return fsmState != RoleFSMState.Dying
                && fsmState != RoleFSMState.JumpingDown
                && !ctrlStatus.Contains(RoleCtrlStatus.SkillMove)
                && !ctrlStatus.Contains(RoleCtrlStatus.Root)
                && !ctrlStatus.Contains(RoleCtrlStatus.Stun);
        }

        /// <summary>
        /// 是否可以下跳
        /// </summary>
        public bool CanJumpDown() {
            return fsmState != RoleFSMState.Dying
                && fsmState != RoleFSMState.JumpingUp
                && !ctrlStatus.Contains(RoleCtrlStatus.Root)
                && !ctrlStatus.Contains(RoleCtrlStatus.Stun)
                && positionStatus.Contains(RolePositionStatus.OnCrossPlatform);
        }

        /// <summary>
        /// 是否可以上跳
        /// </summary>
        public bool CanJumpUp() {
            return fsmState != RoleFSMState.Dying
                && fsmState != RoleFSMState.JumpingUp
                && !ctrlStatus.Contains(RoleCtrlStatus.Root)
                && !ctrlStatus.Contains(RoleCtrlStatus.Stun)
                && (positionStatus.Contains(RolePositionStatus.OnGround)
                    || positionStatus.Contains(RolePositionStatus.OnCrossPlatform)
                    || positionStatus.Contains(RolePositionStatus.InWater));
        }

        /// <summary>
        /// 是否会下落
        /// </summary>
        public bool CanFall() {
            return !ctrlStatus.Contains(RoleCtrlStatus.Stun)
                && !positionStatus.Contains(RolePositionStatus.OnGround)
                && !positionStatus.Contains(RolePositionStatus.OnCrossPlatform);
        }

        /// <summary>
        /// 是否可改变面向
        /// </summary>
        public bool CanChangeFaceTo() {
            return fsmState != RoleFSMState.Dying
                && !ctrlStatus.Contains(RoleCtrlStatus.Root)
                && !ctrlStatus.Contains(RoleCtrlStatus.Stun);
        }

        #endregion

    }

}