﻿using SMLHelper.V2.Json;
using SMLHelper.V2.Options.Attributes;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Straitjacket.Subnautica.Mods.ForceResolution
{
    [Menu("Force Resolution", LoadOn = MenuAttribute.LoadEvents.MenuRegistered | MenuAttribute.LoadEvents.MenuOpened)]
    internal class Config : ConfigFile
    {
        public Config() : base()
        {
            OnFinishedLoading += OnLoaded;
        }

        public void OnLoaded(object sender, ConfigFileEventArgs e) => OnServiceModeChanged();

        public Resolution DesiredResolution { get; set; }

        [Choice(Label = "Desired fullscreen mode")]
        public FullscreenMode DesiredFullscreenMode { get; set; } = FullscreenMode.WindowedFullscreen;

        [Choice(Label = "Resolution service mode",
                Tooltip = "Runs a background service that checks for changes to the game resolution," +
                          "and automatically enforces your preferences.")]
        [OnChange(nameof(OnServiceModeChanged))]
        public ServiceMode ResolutionServiceMode { get; set; } = ServiceMode.Startup;
        public void OnServiceModeChanged()
        {
            switch (ResolutionServiceMode)
            {
                case ServiceMode.Enabled:
                    ResolutionService.Instance.gameObject.EnsureComponent<SceneCleanerPreserve>();
                    Object.DontDestroyOnLoad(ResolutionService.Instance.gameObject);
                    break;
                case ServiceMode.Startup when Object.FindObjectOfType<Player>() is null:
                    foreach (var preserver in ResolutionService.Instance.GetComponents<SceneCleanerPreserve>().ToList())
                    {
                        Object.Destroy(preserver);
                    }
                    SceneManager.MoveGameObjectToScene(ResolutionService.Instance.gameObject, SceneManager.GetActiveScene());
                    break;
                case ServiceMode.Startup:
                case ServiceMode.Disabled:
                    Object.Destroy(ResolutionService.Instance.gameObject);
                    break;
            }
        }

        [Button(Label = "Save current settings",
                Tooltip = "Saves the current resolution and fullscreen mode, to be automatically enforced " +
                          "by the resolution service or applied manually.")]
        public void SaveSettings()
        {
            Main.Logger.LogDebug($"Saving resolution {Screen.currentResolution}, {DesiredFullscreenMode}");
            DesiredResolution = Screen.currentResolution;
            Save();
        }

        [Button(Label = "Apply saved settings",
                Tooltip = "Sets your resolution and fullscreen mode to the saved settings.")]
        public void ApplySavedSettings() => ResolutionService.Instance.SetResolution(DesiredResolution, DesiredFullscreenMode);

        public enum FullscreenMode
        {
            ExclusiveFullscreen = 0,
            WindowedFullscreen = 1,
            Windowed = 3
        }

        public enum ServiceMode
        {
            Disabled, Enabled, Startup
        }
    }
}