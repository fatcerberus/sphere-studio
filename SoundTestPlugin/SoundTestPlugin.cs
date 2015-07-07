﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Sphere.Plugins;

namespace SphereStudio.Plugins
{
    public class SoundTestPlugin : IPlugin
    {
        public string Name { get { return "Sound Test"; } }
        public string Author { get { return "Lord English"; } }
        public string Description { get { return "Listen to sounds from your game while you work! :o)"; } }
        public string Version { get { return "1.2.0"; } }
        public Icon Icon { get; set; }

        private const string _openFileFilters = "*.mp3;*.ogg;*.flac;*.mod;*.it;*.s3d;*.wav";
        private readonly List<string> _extensionList = new List<string>(new[] {
            ".mp3", ".ogg", ".flac",  // compressed audio formats
            ".mod", ".it", ".s3d",    // tracker formats
            ".wav"                    // uncompressed/PCM formats
        });
        
        private SoundPicker _soundPicker;

        public SoundTestPlugin()
        {
            Icon = Icon.FromHandle(Properties.Resources.Icon.GetHicon());
        }

        public void Initialize(ISettings conf)
        {
            _soundPicker = new SoundPicker() { Dock = DockStyle.Fill };
            _soundPicker.Refresh();

            DockDescription description = new DockDescription();
            description.TabText = @"Sound Test";
            description.Icon = Icon;
            description.Control = _soundPicker;
            description.DockAreas = DockDescAreas.Document | DockDescAreas.Sides;
            description.HideOnClose = true;
            description.DockState = DockDescStyle.Side;

            PluginManager.Core.DockControl(description);
            PluginManager.Core.RegisterOpenFileType("Audio", _openFileFilters);
            PluginManager.Core.LoadProject += IDE_LoadProject;
            PluginManager.Core.UnloadProject += IDE_UnloadProject;
            PluginManager.Core.TestGame += IDE_TestGame;
            PluginManager.Core.TryEditFile += IDE_TryEditFile;
            _soundPicker.WatchProject(PluginManager.Core.CurrentGame);
        }

        public void Destroy()
        {
            PluginManager.Core.UnregisterOpenFileType(_openFileFilters);
            _soundPicker.WatchProject(null);
            _soundPicker.StopMusic();
            PluginManager.Core.RemoveControl("Sound Test");
            PluginManager.Core.TryEditFile -= IDE_TryEditFile;
            PluginManager.Core.TestGame -= IDE_TestGame;
            PluginManager.Core.LoadProject -= IDE_LoadProject;
            PluginManager.Core.UnloadProject -= IDE_UnloadProject;
        }

        private void IDE_LoadProject(object sender, EventArgs e)
        {
            _soundPicker.WatchProject(PluginManager.Core.CurrentGame);
        }

        private void IDE_UnloadProject(object sender, EventArgs e)
        {
            _soundPicker.WatchProject(null);
        }

        private void IDE_TryEditFile(object sender, EditFileEventArgs e)
        {
            if (e.Handled) return;
            if (_extensionList.Contains(e.Extension.ToLowerInvariant()))
            {
                _soundPicker.PlayFile(e.Path);
                e.Handled = true;
            }
        }

        private void IDE_TestGame(object sender, EventArgs e)
        {
            _soundPicker.ForcePause();
        }
    }
}
