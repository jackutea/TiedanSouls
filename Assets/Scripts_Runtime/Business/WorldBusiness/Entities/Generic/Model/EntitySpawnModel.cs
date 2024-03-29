using System;
using TiedanSouls.Generic;
using UnityEngine;

namespace TiedanSouls.Client {

    [Serializable]
    public struct EntitySpawnModel {

        public EntityType entityType;
        public int typeID;
        public ControlType controlType;
        public CampType campType;
        public Vector3 spawnPos;
        public bool isBoss;

    }

}