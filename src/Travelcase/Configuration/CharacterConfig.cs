using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Logging;
using Newtonsoft.Json;
using Travelcase.Base;

namespace Travelcase.Configuration
{
    /// <summary>
    ///     Provides access to and determines the per-character Plugin configuration.
    /// </summary>
    internal sealed class CharacterConfiguration
    {
        /// <summary>
        ///     The current configuration version, incremented on breaking changes.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     If true, the plugin will be active for this character.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        ///     If true, the plugin will only be active when the roleplaying online status is active.
        /// </summary>
        public bool OnlyInRoleplayMode { get; set; }

        /// <summary>
        ///     A dictionary of gearset bindings, where the key is the territoryID and the value is the gearset configuration.
        /// </summary>
        public Dictionary<uint, Gearset> GearsetBindings { get; set; } = new();

        /// <summary>
        ///     Information about a gearset configuration, such as the number and linked glamour plate.
        /// </summary>
        public sealed class Gearset
        {
            public int GearsetNumber { get; set; }
            public byte GlamourPlate { get; set; }
            public bool Enabled { get; set; }
        }

        /// <summary>
        ///     Returns the path to the configuration directory, alias for <see cref="PluginInterface.GetPluginConfigDirectory" />.
        /// </summary>
        private static string GetConfigDir() => DalamudInjections.PluginInterface.GetPluginConfigDirectory();

        /// <summary>
        ///     Returns the path to the configuration file for the ContentID provided.
        /// </summary>
        private static string UserConfigPath(ulong contentId) => Path.Combine(GetConfigDir(), $"{contentId}.json");

        /// <summary>
        ///     Saves the current configuration (and any modifications) as a file for this character.
        /// </summary>
        public void Save()
        {
            try
            {
                var contentId = DalamudInjections.ClientState.LocalContentId;
                if (contentId == 0)
                {
                    PluginLog.Warning("CharacterConfiguration(Save): No ContentID found, not able to save user configuration.");
                    return;
                }

                Directory.CreateDirectory(GetConfigDir());
                var configObj = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(UserConfigPath(contentId), configObj);
            }
            catch (Exception e)
            {
                PluginLog.Error($"CharacterConfiguration(Save): Failed to save character configuration to file: {e}");
            }
        }

        /// <summary>
        ///     Saves the current configuration (and any modifications) as a file for the ContentID provided.
        /// </summary>
        /// <param name="contentId">The ContentID to save the configuration for.</param>
        public void Save(ulong contentId)
        {
            try
            {
                Directory.CreateDirectory(GetConfigDir());
                var configObj = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(UserConfigPath(contentId), configObj);
            }
            catch (Exception e)
            {
                PluginLog.Error($"CharacterConfiguration(Save): Failed to save character configuration to file: {e}");
            }
        }

        /// <summary>
        ///     Returns the configuration for the current character.
        /// </summary>
        public static CharacterConfiguration? Load()
        {
            var contentID = DalamudInjections.ClientState.LocalContentId;
            if (contentID == 0)
            {
                PluginLog.Warning("CharacterConfiguration(Load): No ContentID found, not able to load configuration.");
                return null;
            }

            var configPath = UserConfigPath(contentID);
            if (!File.Exists(configPath))
            {
                return new();
            }

            try
            {
                var configObj = JsonConvert.DeserializeObject<CharacterConfiguration>(File.ReadAllText(configPath));
                return configObj ?? (CharacterConfiguration?)new();
            }
            catch (Exception e)
            {
                PluginLog.Error($"CharacterConfiguration(Load): Failed to load character configuration from {configPath}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Loads the configuration for the ContentID provided.
        /// </summary>
        /// <param name="contentId"></param>
        public static CharacterConfiguration? Load(ulong contentId)
        {
            var configPath = UserConfigPath(contentId);
            if (!File.Exists(configPath))
            {
                return new();
            }

            try
            {
                return JsonConvert.DeserializeObject<CharacterConfiguration>(File.ReadAllText(configPath));
            }
            catch (Exception e)
            {
                PluginLog.Error($"CharacterConfiguration(Load): Failed to load character configuration from {configPath}: {e.Message}");
                return null;
            }
        }
    }
}
