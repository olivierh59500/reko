﻿#region License
/* 
 * Copyright (C) 1999-2017 John Källén.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */
#endregion

using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Reko.Core.Configuration
{
    [Serializable]
    [XmlRoot(ElementName="Reko", Namespace = "http://schemata.jklnet.org/Reko/Config/v1", IsNullable = false)]
    public partial class RekoConfiguration_v1
    {
        [XmlArray(ElementName = "Loaders")]
        [XmlArrayItem(ElementName = "Loader")]
        public RekoLoader[] Loaders;

        [XmlArray("RawFiles")]
        [XmlArrayItem("RawFile")]
        public RawFile_v1[] RawFiles;

        [XmlArray("SignatureFiles")]
        [XmlArrayItem("SignatureFile")]
        public SignatureFile_v1[] SignatureFiles;

        [XmlArray("Environments")]
        [XmlArrayItem("Environment")]
        public Environment_v1[] Environments;

        [XmlArray("Architectures")]
        [XmlArrayItem("Architecture")]
        public Architecture_v1[] Architectures;

        [XmlArray("Assemblers")]
        [XmlArrayItem("Assembler")]
        public Assembler_v1[] Assemblers;

        [XmlElement("UiPreferences")]
        public RekoUiPreferences UiPreferences;
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://schemata.jklnet.org/Reko/Config/v1")]
    public partial class Architecture_v1
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Description")]
        public string Description;

        [XmlAttribute("Type")]
        public string Type;
    }

    [Serializable]
    public partial class Assembler_v1
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Description")]
        public string Description;

        [XmlAttribute("Type")]
        public string Type;
    }

    [Serializable]
    public partial class Environment_v1
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Description")]
        public string Description;

        [XmlAttribute("Type")]
        public string Type;

        [XmlAttribute("MemoryMap")]
        public string MemoryMap;

        [XmlArray("TypeLibraries")]
        [XmlArrayItem("TypeLibrary")]
        public TypeLibraryReference_v1[] TypeLibraries;

        [XmlArray("Characteristics")]
        [XmlArrayItem("TypeLibrary")]
        public TypeLibraryReference_v1[] Characteristics;

        [XmlElement("Heuristics")]
        public PlatformHeuristics_v1 Heuristics;

        [XmlArray("SignatureFiles")]
        [XmlArrayItem("SignatureFile")]
        public SignatureFile_v1[] SignatureFiles;

        [XmlArray("Architectures")]
        [XmlArrayItem("Architecture")]
        public PlatformArchitecture_v1[] Architectures;

        // Collect any other platform-specific elements in "Options"
        [XmlAnyElement]
        public XmlElement[] Options;
    }

    [Serializable]
    public partial class RekoLoader
    {
        [XmlAttribute("MagicNumber")]
        public string MagicNumber;

        [XmlAttribute("Type")]
        public string Type;

        [XmlAttribute("Offset")]
        public string Offset;

        [XmlAttribute("Extension")]
        public string Extension;

        [XmlAttribute("Label")]
        public string Label;

        [XmlAttribute("Argument")]
        public string Argument;
    }

    [Serializable]
    public partial class RawFile_v1
    {
        [XmlElement("Entry")]
        public EntryPoint_v1 Entry;

        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Description")]
        public string Description;

        [XmlAttribute("Arch")]
        public string Architecture;

        [XmlAttribute("Env")]
        public string Environment;

        [XmlAttribute("Base")]
        public string Base;
    }

    [Serializable]
    public partial class EntryPoint_v1
    {
        [XmlAttribute("Addr")]
        public string Address;

        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Follow")]
        [DefaultValue(false)]
        public bool Follow;
    }

    [Serializable]
    public partial class SignatureFile_v1
    {
        [XmlAttribute("Filename")]
        public string Filename;

        [XmlAttribute("Label")]
        public string Label;

        [XmlAttribute("Type")]
        public string Type;
    }

    public partial class RekoUiPreferences
    {
        [XmlElement("Style")]
        public StyleDefinition_v1[] Styles;
    }

    [Serializable]
    public partial class StyleDefinition_v1
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Font")]
        public string Font;

        [XmlAttribute("ForeColor")]
        public string ForeColor;

        [XmlAttribute("BackColor")]
        public string BackColor;

        [XmlAttribute("Width")]
        public string Width;

        [XmlAttribute("Cursor")]
        public string Cursor;

        [XmlAttribute("TextAlign")]
        public string TextAlign;

        [XmlAttribute("PaddingTop")]
        public string PaddingTop;

        [XmlAttribute("PaddingBottom")]
        public string PaddingBottom;

        [XmlAttribute("PaddingLeft")]
        public string PaddingLeft;

        [XmlAttribute("PaddingRight")]
        public string PaddingRight;
    }

    [Serializable]
    public partial class TypeLibraryReference_v1
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Arch")]
        public string Arch;

        [XmlAttribute("Loader")]
        public string Loader;

        [XmlAttribute("Module")]
        public string Module;
    }

    [Serializable]
    public class PlatformHeuristics_v1
    {
        [XmlArray("ProcedurePrologs")]
        [XmlArrayItem("Pattern")]
        public BytePattern_v1[] ProcedurePrologs;
    }

    [Serializable]
    public class PlatformArchitecture_v1
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlElement("TrashedRegisters")]
        public string TrashedRegisters;
    }

    public class BytePattern_v1
    {
        [XmlElement("Bytes")]
        public string Bytes;

        [XmlElement("Mask")]
        public string Mask;
    }
}