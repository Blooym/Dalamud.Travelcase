using System;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Travelcase.Base;
using Travelcase.Utils;

namespace Travelcase.Managers
{
    /// <summary>
    ///     Manages automatic gearset switching based on conditions with protections in place.
    /// </summary>
    public sealed class GearsetManager : IDisposable
    {
        /// <summary>
        ///     Initializes the GearsetManager and its resources.
        /// </summary>
        public GearsetManager() => DalamudInjections.ClientState.TerritoryChanged += this.OnTerritoryChanged;

        /// <summary>
        ///      Disposes of the GearsetManager and associated resources.
        /// </summary>
        public void Dispose() => DalamudInjections.ClientState.TerritoryChanged -= this.OnTerritoryChanged;

        /// <summary>
        ///     The player's stored territory, used to prevent gearset switching more than 1 time per territory.
        /// </summary>
        private ushort storedTerritory;

        /// <summary>
        ///     Handles territory changes and gearset switching.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="territory">The new territory ID.</param>
        public void OnTerritoryChanged(object? _, ushort territory)
        {
            var config = Services.CharacterConfig.CurrentConfig;

            if (this.storedTerritory != territory && config?.IsEnabled == true)
            {
                if (config.OnlyInRoleplayMode && DalamudInjections.ClientState.LocalPlayer?.OnlineStatus.Id != 22)
                {
                    PluginLog.Log($"{DalamudInjections.ClientState.LocalPlayer?.OnlineStatus.Id}");
                    PluginLog.Debug("GearsetManager(OnTerritoryChanged): Player is not in roleplay mode, skipping gearset change.");
                    return;
                }

                PluginLog.Debug($"GearsetManager(OnTerritoryChange): Territory changed to {territory}, and config is enabled, attempting to apply gearset.");
                this.storedTerritory = territory;

                var gearset = config.GearsetBindings.FirstOrDefault(g => g.Key == territory).Value;
                if (gearset?.Enabled != true)
                {
                    return;
                }

                if (!DataUtil.AllowedZones?.Any(x => x.RowId == territory) ?? true)
                {
                    PluginLog.Warning($"GearsetManager(OnTerritoryChange): Territory {territory} is not an allowed territoryID, skipping gearset change.");
                    return;
                }

                if (gearset.GlamourPlate is > 20 or < 0)
                {
                    PluginLog.Warning($"GearsetManager(OnTerritoryChange): Glamour plate {gearset.GlamourPlate} is not a valid value, skipping gearset change.");
                    return;
                }

                if (gearset.GearsetNumber is > 100 or < 0)
                {
                    PluginLog.Warning($"GearsetManager(OnTerritoryChange): Gearset number {gearset.GearsetNumber} is not a valid value, skipping gearset change.");
                    return;
                }

                new Task(() =>
                {
                    try
                    {
                        if (config.GearsetBindings.TryGetValue(territory, out var gearset))
                        {
                            while (DalamudInjections.Condition[ConditionFlag.BetweenAreas]
                                || DalamudInjections.Condition[ConditionFlag.BetweenAreas51]
                                || DalamudInjections.Condition[ConditionFlag.Occupied]
                                || DalamudInjections.Condition[ConditionFlag.OccupiedInCutSceneEvent]
                                || DalamudInjections.Condition[ConditionFlag.Unconscious])
                            {
                                PluginLog.Debug("GearsetManager(OnTerritoryChange): Unable to change gearset yet, waiting for conditions to clear.");
                                Task.Delay(1000).Wait();
                            }

                            this.ChangeGearset(gearset.GearsetNumber, gearset.GlamourPlate);
                        }
                    }
                    catch (Exception e) { PluginLog.Error(e.ToString()); }
                }).Start();
            }
        }

        /// <summary>
        ///     Changes the player's gearset.
        /// </summary>
        /// <param name="gearsetId"></param>
        /// <param name="glamourId"></param>
        private unsafe bool ChangeGearset(int gearsetId, byte glamourId)
        {
            var gsModuleInstance = RaptureGearsetModule.Instance();
            PluginLog.Information($"GearsetManager(OnTerritoryChange): Moved zones, changing gearset to {gearsetId}{(glamourId == 0 ? string.Empty : $" and glamour plate to {glamourId}")}.");

            if (gsModuleInstance->IsValidGearset(gearsetId) == 0)
            {
                PluginLog.Warning($"GearsetManager(OnTerritoryChange): Unknown or invalid gearset value: {gearsetId}, skipping gearset change.");
                return false;
            }

            if (gsModuleInstance->EquipGearset(gearsetId, glamourId) == -1)
            {
                PluginLog.Error($"GearsetManager(OnTerritoryChange): Failed to change gearset to {gearsetId}, recieved -1 from RaptureGearsetModule::EquipGearset.");
            }

            return true;
        }
    }
}
