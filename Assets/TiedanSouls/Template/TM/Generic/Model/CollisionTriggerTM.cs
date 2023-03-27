using System;
using TiedanSouls.Generic;
using UnityEngine;

namespace TiedanSouls.Template {

    /// <summary>
    /// 碰撞触发器模型
    /// </summary>
    [Serializable]
    public struct CollisionTriggerTM {

        [Header("启用/关闭")] public bool isEnabled;

        [Header("总时间(帧)")] public int totalFrame;
        [Header("延时触发(帧)")] public int delayFrame;
        [Header("触发间隔(帧)")] public int intervalFrame;
        [Header("单次触发时间(帧)")] public int maintainFrame;

        [Header("作用目标")] public RelativeTargetGroupType relativeTargetGroupType;
        [Header("模型: 伤害")] public DamageTM damageTM;
        [Header("模型: 击退力度")] public KnockBackTM knockBackPowerTM;
        [Header("模型: 击飞力度")] public KnockUpTM knockUpPowerTM;
        [Header("模型: 状态影响")] public RoleStateEffectTM stateEffectTM;  // TODO ID
        [Header("模型: 效果器(类型ID)")] public int hitEffectorTypeID;

        [Header("碰撞盒(组)")] public ColliderTM[] colliderTMArray;

        [Header("(仅用于编辑器) 碰撞盒相对路径(组)")] public string[] colliderRelativePathArray;

    }

}