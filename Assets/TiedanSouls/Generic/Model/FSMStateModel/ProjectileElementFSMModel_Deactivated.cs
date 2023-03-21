using UnityEngine;

namespace TiedanSouls.Generic {

    public class ProjectileElementFSMModel_Deactivated {

        bool isEntering;
        public bool IsEntering => isEntering;
        public void SetIsEntering(bool value) => isEntering = value;

        public ProjectileElementFSMModel_Deactivated() { }

        public void Reset() {
            isEntering = false;
        }

    }

}