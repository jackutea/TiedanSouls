namespace TiedanSouls.Generic {

    public class RoleFSMModel_Dying {

        bool isEntering;
        public bool IsEntering => isEntering;
        public void SetIsEntering(bool value) => isEntering = value;

        public int maintainFrame;

        public RoleFSMModel_Dying() { }

        public void Reset() {
            isEntering = false;
        }

    }

}