﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

using SourcemapToolkit.SourcemapParser;

using SphereStudio.Utility;

namespace SphereStudio.Debugging
{
    /// <summary>
    /// Describes a location (line and column) in a source file.
    /// </summary>
    public struct CodeLocation
    {
        /// <summary>
        /// The file name of the source file.
        /// </summary>
        public string FileName;

        /// <summary>
        /// The line number within the code.
        /// </summary>
        public int Line;

        /// <summary>
        /// The column number within the line.
        /// </summary>
        public int Column;
    }
    
    /// <summary>
    /// Facilitates debugging by translating code locations using a V3 source map.
    /// </summary>
    public class SourceMapper
    {
        private SourceMapParser m_parser;
        private Dictionary<string, SourceMap> m_maps;

        /// <summary>
        /// Constructs a new SourceMapper.
        /// </summary>
        public SourceMapper()
        {
            this.m_parser = new SourceMapParser();
            this.m_maps = new Dictionary<string, SourceMap>();
        }
        
        /// <summary>
        /// Adds a JS file and associated source map to the mapper.
        /// </summary>
        /// <param name="fileName">The filename of a transpiled target file (NOT the source file).</param>
        /// <param name="mapJson">The JSON text of the V3 source map.</param>
        public void AddSource(string fileName, string mapJson)
        {
            using (var mapReader = new StreamReader(mapJson.ToStream()))
            {
                var map = m_parser.ParseSourceMap(mapReader);
                this.m_maps[fileName] = map;
            }
        }

        /// <summary>
        /// Finds out whether the mapper has an entry for a specified module.
        /// </summary>
        /// <param name="fileName">The filename of the transpiled target file.</param>
        public bool Contains(string fileName)
        {
            return this.m_maps.ContainsKey(fileName);
        }

        /// <summary>
        /// Maps a line number in a transpiled file to its original line number in the source code.
        /// </summary>
        /// <param name="fileName">The filename of the transpiled target file.</param>
        /// <param name="lineNumber">A line number in the transpiled code.</param>
        /// <returns>The corresponding line number in the original source code.</returns>
        public int LineInSource(string fileName, int lineNumber)
        {
            if (!(this.m_maps.ContainsKey(fileName)))
                return lineNumber;

            var q = from item in m_maps[fileName].ParsedMappings
                    where item.GeneratedSourcePosition.ZeroBasedLineNumber == lineNumber - 1
                    select item;
            var mapping = q.FirstOrDefault();
            if (mapping == null)
                return lineNumber;
            return mapping.OriginalSourcePosition.ZeroBasedLineNumber + 1;
        }

        /// <summary>
        /// Maps a source line number back to its line number in the transpiled code.
        /// </summary>
        /// <param name="fileName">The filename of the transpiled target file.</param>
        /// <param name="lineNumber">A line number in the original source code.</param>
        /// <returns>The corresponding line number in the transpiled code.</returns>
        public int LineInTarget(string fileName, int lineNumber)
        {
            if (!(this.m_maps.ContainsKey(fileName)))
                return lineNumber;

            var q = from item in m_maps[fileName].ParsedMappings
                    where item.OriginalSourcePosition.ZeroBasedLineNumber == lineNumber - 1
                    select item;
            var mapping = q.FirstOrDefault();
            if (mapping == null)
                return lineNumber;
            return mapping.GeneratedSourcePosition.ZeroBasedLineNumber + 1;
        }
    }
}
