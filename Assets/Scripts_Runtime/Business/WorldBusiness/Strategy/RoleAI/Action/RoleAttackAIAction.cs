using UnityEngine;
using GameArki.BTTreeNS;
using TiedanSouls.Client.Facades;
using TiedanSouls.Generic;

namespace TiedanSouls.Client.Entities {

    public class RoleAttackAIAction : IBTTreeAction {
        RoleEntity role;
        WorldContext worldContext;
        float attackRange;
        float attackCD;
        float time;

        public RoleAttackAIAction() : this(2, 1) { }

        public RoleAttackAIAction(float attackRange) : this(attackRange, 1) { }

        public RoleAttackAIAction(float attackRange, float attackCD) {
            this.attackRange = attackRange;
            this.attackCD = attackCD;
        }

        public void Inject(RoleEntity role, WorldContext worldContext) {
            this.role = role;
            this.worldContext = worldContext;
        }

        public void Enter() {
            time = attackCD;
            TDLog.Log("RoleAttackAIAction Enter");
        }

        public bool Execute() {
            var fsmCom = role.FSMCom;
            if (fsmCom.FSMState != RoleFSMState.BeHit) {
                return false;
            }

            Vector2 pos_role = role.RootPos;
            Vector2 pos_target = worldContext.RoleRepo.PlayerRole.RootPos;
            time += Time.deltaTime;
            //Attack CD
            if (time < attackCD) {
                if (Vector2.Distance(pos_role, pos_target) < 3 * attackRange) {
                    var input = role.InputCom;
                    var dir = (pos_role - pos_target).normalized;
                    input.SetMoveAxis(dir);
                    return true;
                }
            }

            //Attack Judge
            if (Vector2.Distance(pos_role, pos_target) > attackRange) {
                return false;
            }

            //TODO:攻击方式后续修改，暂时先凑合一下        
            var playRole = worldContext.RoleRepo.PlayerRole;
            worldContext.RootDomain.RoleDomain.ReduceHP(playRole, 30);
            time = 0;
            return true;

        }

        public void Exit() {
            time = attackCD;
            TDLog.Log("RoleAttackAIAction Exit");
        }
    }

}