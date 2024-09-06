using System;
using System.Diagnostics;
using System.IO;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;

namespace DiscordRpcWine
{
    internal sealed class DiscordRpcWinePlugin : IDalamudPlugin
    {
        private readonly IDalamudPluginInterface pluginInterface;
        private readonly IPluginLog pluginLog;
        private readonly Process bridgeProcess;

        private DirectoryInfo RPC_BRIDGE_PATH => new(Path.Combine(this.pluginInterface.AssemblyLocation.Directory!.FullName, "binaries", "WineRPCBridge.exe"));

        public DiscordRpcWinePlugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog)
        {
            this.pluginInterface = pluginInterface;
            this.pluginLog = pluginLog;

            if (!Util.IsWine())
            {
                throw new InvalidOperationException("This plugin can only be loaded while the game is running under WINE.");
            }

            // Check if bridge is already running.
            var wineBridge = Process.GetProcessesByName(this.RPC_BRIDGE_PATH.Name);
            if (wineBridge.Length > 0)
            {
                this.pluginLog.Information($"Found existing RPC bridge process, PID: {wineBridge[0].Id}, not starting a new one.");
                this.bridgeProcess = wineBridge[0];
                return;
            }

            // Start the bridge.
            this.pluginLog.Information($"Starting RPC bridge process: {this.RPC_BRIDGE_PATH}");
            this.bridgeProcess = Process.Start(new ProcessStartInfo
            {
                FileName = this.RPC_BRIDGE_PATH.FullName,
                UseShellExecute = false,
                CreateNoWindow = true,
            })!;
            this.pluginLog.Information($"Started RPC bridge process, PID: {this.bridgeProcess.Id}");
        }

        public void Dispose()
        {
            this.bridgeProcess.Kill();
            this.pluginLog.Information($"Killed RPC bridge process: {this.bridgeProcess.Id}");
        }
    }
}
