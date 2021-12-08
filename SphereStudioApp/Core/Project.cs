﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SphereStudio.Base;
using SphereStudio.IO;

namespace SphereStudio.Core
{
    /// <summary>
    /// Represents a Sphere Studio project.
    /// </summary>
    class Project : IProject
    {
        private Dictionary<string, HashSet<int>> breakpoints = new Dictionary<string, HashSet<int>>();
        private IniSettings settings;

        /// <summary>
        /// Creates a new, empty Sphere Studio project.
        /// </summary>
        /// <param name="rootPath">Path of the directory where the project will reside. Must be empty.</param>
        /// <param name="name">The name of the project to create.</param>
        /// <returns>A Project object representing the new project.</returns>
        public static Project Create(string rootPath, string name)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            if (dirInfo.Exists && dirInfo.GetFileSystemInfos().Length > 0)
                throw new ArgumentException("Root directory for a new project must be empty.");
            dirInfo.Create();
            var project = new Project(Path.Combine(dirInfo.FullName, makeFileName(name)))
            {
                Name = name
            };
            return project;
        }

        /// <summary>
        /// Loads an existing project.
        /// </summary>
        /// <param name="rootPath">The full path of the directory containing the project.</param>
        /// <returns>A Project object used to manage the loaded project.</returns>
        public static Project Open(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            return Path.GetFileName(fileName).ToUpperInvariant() == "GAME.SGM"
                ? Project.FromSgm(fileName)
                : new Project(fileName);
        }

        /// <summary>
        /// Opens a Sphere game manifest (SGM) file as a Sphere Studio project.
        /// </summary>
        /// <param name="fileName">The fully qualified filename of the SGM file to import.</param>
        /// <returns>A Project object representing the synthesized project.</returns>
        public static Project FromSgm(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            Project project = new Project(fileName)
            {
                GameOnly = true,
                Name = "Untitled",
                Author = "Author Unknown",
                Summary = "",
            };

            var sgmText = File.ReadAllLines(fileName);
            var apiVersion = 0;
            var apiLevel = 1;
            var resolution = new Size(320, 240);
            var screenWidth = 320;
            var screenHeight = 240;
            var scriptPath = string.Empty;
            var saveId = string.Empty;
            foreach (string line in sgmText)
            {
                try
                {
                    Match match = new Regex("(.+)=(.*)").Match(line);
                    if (match.Success)
                    {
                        string key = match.Groups[1].Value;
                        string value = match.Groups[2].Value;
                        switch (key)
                        {
                            case "version":
                                apiVersion = int.Parse(value);
                                break;
                            case "api":
                                if (apiVersion == 0)
                                    apiVersion = 2;
                                apiLevel = int.Parse(value);
                                break;
                            case "name": project.Name = value; break;
                            case "author": project.Author = value; break;
                            case "description": project.Summary = value; break;
                            case "script": scriptPath = value; break;
                            case "saveID": saveId = value; break;
                            case "main":
                                if (apiVersion == 0)
                                    apiVersion = 2;
                                scriptPath = value;
                                break;
                            case "resolution":
                                Match resoMatch = new Regex(@"(\d+)x(\d+)").Match(value);
                                if (resoMatch.Success)
                                    resolution = new Size(int.Parse(resoMatch.Groups[1].Value), int.Parse(resoMatch.Groups[2].Value));
                                break;
                            case "screen_width":
                                screenWidth = int.Parse(value);
                                break;
                            case "screen_height":
                                screenHeight = int.Parse(value);
                                break;
                        }
                    }
                }
                catch
                {
                    // ignore any parsing errors. if an error occurs parsing the manifest,
                    // we'll just use the default values. this ensures it is always possible
                    // to upgrade a Sphere 1.x project even if the game.sgm is damaged.
                }
            }

            apiVersion = Math.Max(apiVersion, 1);
            if (apiVersion < 2 && scriptPath != string.Empty)
                scriptPath = $"scripts/{scriptPath}";
            project.Compiler = Defaults.Compiler;
            project.Settings.SetInteger("apiVersion", apiVersion);
            project.Settings.SetInteger("apiLevel", apiLevel);
            project.Settings.SetSize("resolution", apiVersion >= 2
                ? resolution
                : new Size(screenWidth, screenHeight));
            project.Settings.SetString("mainScript", scriptPath);
            project.Settings.SetString("saveID", saveId);

            var jsonPath = Path.Combine(project.RootPath, "game.json");
            if (File.Exists(jsonPath))
                project.settings.SetValue("manageGameJson", true);
            return project;
        }

        private Project(string fileName)
        {
            fileName = Path.GetFullPath(fileName);
            var userFilePath = Path.Combine(Path.GetDirectoryName(fileName), "sphereStudio.usr");
            settings = new IniSettings(new IniFile(fileName, false), ".ssproj");
            FileName = fileName;
            User = new UserSettings(userFilePath);
        }

        public UserSettings User { get; private set; }

        /// <summary>
        /// Gets the fully qualified filename of the .ssproj file.
        /// </summary>
        public string FileName { get; private set; }
        
        /// <summary>
        /// Gets the full path of the project's root directory.
        /// </summary>
        public string RootPath
        {
            get { return Path.GetDirectoryName(FileName); }
        }

        /// <summary>
        /// Gets the <c>ISettings</c> object used to store settings for this project.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Gets or sets the registered name of the compiler to use when building
        /// this project.
        /// </summary>
        public string Compiler
        {
            get => !GameOnly ? settings.GetString("compiler", Defaults.Compiler) : Defaults.Compiler;
            set => settings.SetValue("compiler", value);
        }

        /// <summary>
        /// Gets or sets the project name (usually a title).
        /// </summary>
        public string Name
        {
            get { return settings.GetString("name", "Untitled"); }
            set { settings.SetValue("name", value); }
        }

        /// <summary>
        /// Gets or sets the name of the project author.
        /// </summary>
        public string Author
        {
            get { return settings.GetString("author", ""); }
            set { settings.SetValue("author", value); }
        }

        /// <summary>
        /// Gets or sets a short description of the game.
        /// </summary>
        public string Summary
        {
            get { return settings.GetString("description", ""); }
            set { settings.SetValue("description", value); }
        }

        /// <summary>
        /// Gets or sets the game's vertical resolution.
        /// </summary>
        public int ScreenWidth
        {
            get { return settings.GetInteger("screenWidth", 320); }
            set { settings.SetValue("screenWidth", value); }
        }

        /// <summary>
        /// Gets or sets the game's horizontal resolution.
        /// </summary>
        public int ScreenHeight
        {
            get { return settings.GetInteger("screenHeight", 240); }
            set { settings.SetValue("screenHeight", value); }
        }

        /// <summary>
        /// Gets whether the project is game-only (e.g. synthesized from an SGM file).
        /// </summary>
        public bool GameOnly
        {
            get { return settings.GetBoolean("backCompatible", false); }
            set { settings.SetValue("backCompatible", value); }
        }

        public IReadOnlyDictionary<string, int[]> GetAllBreakpoints()
        {
            Dictionary<string, int[]> retval = new Dictionary<string, int[]>();
            foreach (string k in breakpoints.Keys)
            {
                retval.Add(k, breakpoints[k].ToArray());
            }
            return retval;
        }

        public int[] GetBreakpoints(string scriptPath)
        {
            if (scriptPath == null)
                return new int[0];
            int hash = scriptPath.GetHashCode();
            if (breakpoints.ContainsKey(scriptPath))
            {
                return breakpoints[scriptPath].ToArray();
            }
            else
            {
                int[] lines = new int[0];
                try
                {
                    lines = Array.ConvertAll(
                        User.GetString($"breakpointsSet:{hash:X8}", "").Split(','),
                        int.Parse);
                }
                catch (Exception)
                {
                    // *munch*
                }
                breakpoints.Add(scriptPath, new HashSet<int>(lines));
                return lines;
            }
        }

        /// <summary>
        /// Saves any changes made to the project.
        /// </summary>
        public void Save()
        {
            var userFileName = Path.Combine(RootPath, "sphereStudio.usr");
            User.SaveAs(userFileName);
            if (GameOnly)
            {
                // Sphere 1.x-compatible project mode (treat .sgm as project file)
                string fileName = Path.Combine(Path.GetDirectoryName(FileName), "game.sgm");
                using (var writer = new StreamWriter(fileName, false, new UTF8Encoding(false)))
                {
                    var apiVersion = settings.GetInteger("apiVersion", 1);
                    var apiLevel = settings.GetInteger("apiLevel", 1);
                    var resolution = settings.GetSize("resolution", new Size(320, 240));
                    var mainPath = settings.GetString("mainScript", "scripts/main.js");
                    var saveId = settings.GetString("saveID", string.Empty);
                    writer.WriteLine($"version={apiVersion}");
                    if (apiVersion >= 2)
                        writer.WriteLine($"api={apiLevel}");
                    writer.WriteLine($"name={Name}");
                    writer.WriteLine($"author={Author}");
                    writer.WriteLine($"description={Summary}");
                    writer.WriteLine($"saveID={saveId}");
                    if (apiVersion >= 2)
                    {
                        writer.WriteLine($"resolution={resolution.Width}x{resolution.Height}");
                        writer.WriteLine($"main={mainPath}");
                    }
                    else
                    {
                        var scriptPath = mainPath.StartsWith("scripts/")
                            ? mainPath.Substring(8)
                            : $"../{mainPath}";
                        writer.WriteLine(string.Format("screen_width={0}", resolution.Width));
                        writer.WriteLine(string.Format("screen_height={0}", resolution.Height));
                        writer.WriteLine(string.Format("script={0}", scriptPath));
                    }
                }
            }
            else
            {
                settings.SaveAs(FileName);
            }
        }

        public void SetBreakpoints(string scriptPath, int[] lineNumbers)
        {
            breakpoints[scriptPath] = new HashSet<int>(lineNumbers);
            foreach (var k in breakpoints.Keys)
            {
                User.SetValue($"breakpointsSet:{k.GetHashCode():X8}",
                    string.Join(",", breakpoints[k]));
            }
        }

        /// <summary>
        /// Upgrades a Sphere game to a full Sphere Studio project.
        /// </summary>
        public void Upgrade()
        {
            var basePath = Path.GetDirectoryName(FileName);
            FileName = Path.Combine(basePath, makeFileName(Name));
            GameOnly = false;
            Compiler = Defaults.Compiler;
            Save();
        }

        private static string makeFileName(string name)
        {
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string pattern = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return $"{Regex.Replace(name, pattern, "_")}.ssproj";
        }
    }

    class UserSettings : IniSettings
    {
        public UserSettings(string filePath) :
            base(new IniFile(filePath, false), "sphereStudio.usr")
        {
        }

        /// <summary>
        /// Stores as a comma-separated list the opened files in the editor.
        /// </summary>
        public string[] Documents
        {
            get { return GetStringArray("openDocuments", new string[0]); }
            set { SetValue("openDocuments", value); }
        }

        /// <summary>
        /// Gets or sets the filepath of the last opened document you viewed.
        /// </summary>
        public string ActiveDocument
        {
            get { return GetString("currentDocument", ""); }
            set { SetValue("currentDocument", value); }
        }

        /// <summary>
        /// Gets or sets the registered name of the engine starter to use when
        /// testing or debugging this project.
        /// </summary>
        public string Engine
        {
            get
            {
                string[] engines = PluginManager.GetNames<IStarter>();
                string defaultEngine =
                    engines.Contains(Session.Settings.Engine) ? Session.Settings.Engine
                    : engines.Length > 0 ? engines[0]
                    : "";
                string value = GetString("engine", defaultEngine);
                return engines.Contains(value) ? value : Session.Settings.Engine;
            }
            set { SetValue("engine", value); }
        }

        /// <summary>
        /// Gets or sets if the Start Page is hidden for this user.
        /// </summary>
        public bool StartPageHidden
        {
            get { return GetBoolean("hideStartPage", false); }
            set { SetValue("hideStartPage", value); }
        }

    }
}