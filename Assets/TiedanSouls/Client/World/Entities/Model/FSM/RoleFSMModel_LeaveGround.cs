namespace TiedanSouls.Client.Entities {

    public class RoleFSMModel_LeaveGround {

        bool isEntering;
        public bool IsEntering => isEntering;
        public void SetIsEntering(bool value) => isEntering = value;

        public RoleFSMModel_LeaveGround() { }

        public void Reset() {
            isEntering = false;
        }

    }
}