﻿using Sphere.Plugins;
using Sphere.Plugins.Interfaces;
using Sphere.Plugins.Views;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace SphereStudio.Plugins
{
    public class PluginMain : IPluginMain
    {
        public string Name { get; } = "Sound Test";
        public string Author { get; } = "Spherical";
        public string Description { get; } = "Listen to sounds from your game while you work!";
        public string Version { get; } = "1.3 a0";

        private SoundPicker _soundPicker;

        public void Initialize(ISettings conf)
        {
            _soundPicker = new SoundPicker(this);
            _soundPicker.WatchProject(PluginManager.Core.Project);
            _soundPicker.Refresh();

            PluginManager.Register(this, _soundPicker, "Sound Test");

            PluginManager.Core.LoadProject += IDE_LoadProject;
            PluginManager.Core.UnloadProject += IDE_UnloadProject;
            PluginManager.Core.TestGame += IDE_TestGame;
        }

        public void ShutDown()
        {
            PluginManager.UnregisterAll(this);
            _soundPicker.WatchProject(null);
            _soundPicker.StopMusic();
            PluginManager.Core.TestGame -= IDE_TestGame;
            PluginManager.Core.LoadProject -= IDE_LoadProject;
            PluginManager.Core.UnloadProject -= IDE_UnloadProject;
        }

        private void IDE_LoadProject(object sender, EventArgs e) =>
            _soundPicker.WatchProject(PluginManager.Core.Project);

        private void IDE_UnloadProject(object sender, EventArgs e) =>
            _soundPicker.WatchProject(null);

        private void IDE_TestGame(object sender, EventArgs e) =>
            _soundPicker.ForcePause();
    }
}
