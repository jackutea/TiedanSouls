using TiedanSouls.Infra.Facades;
using TiedanSouls.World.Facades;
using TiedanSouls.World.Entities;

namespace TiedanSouls.World.Domain {

    public class WorldFieldDomain {

        InfraContext infraContext;
        WorldContext worldContext;

        public WorldFieldDomain() { }

        public void Inject(InfraContext infraContext, WorldContext worldContext) {
            this.infraContext = infraContext;
            this.worldContext = worldContext;
        }

        public bool TryGetOrSpawnField(int typeID, out FieldEntity field) {
            var fieldRepo = worldContext.FieldRepo;
            if (!fieldRepo.TryGet(typeID, out field)) {
                var factory = worldContext.WorldFactory;
                if (!factory.TrySpawnFieldEntity(typeID, out field)) {
                    return false;
                }

                fieldRepo.Add(field);
            }

            field.Show();
            return true;
        }

        public void HideField(int typeID) {
            var fieldRepo = worldContext.FieldRepo;
            if (!fieldRepo.TryGet(typeID, out FieldEntity field)) {
                return;
            }

            field.Hide();
        }

        public FieldEntity GetCurField() {
            var stateEntity = worldContext.StateEntity;
            var curFieldTypeID = stateEntity.CurFieldTypeID;
            var fieldRepo = worldContext.FieldRepo;
            if (!fieldRepo.TryGet(curFieldTypeID, out var field)) {
                TDLog.Log($"关卡不存在!: {curFieldTypeID}");
            }

            return field;
        }

        public bool HasFieldLoadBefore(int typeID) {
            var fieldRepo = worldContext.FieldRepo;
            return fieldRepo.TryGet(typeID, out var _);
        }

    }
}