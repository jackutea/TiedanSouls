using System;
using UnityEngine;
using TiedanSouls.Generic;

namespace TiedanSouls.EditorTool {

    [Serializable]
    public struct RoleSummonEM {

        [Header("类型ID")] public int typeID;
        [Header("控制类型")] public ControlType controlType;

    }

}