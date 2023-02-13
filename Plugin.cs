using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using BetterCarts.Patches;
using System;

namespace BetterCarts
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class BetterCartsMain : BaseUnityPlugin
    {
        internal const string ModName = "BetterCarts";
        internal const string ModVersion = "1.0.1";
        internal const string Author = "TastyChickenLegs";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        public static ConfigEntry<bool> modEnabled;
        internal static string ConnectionError = "";
        public static BetterCartsMain context;

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource BetterCartsLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        
        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            context = this;
            _serverConfigLocked = config("", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            modEnabled = config("", "Enabled", true, "Enable this mod");

            CartConfigsMain.Generate();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                BetterCartsLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                BetterCartsLogger.LogError($"There was an issue loading your {ConfigFileName}");
                BetterCartsLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;

        internal ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        internal ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }
        
        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion

        private void Update()
        {
            if (!modEnabled.Value || TastyUtils.IgnoreKeyPresses(true))
                return;

            if (Input.GetKeyDown(CartConfigsMain.cartHotKey.Value.MainKey))
            {

                float attachDistanceFloat = Convert.ToSingle(CartConfigsMain.attachDistance.Value);
                float closest = attachDistanceFloat;
                Vagon closestVagon = null;
                Vector3 position = Player.m_localPlayer.transform.position + Vector3.up;
                foreach (Collider collider in Physics.OverlapSphere(position, attachDistanceFloat))
                {
                    Vagon v = collider.gameObject.GetComponent<Vagon>();
                    if (!v)
                        v = collider.transform.parent?.gameObject.GetComponent<Vagon>();
                    if (collider.attachedRigidbody && v && Vector3.Distance(collider.ClosestPoint(position), position) < closest && (v.IsAttached(Player.m_localPlayer) || !v.InUse()))
                    {
                        //Dbgl("Got nearby cart");
                        closest = Vector3.Distance(collider.ClosestPoint(position), position);
                        closestVagon = collider.transform.parent.gameObject.GetComponent<Vagon>();
                    }
                }
                if (closestVagon != null)
                {
                    closestVagon.Interact(Player.m_localPlayer, false, false);
                }
            }
        }


        [HarmonyPatch(typeof(Terminal), "InputText")]
        private static class InputText_Patch
        {
            private static bool Prefix(Terminal __instance)
            {
                if (!BetterCartsMain.modEnabled.Value)
                    return true;
                string text = __instance.m_input.text;
                if (text.ToLower().Equals($"{typeof(BepInPlugin).Namespace.ToLower()} reset"))
                {
                    BetterCartsMain.context.Config.Reload();
                    BetterCartsMain.context.Config.Save();

                    __instance.AddString(text);
                    __instance.AddString($"{BetterCartsMain.context.Info.Metadata.Name} config reloaded");
                    return false;
                }
                return true;
            }
        }
    }
    
}