using TiedanSouls.Infra.Facades;
using TiedanSouls.Client.Facades;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Controller {

    public class WorldController {

        InfraContext infraContext;
        WorldContext worldContext;
        WorldRootDomain worldDomain;

        public WorldController() {
            worldContext = new WorldContext();
            worldDomain = new WorldRootDomain();
        }

        public void Inject(InfraContext infraContext) {

            this.infraContext = infraContext;

            worldContext.Factory.Inject(infraContext, worldContext);

            worldDomain.Inject(infraContext, worldContext);

        }

        public void Init() {
            infraContext.EventCenter.Listen_OnStartGameAct(() => {
                worldDomain.WorldFSMDomain.StartGame();
            });
        }

        float resTime;
        public void Tick(float dt) {
            // ==== Input ====
            worldDomain.RoleDomain.BackPlayerInput();

            // ==== Logic ====
            resTime += dt;
            var logicDT = GameCollection.LOGIC_INTERVAL_TIME;
            while (resTime >= logicDT) {
                // - Logic
                worldDomain.WorldFSMDomain.ApplyWorldState(logicDT);
                resTime -= logicDT;
            }

            // ==== Render ====
            worldDomain.WorldRendererDomain.Tick(dt);

            // Clear Input
            var roleRepo = worldContext.RoleRepo;
            roleRepo.Foreach_All((role) => {
                role.InputCom.Reset();
            });

        }

    }

}