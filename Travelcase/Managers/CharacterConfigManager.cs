using System;
using Travelcase.Base;

namespace Travelcase.Managers
{
    /// <summary>
    ///     CharacterConfigManager is responsible for managing the per-character configuration.
    /// </summary>
    internal sealed class CharacterConfigManager : IDisposable
    {
        /// <summary>
        ///     The current configuration loaded for the character.
        /// </summary>
        internal CharacterConfiguration? CurrentConfig { get; private set; }

        /// <summary>
        ///     Initializes the CharacterConfigManager and loads the configuration for the current character.
        /// </summary>
        internal CharacterConfigManager() => PluginService.Framework.Update += this.OnFrameworkUpdate;

        /// <summary>
        ///     Disposes of the CharacterConfigManager and associated resources.
        /// </summary>
        public void Dispose()
        {
            PluginService.Framework.Update -= this.OnFrameworkUpdate;
            this.CurrentConfig = null;
        }

        /// <summary>
        ///     Listens to the framework for logins and logout events for loading and unloading the configuration.
        /// </summary>
        /// <param name="e">The framework update event arguments.</param>
        private void OnFrameworkUpdate(object? e)
        {
            var contentID = PluginService.ClientState?.LocalContentId;

            if (contentID is null or 0)
            {
                if (this.CurrentConfig != null)
                {
                    PluginService.PluginLog.Information("CharacterConfigManager(OnFrameworkUpdate): Player logged out, unloading their config.");
                    this.CurrentConfig = null;
                }
            }
            else if (this.CurrentConfig == null)
            {
                PluginService.PluginLog.Information("CharacterConfigManager(OnFrameworkUpdate): Player logged in, loading their config.");
                this.CurrentConfig = LoadConfig();
            }
        }

        /// <summary>
        ///     Loads the configuration for the current character.
        /// </summary>
        private static CharacterConfiguration? LoadConfig()
        {
            PluginService.PluginLog.Debug("CharacterConfigManager(LoadConfig): Loading configuration");
            return CharacterConfiguration.Load(PluginService.ClientState.LocalContentId);
        }
    }
}
