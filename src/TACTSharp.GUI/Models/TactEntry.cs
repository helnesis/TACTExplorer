using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TACTSharp.GUI.Models;


public enum EntryType
{
    Directory,
    File,
}


public readonly record struct SizeUnit(double Size, string Unit)
{
    public static readonly SizeUnit KibiByte = new(1024, "KiB");
    public static readonly SizeUnit MebiByty = new(KibiByte.Size * 1024, "MiB");
    public static readonly SizeUnit GibiByte = new(MebiByty.Size * 1024, "GiB");
    public static readonly SizeUnit TebiByte = new(GibiByte.Size * 1024, "TiB");

    public static SizeUnit GetSize(ulong rawSize)
    {
        if (rawSize < KibiByte.Size)
            return new SizeUnit(rawSize, "B");
        if (rawSize < MebiByty.Size)
            return new SizeUnit(rawSize / KibiByte.Size, KibiByte.Unit);
        if (rawSize < GibiByte.Size)
            return new SizeUnit(rawSize / MebiByty.Size, MebiByty.Unit);
        
        return rawSize < TebiByte.Size ? new SizeUnit(rawSize / GibiByte.Size, GibiByte.Unit) : 
            new SizeUnit(rawSize / TebiByte.Size, TebiByte.Unit);
    }
}


public readonly record struct FileMetaData(
    SizeUnit Size,
    string Type,
    uint FileDataId,
    RootInstance.LocaleFlags LocaleFlags,
    RootInstance.ContentFlags ContentFlags
);


public sealed class TactEntry
{
    public List<TactEntry> Files { get; private set; } = [];
    public List<TactEntry> Directories { get; private set; } = [];
    public string Name { get; private set; } = string.Empty;
    public EntryType Type { get; private set; } 
    public TactEntry? Parent { get; private set; } = null;
    
    public FileMetaData? MetaData { get; private set; } = null;
    public TactEntry(string name, EntryType type, FileMetaData? metadata = null, TactEntry? parent = null)
    {
        if (parent is not null) 
            Parent = parent;
        
        if (metadata is not null)
            MetaData = metadata;
        
        Type = type;
        Name = name;
    }

    public string ReconstitutePath()
    {
        var paths = new List<string>();
        var current = this;
        
        if (current.Parent is null)
            return current.Name;
        
        while (current.Parent is not null)
        {
            paths.Add(current.Name);
            current = current.Parent;
        }
        
        paths.Reverse();
        return string.Join('/', paths);
    }

    public void AddChild(TactEntry child)
    {
        if (child.Type == EntryType.File)
            Files.Add(child);
        else
            Directories.Add(child);
        
        child.Parent = this;
    }
}


public sealed class TactEntryBuilder(string name, EntryType type, FileMetaData? fileMetaData = null, TactEntryBuilder? parent = null)
{
    private readonly string _name = name;
    private readonly EntryType _type = type;
    private readonly FileMetaData? _fileMetaData = fileMetaData;
    private TactEntryBuilder? _parent = parent;

    private readonly Dictionary<string, TactEntryBuilder> _children =
        new(StringComparer.Ordinal);

    public TactEntryBuilder GetOrAdd(string name, EntryType type, FileMetaData? fileMetaData = null, TactEntryBuilder? parent = null)
    {
        if (!_children.TryGetValue(name, out var child))
        {
            child = new TactEntryBuilder(name, type, fileMetaData, this);
            _children[name] = child;
        }
        
        return child;
    }

    public TactEntry ToTactEntry()
    {
        var me = new TactEntry(_name, _type, _fileMetaData);

        foreach (var child in _children.Values)
        {
            var childEntry = child.ToTactEntry();
            me.AddChild(childEntry);
        }
        
        return me;
    }
}