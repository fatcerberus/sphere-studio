﻿using System;
using System.Drawing;
using System.Windows.Forms;

using Sphere.Plugins;
using Sphere.Plugins.Interfaces;
using Sphere.Plugins.Views;

namespace SphereStudio.Plugins
{
    public class PluginMain : IPluginMain, INewFileOpener
    {
        public string Name { get { return "Spriteset Editor"; } }
        public string Author { get { return "Spherical"; } }
        public string Description { get { return "Sphere Studio default spriteset editor"; } }
        public string Version { get { return "1.2.0"; } }

        #region wire up Spriteset menu
        private static ToolStripMenuItem _spritesetMenu;
        private static ToolStripMenuItem _exportMenuItem;
        private static ToolStripMenuItem _importMenuItem;
        private static ToolStripMenuItem _rescaleMenuItem;
        private static ToolStripMenuItem _resizeMenuItem;

        static PluginMain()
        {
            _spritesetMenu = new ToolStripMenuItem("&Spriteset") { Visible = false };
            _resizeMenuItem = new ToolStripMenuItem("&Resize...", Properties.Resources.arrow_inout, menuResize_Click);
            _rescaleMenuItem = new ToolStripMenuItem("Re&scale...", Properties.Resources.arrow_inout, menuRescale_Click);
            _importMenuItem = new ToolStripMenuItem("&Import...", null, menuImport_Click);
            _exportMenuItem = new ToolStripMenuItem("E&xport...", null, menuExport_Click);
            _spritesetMenu.DropDownItems.AddRange(new ToolStripItem[] {
                _resizeMenuItem,
                _rescaleMenuItem,
                new ToolStripSeparator(),
                _importMenuItem,
                _exportMenuItem
            });
        }
        
        private static void menuExport_Click(object sender, EventArgs e)
        {
            // TODO: implement spriteset export!
            throw new NotImplementedException();
        }

        private static void menuImport_Click(object sender, EventArgs e)
        {
            // TODO: implement spriteset import!
            throw new NotImplementedException();
        }

        private static void menuRescale_Click(object sender, EventArgs e)
        {
            (PluginManager.IDE.CurrentDocument as SpritesetEditView).RescaleAll();
        }

        private static void menuResize_Click(object sender, EventArgs e)
        {
            (PluginManager.IDE.CurrentDocument as SpritesetEditView).ResizeAll();
        }
        #endregion

        internal static void ShowMenus(bool show)
        {
            _spritesetMenu.Visible = show;
        }
        
        public PluginMain()
        {
            FileTypeName = "Sphere Spriteset";
            FileExtensions = new[] { "rss" };
            FileIcon = Properties.Resources.PersonIcon;
        }

        public void Initialize(ISettings conf)
        {
            PluginManager.Register(this, this, Name);
            PluginManager.IDE.AddMenuItem(_spritesetMenu, "View");
        }

        public void ShutDown()
        {
            PluginManager.UnregisterAll(this);
        }

        public string FileTypeName { get; private set; }
        public string[] FileExtensions { get; private set; }
        public Bitmap FileIcon { get; private set; }

        public DocumentView New()
        {
            SpritesetEditView view = new SpritesetEditView();
            return view.NewDocument() ? view : null;
        }

        public DocumentView Open(string fileName)
        {
            SpritesetEditView view = new SpritesetEditView();
            view.Load(fileName);
            return view;
        }
    }
}