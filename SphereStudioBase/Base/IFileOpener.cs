﻿using System.Drawing;

namespace SphereStudio.Base
{
    /// <summary>
    /// Specifies the interface for a file opener.
    /// </summary>
    public interface IFileOpener : IPlugin
    {
        /// <summary>
        /// Gets the set of file extensions handled by this plugin (sans dots).
        /// </summary>
        string[] FileExtensions { get; }

        /// <summary>
        /// Gets the name of the type of file created by this plugin.
        /// e.g. "Image"
        /// </summary>
        string FileTypeName { get; }

        /// <summary>
        /// Gets the icon used for file types handled by this plugin.
        /// </summary>
        Bitmap FileIcon { get; }

        /// <summary>
        /// Opens an existing file.
        /// </summary>
        /// <param name="fileName">The filename of the asset to edit.</param>
        /// <returns>The DocumentView used for editing the file, or null if no document view is needed.</returns>
        DocumentView Open(string fileName);
    }

    /// <summary>
    /// Specifies the interface for a file opener supporting new document creation.
    /// </summary>
    public interface INewFileOpener : IFileOpener
    {
        /// <summary>
        /// Creates a new, untitled file.
        /// </summary>
        /// <returns>The DocumentView for the new file, or null to cancel document creation.</returns>
        DocumentView New();
    }
}
