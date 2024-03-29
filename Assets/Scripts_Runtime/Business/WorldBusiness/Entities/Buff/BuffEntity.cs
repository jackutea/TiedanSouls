using TiedanSouls.Generic;

namespace TiedanSouls.Client.Entities {

    public class BuffEntity {

        public EntityIDComponent IDCom { get; private set; }

        string description;
        public string Description => description;
        public void SetDescription(string value) => description = value;

        string iconName;
        public string IconName => iconName;
        public void SetIconName(string value) => iconName = value;

        int delayFrame;
        public int DelayFrame => delayFrame;
        public void SetDelayFrame(int value) => delayFrame = value;

        int intervalFrame;
        public int IntervalFrame => intervalFrame;
        public void SetIntervalFrame(int value) => intervalFrame = value;

        int totalFrame;
        public int TotalFrame => totalFrame;
        public void SetTotalFrame(int value) => totalFrame = value;

        RoleModifyModel roleAttrModifyModel;
        public RoleModifyModel RoleAttrModifyModel => roleAttrModifyModel;
        public void SetRoleAttrModifyModel(RoleModifyModel value) => roleAttrModifyModel = value;

        int effectorTypeID;
        public int EffectorTypeID => effectorTypeID;
        public void SetEffectorTypeID(int value) => effectorTypeID = value;

        int maxExtraStackCount;
        public int MaxExtraStackCount => maxExtraStackCount;
        public void SetMaxExtraStackCount(int value) => maxExtraStackCount = value;

        int curFrame;
        public int CurFrame => curFrame;
        public void AddCurFrame() => curFrame++;
        public void ResetCurFrame() => curFrame = -1;

        int triggerTimes;
        public int TriggerTimes => triggerTimes;
        public void ResetTriggerTimes() => triggerTimes = 0;

        int extraStackCount;
        public int ExtraStackCount => extraStackCount;
        public void AddExtraStackCount() => extraStackCount++;
        public void ResetExtraStackCount() => extraStackCount = 0;

        public void Ctor() {
            IDCom = new EntityIDComponent();
            IDCom.SetEntityType(EntityType.Buff);
            curFrame = -1;
        }

        public void ResetAll() {
            curFrame = -1;
            triggerTimes = 0;
            extraStackCount = 0;
            roleAttrModifyModel.ResetOffset();
        }

        public void SetFather(in EntityIDComponent father) {
            IDCom.SetFather(father);
        }

        public bool IsFinished() {
            return curFrame >= totalFrame;
        }

        public bool CanTrigger() {
            return curFrame >= delayFrame && (curFrame - delayFrame) % (intervalFrame + 1) == 0;
        }

    }

}