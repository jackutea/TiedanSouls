using System;
using UnityEngine;
using GameArki.FPEasing;
using TiedanSouls.Infra.Facades;
using TiedanSouls.World.Facades;
using TiedanSouls.Generic;
using TiedanSouls.World.Entities;

namespace TiedanSouls.World.Domain {

    public class WorldFSMDomain {

        InfraContext infraContext;
        WorldContext worldContext;
        WorldDomain worldDomain;

        public WorldFSMDomain() { }

        public void Inject(InfraContext infraContext, WorldContext worldContext, WorldDomain worldDomain) {
            this.infraContext = infraContext;
            this.worldContext = worldContext;
            this.worldDomain = worldDomain;
        }

        public void StartGame() {
            var gameConfigTM = infraContext.TemplateCore.GameConfigTM;
            var lobbyFieldTypeID = gameConfigTM.lobbyFieldTypeID;

            var stateEntity = worldContext.StateEntity;
            stateEntity.EnterState_Loading(-1, lobbyFieldTypeID, 0);
        }

        public void ApplyWorldState(float dt) {
            var stateEntity = worldContext.StateEntity;
            var worldState = stateEntity.State;
            if (worldState == WorldFSMState.Exit) {
                return;
            }

            if (worldState == WorldFSMState.Lobby) {
                Apply_Lobby(dt);
            } else if (worldState == WorldFSMState.Battle) {
                Apply_Battle(dt);
            } else if (worldState == WorldFSMState.Store) {
                Apply_Store(dt);
            } else if (worldState == WorldFSMState.Loading) {
                Apply_Loading(dt);
            }

            // Clear Input
            var roleRepo = worldContext.RoleRepo;
            roleRepo.ForeachAll((role) => {
                role.InputCom.Reset();
            });
        }

        public void Apply_Lobby(float dt) {
            var stateEntity = worldContext.StateEntity;
            var lobbyStateModel = stateEntity.LobbyStateModel;

            var roleRepo = worldContext.RoleRepo;
            RoleEntity playerRole = roleRepo.PlayerRole;

            if (lobbyStateModel.IsEntering) {
                lobbyStateModel.SetIsEntering(false);

                // ???????????? 
                var gameConfigTM = infraContext.TemplateCore.GameConfigTM;
                var tieDanRoleTypeID = gameConfigTM.tiedanRoleTypeID;

                var roleDomain = worldDomain.RoleDomain;
                playerRole = roleRepo.PlayerRole;
                playerRole ??= roleDomain.SpawnRole(ControlType.Player, tieDanRoleTypeID, AllyType.Player, new Vector2(5, 5));
                playerRole.Reset();
                playerRole.Show();
                playerRole.HudSlotCom.ShowHUD();

                // ??????????????????
                playerRole.WeaponSlotCom.SetWeaponActive(false);

                // ???????????? 
                _ = worldContext.FieldRepo.TryGet(stateEntity.CurFieldTypeID, out var field);
                var cameraSetter = infraContext.CameraCore.SetterAPI;
                cameraSetter.Follow_Current(playerRole.transform, new Vector3(0, 0, -10), EasingType.Immediate, 1f, EasingType.Linear, 1f);
                cameraSetter.Confiner_Set_Current(true, field.transform.position, (Vector2)field.transform.position + field.ConfinerSize);
            }

            var roleFSMDomain = worldDomain.RoleFSMDomain;

            // Player
            roleFSMDomain.TickFSM(playerRole, dt);

            // AI
            var curFieldTypeID = stateEntity.CurFieldTypeID;
            roleRepo.Foreach_AIFromField(curFieldTypeID, (role) => {
                // Strategy
                if (role.FSMCom.State != RoleFSMState.Dying) {
                    role.AIStrategy.Tick(dt);
                }

                // Role FSM
                roleFSMDomain.TickFSM(role, dt);

                // HUD
                if (role.IDCom.AllyType == AllyType.Enemy) role.HudSlotCom.HpBarHUD.SetColor(Color.red);
                else if (role.IDCom.AllyType == AllyType.Neutral) role.HudSlotCom.HpBarHUD.SetColor(Color.yellow);
            });

            // Physics
            var phxDomain = worldDomain.WorldPhysicsDomain;
            phxDomain.Tick(dt);

            // ?????????????????????
            var fieldFSMDomain = worldDomain.FieldFSMDomain;
            fieldFSMDomain.TickFSM_CurrentField(dt);

            // ????????????????????????????????????: ???????????????
            if (!IsTieDanWantToLeave(out var door)) {
                return;
            }

            if (!playerRole.WeaponSlotCom.HasWeapon()) {
                TDLog.Warning("???????????????????????????!");
                return;
            }

            var nextFieldTypeID = door.fieldTypeID;
            var fieldDomain = worldDomain.FieldDomain;
            if (!fieldDomain.TryGetOrSpawnField(nextFieldTypeID, out var nextField)) {
                TDLog.Error($"???????????????! ?????????????????????! FieldTypeID: {nextFieldTypeID}");
                return;
            }

            if (nextField.FieldType == FieldType.BattleField) {
                var doorIndex = door.doorIndex;
                stateEntity.EnterState_Loading(curFieldTypeID, nextFieldTypeID, doorIndex);
                return;
            }
        }

        public void Apply_Battle(float dt) {
            var roleRepo = worldContext.RoleRepo;
            var playerRole = roleRepo.PlayerRole;
            var stateEntity = worldContext.StateEntity;
            var battleFieldStateModel = stateEntity.BattleStateModel;
            if (battleFieldStateModel.IsEntering) {
                battleFieldStateModel.SetIsEntering(false);
                playerRole.WeaponSlotCom.SetWeaponActive(true);
            }

            var roleFSMDomain = worldDomain.RoleFSMDomain;

            // Player
            if (playerRole != null) {
                if (playerRole.FSMCom.IsExiting) {
                    var gameConfigTM = infraContext.TemplateCore.GameConfigTM;
                    var lobbyFieldTypeID = gameConfigTM.lobbyFieldTypeID;
                    stateEntity.EnterState_Loading(stateEntity.CurFieldTypeID, lobbyFieldTypeID, 0);
                } else {
                    roleFSMDomain.TickFSM(playerRole, dt);
                }
            }

            // AI
            var curFieldTypeID = stateEntity.CurFieldTypeID;
            roleRepo.Foreach_AIFromField(curFieldTypeID, (role) => {
                // Strategy
                if (role.FSMCom.State != RoleFSMState.Dying) {
                    role.AIStrategy.Tick(dt);
                }

                // Role FSM
                roleFSMDomain.TickFSM(role, dt);

                // HUD
                if (role.IDCom.AllyType == AllyType.Enemy) {
                    role.HudSlotCom.HpBarHUD.SetColor(Color.red);
                } else if (role.IDCom.AllyType == AllyType.Neutral) {
                    role.HudSlotCom.HpBarHUD.SetColor(Color.yellow);
                }
            });

            // Physics
            var phxDomain = worldDomain.WorldPhysicsDomain;
            phxDomain.Tick(dt);

            // ?????????????????????
            var fieldFSMDomain = worldDomain.FieldFSMDomain;
            fieldFSMDomain.TickFSM_CurrentField(dt);

            // ????????????????????????????????????: ??????????????????????????????????????????????????????????????????
            if (!IsTieDanWantToLeave(out var door)) {
                return;
            }

            var fieldDomain = worldDomain.FieldDomain;

            // -??????????????????????????????
            var curField = fieldDomain.GetCurField();
            var fieldFSM = curField.FSMComponent;
            if (fieldFSM.State != FieldFSMState.Finished) {
                TDLog.Warning("?????????????????????-------------------");
                return;
            }

            // - TODO: ??????????????????????????????

            var nextFieldTypeID = door.fieldTypeID;
            var doorIndex = door.doorIndex;
            stateEntity.EnterState_Loading(curFieldTypeID, nextFieldTypeID, doorIndex);
        }

        void Apply_Store(float dt) {
        }

        void Apply_Loading(float dt) {
            var stateEntity = worldContext.StateEntity;
            var loadingStateModel = stateEntity.LoadingStateModel;
            var loadingFieldTypeID = loadingStateModel.NextFieldTypeID;
            var fieldDomain = worldDomain.FieldDomain;

            if (loadingStateModel.IsEntering) {
                loadingStateModel.SetIsEntering(false);

                // ???????????????????????????????????????
                var fieldRepo = worldContext.FieldRepo;
                if (!fieldRepo.TryGet(loadingFieldTypeID, out var field)) {
                    fieldDomain.TryGetOrSpawnField(loadingFieldTypeID, out field);
                }

                if (field == null) {
                    TDLog.Error($"???????????????! FieldTypeID: {loadingFieldTypeID}");
                    return;
                }

                loadingStateModel.SetIsLoadingCompleted(true);
                field.Hide();
            }

            if (loadingStateModel.IsLoadingCompleted) {
                if (loadingStateModel.completeLoadingDelayFrame > 0) {
                    loadingStateModel.completeLoadingDelayFrame--;
                    return;
                }

                // ??????????????????
                var curFieldTypeID = stateEntity.CurFieldTypeID;
                fieldDomain.HideField(curFieldTypeID);

                // ???????????????????????????
                var itemRepo = worldContext.ItemRepo;
                itemRepo.HideAllItemsInField(curFieldTypeID);

                // ?????????????????????AI??????
                var roleRepo = worldContext.RoleRepo;
                roleRepo.HideAllAIRolesInField(curFieldTypeID);

                // ??????????????????
                _ = worldContext.FieldRepo.TryGet(loadingFieldTypeID, out var field);
                field.Show();

                // ??????????????????
                if (field.FieldType == FieldType.BattleField) {
                    stateEntity.EnterState_Battle(loadingFieldTypeID);
                } else if (field.FieldType == FieldType.Store) {
                    stateEntity.EnterState_Store(loadingFieldTypeID);
                } else if (field.FieldType == FieldType.Lobby) {
                    stateEntity.EnterState_Lobby(loadingFieldTypeID);
                } else {
                    TDLog.Warning($"????????????????????????: {field.FieldType}");
                }

                // ??????????????????
                var doorIndex = loadingStateModel.DoorIndex;
                _ = field.TryFindDoorByIndex(doorIndex, out var door);
                field.FSMComponent.Enter_Ready(door);
            }
        }

        bool IsTieDanWantToLeave(out FieldDoorModel door) {
            door = default;
            var stateEntity = worldContext.StateEntity;
            // ????????????InputComponent????????????????????????????????????
            var fieldRepo = worldContext.FieldRepo;
            var playerRole = worldContext.RoleRepo.PlayerRole;
            var inputCom = playerRole.InputCom;
            if (inputCom.HasInput_Basic_Pick) {
                var curFieldTypeID = stateEntity.CurFieldTypeID;
                if (!fieldRepo.TryGet(curFieldTypeID, out var curField)) {
                    TDLog.Error($"???????????????! ?????????????????????! FieldTypeID: {stateEntity.CurFieldTypeID}");
                    return false;
                }

                // ????????????????????????????????????
                var allDoors = curField.FieldDoorArray;
                var count = allDoors?.Length;
                for (int i = 0; i < count; i++) {
                    var d = allDoors[i];
                    var pos = d.pos;
                    var rolePos = playerRole.GetPos_LogicRoot();
                    if (Vector2.SqrMagnitude(pos - rolePos) > 1f) {
                        continue;
                    }

                    door = d;
                    return true;
                }
            }

            return false;
        }

    }
}