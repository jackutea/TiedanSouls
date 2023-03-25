using System;
using System.Collections.Generic;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Entities {

    /// <summary>
    /// 碰撞触发器模型
    /// </summary>
    public struct CollisionTriggerModel {

        public bool isEnabled;

        public int totalFrame;

        // 触发状态字典
        public Dictionary<int, TriggerStatus> triggerStatusDic;

        // 作用目标
        public RelativeTargetGroupType relativeTargetGroupType;
        // 模型: 伤害
        public DamageModel damageModel;
        // 模型: 击退力度
        public KnockBackModel knockBackPowerModel;
        // 模型: 击飞力度
        public KnockUpModel knockUpPowerModel;
        // 模型: 击中效果器
        public int hitEffectorTypeID;
        // 模型: 状态影响
        public StateEffectModel stateEffectModel;

        public ColliderModel[] colliderModelArray;

        public TriggerStatus GetTriggerStatus(int frame) {
            if (!isEnabled) return TriggerStatus.None;
            return triggerStatusDic.TryGetValue(frame, out var triggerStatus) ? triggerStatus : TriggerStatus.None;
        }

        public void ActivateAll() {
            var len = colliderModelArray.Length;
            for (int i = 0; i < len; i++) {
                colliderModelArray[i].Activate();
            }
        }

        public void DeactivateAll() {
            var len = colliderModelArray.Length;
            for (int i = 0; i < len; i++) {
                colliderModelArray[i].Deactivate();
            }
        }

    }

}