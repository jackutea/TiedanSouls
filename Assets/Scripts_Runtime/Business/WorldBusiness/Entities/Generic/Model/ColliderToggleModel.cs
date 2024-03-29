using TiedanSouls.Generic;

namespace TiedanSouls.Client.Entities {

    /// <summary>
    /// 实体碰撞器触发器模型
    /// </summary>
    public struct ColliderToggleModel {

        public bool isEnabled;

        // 触发模式
        public TriggerMode triggerMode;
        // 固定间隔
        public TriggerFixedIntervalModel triggerFixedIntervalModel;
        // 自定义
        public TriggerCustomModel triggerCustomModel;

        // 作用实体类型
        public EntityType targetEntityType;
        
        // 作用目标
        public AllyType hitAllyType;

        // 作用目标效果器
        public int[] targetRoleEffectorTypeIDArray;
        // 作用自身效果器
        public int[] selfRoleEffectorTypeIDArray;

        // 伤害
        public DamageModel damageModel;

        // 受击
        public BeHitModel beHitModel;

        // 控制效果组
        public RoleCtrlEffectModel[] roleCtrlEffectModelArray;

        public EntityCollider[] entityColliderArray;

        public ToggleState GetToggleState(int frame) {
            if (!isEnabled) return ToggleState.None;

            if (triggerMode == TriggerMode.Custom) {
                var triggerStateDic = triggerCustomModel.triggerStateDic;
                return triggerStateDic.TryGetValue(frame, out var triggerStatus) ? triggerStatus : ToggleState.None;
            }

            if (triggerMode == TriggerMode.FixedInterval) {
                var delayFrame = triggerFixedIntervalModel.delayFrame;
                var intervalFrame = triggerFixedIntervalModel.intervalFrame;
                var maintainFrame = triggerFixedIntervalModel.maintainFrame;
                if (frame < delayFrame) return ToggleState.None;
                if (frame == delayFrame) return ToggleState.Enter;

                var mod = (frame - delayFrame) % (intervalFrame + maintainFrame);
                if (mod == 0) return ToggleState.Enter;
                if (mod < intervalFrame) return ToggleState.Stay;
                if (mod == intervalFrame) return ToggleState.Exit;
                if (mod > intervalFrame) return ToggleState.None;
            }

            return ToggleState.None;
        }

        public void ActivateAll() {
            var len = entityColliderArray.Length;
            for (int i = 0; i < len; i++) {
                entityColliderArray[i].Activate();
            }
        }

        public void DeactivateAll() {
            var len = entityColliderArray.Length;
            for (int i = 0; i < len; i++) {
                entityColliderArray[i].Deactivate();
            }
        }

    }

}