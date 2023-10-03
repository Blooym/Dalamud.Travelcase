using Dalamud.Plugin;
using Travelcase.Base;

namespace Travelcase
{
    internal sealed class TravelcasePlugin : IDalamudPlugin
    {
        /// <summary>
        ///     The plugin name, fetched from PluginConstants.
        /// </summary>
        public string Name { get; } = PluginConstants.PluginName;

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        /// <param name="pluginInterface"></param>
        public TravelcasePlugin(DalamudPluginInterface pluginInterface)
        {
            pluginInterface.Create<PluginService>();
            PluginService.Initialize();
        }

        /// <summary>
        ///     Disposes of the plugin's resources.
        /// </summary>
        public void Dispose() => PluginService.Dispose();
    }
}
