using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;

using HideMUI.Utils;

namespace HideMUI
{
    [BepInPlugin("moltenhead.HideMUI", "Hide MUI", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static readonly ManualLogSource HMUILogger = BepInEx.Logging.Logger.CreateLogSource("HideMUI");

        private readonly Harmony _harmony = new Harmony("moltenhead.HideMUI");

        private static readonly string configFileName = "moltenhead.HideMUI.cfg";
        private static readonly string configFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + configFileName;

        public static ConfigEntry<bool> m_crosshairActive;
        public static ConfigEntry<bool> m_stealthGUIActive;

        void Awake()
        {
            InitializeConfig();

            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            ConfigWatcher();
        }

        private void InitializeConfig()
        {
            base.Config.SaveOnConfigSet = true;
            m_crosshairActive = base.Config.Bind("General", "ActivateCrosshair", false, new ConfigDescription("Activates the main crosshair - default is deactivated (false)."));
            m_stealthGUIActive = base.Config.Bind("General", "ActivateStealthGUI", false, new ConfigDescription("Activates the stealth indicator and bar - default is deactivated (false)."));
        }

        private void ConfigWatcher()
        {
            FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(Paths.ConfigPath, configFileName);
            fileSystemWatcher.Changed += OnConfigChanged;
            fileSystemWatcher.Created += OnConfigChanged;
            fileSystemWatcher.Renamed += OnConfigChanged;
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(configFileFullPath))
            {
                return;
            }
            try
            {
                HMUILogger.LogDebug("OnConfigChanged called..");
                base.Config.Reload();
            }
            catch
            {
                HMUILogger.LogError("There was an issue loading your " + configFileName);
                HMUILogger.LogError("Please check your config entries for spelling and format!");
            }
        }
    }

    [HarmonyPatch]
    public static class HudPatch
    {
        [HarmonyPatch(typeof(Hud), "Awake")]
        public static class HudAwake_Patch
        {
            public static void Postfix(Hud __instance)
            {
                UI.UpdateHud(__instance);
            }
        }

        [HarmonyPatch(typeof(Hud), "UpdateCrosshair")]
        public static class HudUpdateCrosshair_Patch
        {
            public static void Postfix(Hud __instance, Player player)
            {
                if (player != Player.m_localPlayer) { return; }

                UI.setActiveCrosshair(__instance, Plugin.m_crosshairActive.Value);
            }
        }

        [HarmonyPatch(typeof(Hud), "UpdateStealth")]
        public static class HudUpdateStealth_Patch
        {
            public static bool Prefix(Hud __instance, Player player)
            {
                if (player != Player.m_localPlayer || Plugin.m_stealthGUIActive.Value) {
                    return true;
                }

                UI.setActiveStealthHud(__instance, false);
                return false;
            }
        }
    }
}
