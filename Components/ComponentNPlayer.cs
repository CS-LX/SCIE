using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game {
    public class ComponentNPlayer : ComponentPlayer, IUpdateable {
        public new void Update(float dt) {
            PlayerInput playerInput = ComponentInput.PlayerInput;
            if (ComponentInput.IsControlledByTouch
                && m_aim.HasValue) {
                playerInput.Look = Vector2.Zero;
            }
            if (ComponentMiner.Inventory != null) {
                ComponentMiner.Inventory.ActiveSlotIndex += playerInput.ScrollInventory;
                if (playerInput.SelectInventorySlot.HasValue) {
                    ComponentMiner.Inventory.ActiveSlotIndex = Math.Clamp(playerInput.SelectInventorySlot.Value, 0, 9);
                }
            }
            if (m_subsystemTime.PeriodicGameTimeEvent(0.5, 0)) {
                ReadOnlyList<int> readOnlyList = ComponentClothing.GetClothes(ClothingSlot.Head);
                if ((readOnlyList.Count > 0 && BlocksManager.Blocks[Terrain.ExtractContents(readOnlyList[readOnlyList.Count - 1])].GetClothingData(readOnlyList[readOnlyList.Count - 1]).DisplayName == Utils.Get("潜水头盔"))
                    || (ComponentBody.ImmersionFluidBlock != null && ComponentBody.ImmersionFluidBlock.BlockIndex == RottenMeatBlock.Index)) {
                    //if (ComponentBody.ImmersionDepth > 0.8f)
                    //ComponentScreenOverlays.BlackoutFactor = 1f;
                    ComponentHealth.Air = 1f;
                }
            }
            ComponentSteedBehavior componentSteedBehavior = null;
            ComponentMount mount = ComponentRider.Mount;
            if (mount != null) {
                componentSteedBehavior = mount.Entity.FindComponent<ComponentSteedBehavior>();
                if (componentSteedBehavior != null) {
                    if (playerInput.Move.Z > 0.5f
                        && !m_speedOrderBlocked) {
                        if (PlayerData.PlayerClass == PlayerClass.Male) {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Creatures/MaleYellFast",
                                0.75f,
                                0f,
                                ComponentBody.Position,
                                2f,
                                autoDelay: false
                            );
                        }
                        else {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Creatures/FemaleYellFast",
                                0.75f,
                                0f,
                                ComponentBody.Position,
                                2f,
                                autoDelay: false
                            );
                        }
                        componentSteedBehavior.SpeedOrder = 1;
                        m_speedOrderBlocked = true;
                    }
                    else if (playerInput.Move.Z < -0.5f
                        && !m_speedOrderBlocked) {
                        if (PlayerData.PlayerClass == PlayerClass.Male) {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Creatures/MaleYellSlow",
                                0.75f,
                                0f,
                                ComponentBody.Position,
                                2f,
                                autoDelay: false
                            );
                        }
                        else {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Creatures/FemaleYellSlow",
                                0.75f,
                                0f,
                                ComponentBody.Position,
                                2f,
                                autoDelay: false
                            );
                        }
                        componentSteedBehavior.SpeedOrder = -1;
                        m_speedOrderBlocked = true;
                    }
                    else if (MathF.Abs(playerInput.Move.Z) <= 0.25f) {
                        m_speedOrderBlocked = false;
                    }
                    componentSteedBehavior.TurnOrder = playerInput.Move.X;
                    componentSteedBehavior.JumpOrder = playerInput.Jump ? 1 : 0;
                    ComponentLocomotion.LookOrder = new Vector2(playerInput.Look.X, 0f);
                }
                else {
                    var componentBoat = mount.Entity.FindComponent<ComponentBoat>();
                    if (componentBoat != null
                        || mount.Entity.FindComponent<ComponentBoatI>() != null) {
                        if (componentBoat != null) {
                            componentBoat.TurnOrder = playerInput.Move.X;
                            componentBoat.MoveOrder = playerInput.Move.Z;
                            ComponentLocomotion.LookOrder = new Vector2(playerInput.Look.X, 0f);
                            ComponentCreatureModel.RowLeftOrder = playerInput.Move.X < -0.2f || playerInput.Move.Z > 0.2f;
                            ComponentCreatureModel.RowRightOrder = playerInput.Move.X > 0.2f || playerInput.Move.Z > 0.2f;
                        }
                        else // if (componentBoatI != null)
                        {
                            ComponentLocomotion.LookOrder = playerInput.Look;
                        }
                        ComponentCreatureModel.RowLeftOrder = playerInput.Move.X < -0.2f || playerInput.Move.Z > 0.2f;
                        ComponentCreatureModel.RowRightOrder = playerInput.Move.X > 0.2f || playerInput.Move.Z > 0.2f;
                    }
                    var c = mount.Entity.FindComponent<ComponentLocomotion>();
                    if (c != null) {
                        c.WalkOrder = playerInput.Move.XZ;
                        c.FlyOrder = new Vector3(0f, playerInput.Move.Y, 0f);
                        c.TurnOrder = playerInput.Look * new Vector2(1f, 0f);
                        c.JumpOrder = playerInput.Jump ? 1 : 0;
                        c.LookOrder = playerInput.Look;
                    }
                }
            }
            else {
                ComponentLocomotion.WalkOrder = ComponentBody.IsSneaking ? (0.66f * new Vector2(playerInput.SneakMove.X, playerInput.SneakMove.Z)) : new Vector2(playerInput.Move.X, playerInput.Move.Z);
                ComponentLocomotion.FlyOrder = new Vector3(0f, playerInput.Move.Y, 0f);
                ComponentLocomotion.TurnOrder = playerInput.Look * new Vector2(1f, 0f);
                ComponentLocomotion.JumpOrder = MathUtils.Max(playerInput.Jump ? 1 : 0, ComponentLocomotion.JumpOrder);
            }
            ComponentLocomotion.LookOrder += playerInput.Look * (SettingsManager.FlipVerticalAxis ? new Vector2(0f, -1f) : new Vector2(0f, 1f));
            ComponentLocomotion.VrLookOrder = playerInput.VrLook;
            ComponentLocomotion.VrMoveOrder = playerInput.VrMove;
            int num = Terrain.ExtractContents(ComponentMiner.ActiveBlockValue);
            Block block = BlocksManager.Blocks[num];
            bool flag = false;
            if (playerInput.Interact.HasValue
                && !flag
                && m_subsystemTime.GameTime - m_lastActionTime > 0.33000001311302185) {
                if (!ComponentMiner.Use(playerInput.Interact.Value)) {
                    TerrainRaycastResult? terrainRaycastResult = ComponentMiner.Raycast<TerrainRaycastResult>(playerInput.Interact.Value, RaycastMode.Interaction);
                    if (terrainRaycastResult.HasValue) {
                        if (!ComponentMiner.Interact(terrainRaycastResult.Value)) {
                            if (ComponentMiner.Place(terrainRaycastResult.Value)) {
                                m_subsystemTerrain.TerrainUpdater.RequestSynchronousUpdate();
                                flag = true;
                                m_isAimBlocked = true;
                            }
                        }
                        else {
                            m_subsystemTerrain.TerrainUpdater.RequestSynchronousUpdate();
                            flag = true;
                            m_isAimBlocked = true;
                        }
                    }
                }
                else {
                    m_subsystemTerrain.TerrainUpdater.RequestSynchronousUpdate();
                    flag = true;
                    m_isAimBlocked = true;
                }
            }
            float num2 = (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative || block.BlockIndex == Musket2Block.Index) ? 0.1f : 1.4f;
            if (playerInput.Aim.HasValue
                && block.IsAimable_(ComponentMiner.ActiveBlockValue)
                && m_subsystemTime.GameTime - m_lastActionTime > num2) {
                if (!m_isAimBlocked) {
                    Ray3 value = playerInput.Aim.Value;
                    Vector3 vector = GameWidget.ActiveCamera.WorldToScreen(value.Position + value.Direction, Matrix.Identity);
                    Point2 size = Window.Size;
                    if (vector.X >= size.X * 0.02f
                        && vector.X < size.X * 0.98f
                        && vector.Y >= size.Y * 0.02f
                        && vector.Y < size.Y * 0.98f) {
                        m_aim = value;
                        if (ComponentMiner.Aim(value, AimState.InProgress)) {
                            ComponentMiner.Aim(m_aim.Value, AimState.Cancelled);
                            m_aim = null;
                            m_isAimBlocked = true;
                        }
                        else if (!m_aimHintIssued
                            && Time.PeriodicEvent(1.0, 0.0)) {
                            Time.QueueTimeDelayedExecution(
                                Time.RealTime + 3.0,
                                delegate {
                                    if (!m_aimHintIssued
                                        && m_aim.HasValue
                                        && !ComponentBody.IsSneaking) {
                                        m_aimHintIssued = true;
                                        ComponentGui.DisplaySmallMessage(LanguageControl.Get(fName, 1), Color.White, blinking: true, playNotificationSound: true);
                                    }
                                }
                            );
                        }
                    }
                    else if (m_aim.HasValue) {
                        ComponentMiner.Aim(m_aim.Value, AimState.Cancelled);
                        m_aim = null;
                        m_isAimBlocked = true;
                    }
                }
            }
            else {
                m_isAimBlocked = false;
                if (m_aim.HasValue) {
                    ComponentMiner.Aim(m_aim.Value, AimState.Completed);
                    m_aim = null;
                    m_lastActionTime = m_subsystemTime.GameTime;
                }
            }
            flag |= m_aim.HasValue;
            if (playerInput.Hit.HasValue
                && !flag
                && m_subsystemTime.GameTime - m_lastActionTime > 0.33000001311302185) {
                Vector3 viewPosition3 = GameWidget.ActiveCamera.ViewPosition;
                Vector3 vector = Vector3.Normalize(GameWidget.ActiveCamera.ScreenToWorld(playerInput.Hit.Value.Position, Matrix.Identity) - viewPosition3);
                TerrainRaycastResult? nullable3 = ComponentMiner.Raycast<TerrainRaycastResult>(playerInput.Hit.Value, RaycastMode.Interaction);
                BodyRaycastResult? bodyRaycastResult = ComponentMiner.Raycast<BodyRaycastResult>(playerInput.Hit.Value, RaycastMode.Interaction);
                var componentEngine = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentEngine>();
                if (componentEngine != null)
                {
                    ComponentGui.ModalPanelWidget = new Engine2Widget(ComponentMiner.Inventory, componentEngine);
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    return;
                }
                var componentEngineA = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentEngineA>();
                if (componentEngineA != null)
                {
                    ComponentGui.ModalPanelWidget = new EngineAWidget(ComponentMiner.Inventory, componentEngineA);
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    return;
                }
                var componentEngine2 = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentTrain>();
                if (componentEngine2 != null)
                {
                    Log.Information("LX标记点：火车界面未找到");
                    //ComponentGui.ModalPanelWidget = new Train(ComponentMiner.Inventory, componentEngine2);
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    return;
                }

                if (bodyRaycastResult.HasValue) {
                    flag = true;
                    m_isDigBlocked = true;
                    if (Vector3.Distance(bodyRaycastResult.Value.HitPoint(), ComponentCreatureModel.EyePosition) <= 2f) {
                        ComponentMiner.Hit(bodyRaycastResult.Value.ComponentBody, bodyRaycastResult.Value.HitPoint(), playerInput.Hit.Value.Direction);
                    }
                }
            }
            if (playerInput.Dig.HasValue
                && !flag
                && !m_isDigBlocked
                && m_subsystemTime.GameTime - m_lastActionTime > 0.33000001311302185) {
                TerrainRaycastResult? terrainRaycastResult2 = ComponentMiner.Raycast<TerrainRaycastResult>(playerInput.Dig.Value, RaycastMode.Digging);
                if (terrainRaycastResult2.HasValue
                    && ComponentMiner.Dig(terrainRaycastResult2.Value)) {
                    m_lastActionTime = m_subsystemTime.GameTime;
                    m_subsystemTerrain.TerrainUpdater.RequestSynchronousUpdate();
                }
            }
            if (!playerInput.Dig.HasValue) {
                m_isDigBlocked = false;
            }
            if (playerInput.Drop
                && ComponentMiner.Inventory != null) {
                IInventory inventory = ComponentMiner.Inventory;
                int slotValue = inventory.GetSlotValue(inventory.ActiveSlotIndex);
                int num3 = inventory.RemoveSlotItems(count: inventory.GetSlotCount(inventory.ActiveSlotIndex), slotIndex: inventory.ActiveSlotIndex);
                if (slotValue != 0
                    && num3 != 0) {
                    Vector3 position = ComponentBody.Position + new Vector3(0f, ComponentBody.StanceBoxSize.Y * 0.66f, 0f) + (0.25f * ComponentBody.Matrix.Forward);
                    Vector3 value2 = 8f * Matrix.CreateFromQuaternion(ComponentCreatureModel.EyeRotation).Forward;
                    m_subsystemPickables.AddPickable(
                        slotValue,
                        num3,
                        position,
                        value2,
                        null
                    );
                }
            }
            if (!playerInput.PickBlockType.HasValue || flag) {
                return;
            }
            var componentCreativeInventory = ComponentMiner.Inventory as ComponentCreativeInventory;
            if (componentCreativeInventory == null) {
                return;
            }
            TerrainRaycastResult? terrainRaycastResult3 = ComponentMiner.Raycast<TerrainRaycastResult>(
                playerInput.PickBlockType.Value,
                RaycastMode.Digging,
                raycastTerrain: true,
                raycastBodies: false,
                raycastMovingBlocks: false
            );
            if (!terrainRaycastResult3.HasValue) {
                return;
            }
            int value3 = terrainRaycastResult3.Value.Value;
            value3 = Terrain.ReplaceLight(value3, 0);
            int num4 = Terrain.ExtractContents(value3);
            Block block2 = BlocksManager.Blocks[num4];
            int num5 = 0;
            IEnumerable<int> creativeValues = block2.GetCreativeValues();
            if (block2.GetCreativeValues().Contains(value3)) {
                num5 = value3;
            }
            if (num5 == 0
                && !block2.IsNonDuplicable_(value3)) {
                var list = new List<BlockDropValue>();
                block2.GetDropValues(
                    m_subsystemTerrain,
                    value3,
                    0,
                    int.MaxValue,
                    list,
                    out bool _
                );
                if (list.Count > 0
                    && list[0].Count > 0) {
                    num5 = list[0].Value;
                }
            }
            if (num5 == 0) {
                num5 = creativeValues.FirstOrDefault();
            }
            if (num5 == 0) {
                return;
            }
            int num6 = -1;
            for (int i = 0; i < 10; i++) {
                if (componentCreativeInventory.GetSlotCapacity(i, num5) > 0
                    && componentCreativeInventory.GetSlotCount(i) > 0
                    && componentCreativeInventory.GetSlotValue(i) == num5) {
                    num6 = i;
                    break;
                }
            }
            if (num6 < 0) {
                for (int j = 0; j < 10; j++) {
                    if (componentCreativeInventory.GetSlotCapacity(j, num5) > 0
                        && (componentCreativeInventory.GetSlotCount(j) == 0 || componentCreativeInventory.GetSlotValue(j) == 0)) {
                        num6 = j;
                        break;
                    }
                }
            }
            if (num6 < 0) {
                num6 = componentCreativeInventory.ActiveSlotIndex;
            }
            componentCreativeInventory.RemoveSlotItems(num6, int.MaxValue);
            componentCreativeInventory.AddSlotItems(num6, num5, 1);
            componentCreativeInventory.ActiveSlotIndex = num6;
            ComponentGui.DisplaySmallMessage(block2.GetDisplayName(m_subsystemTerrain, value3), Color.White, blinking: false, playNotificationSound: false);
            m_subsystemAudio.PlaySound(
                "Audio/UI/ButtonClick",
                1f,
                0f,
                0f,
                0f
            );
        }
    }
}