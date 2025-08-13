using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    private static readonly SizeUnit KibiByte = new(1024, "KiB");
    private static readonly SizeUnit MebiByty = new(KibiByte.Size * 1024, "MiB");
    private static readonly SizeUnit GibiByte = new(MebiByty.Size * 1024, "GiB");
    private static readonly SizeUnit TebiByte = new(GibiByte.Size * 1024, "TiB");

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


public sealed record TactEntry(string Name, EntryType Type, FileMetaData? FileMetaData = null, TactEntry? Parent = null)
{

    private readonly List<TactEntry> _files = [];
    private readonly List<TactEntry> _directories = [];

    public IReadOnlyList<TactEntry> Files => _files.ToImmutableList();
    public IReadOnlyList<TactEntry> Directories => _directories.ToImmutableList();
    
    /// <summary>
    /// Returns both files and directories of this node.
    /// </summary>
    public IEnumerable<TactEntry> All
    {
        get
        {
           if (Files is { Count: > 0 } && Directories is { Count: > 0 })
           {
               return Files.Concat(Directories);
           }
           
           return [];
        }
    }
    
    /// <summary>
    /// Reconstitute the path of this TACT entry, including all parent directories.
    /// </summary>
    /// <returns>Path</returns>
    public string ReconstitutePath()
    {
        var current = this;
        if (current.Parent is null) return current.Name;
        
        var paths = new List<string>();
        
        while (current is not null)
        {
            paths.Add(current.Name);
            current = current.Parent;
        } 
        
        paths.Reverse();
        return string.Join('/', paths);
    }

    /// <summary>
    /// Add a child entry to this TACT entry.
    /// </summary>
    /// <param name="child">Child</param>
    public void AddChild(TactEntry child)
    {
        var collection = child.Type == EntryType.File ? _files : _directories;
        collection.Add(child);
        child = child with { Parent = this };
    }
}

public sealed class TactEntryBuilder(string name, EntryType type, FileMetaData? fileMetaData = null, TactEntryBuilder? parent = null)
{
    private readonly Dictionary<string, TactEntryBuilder> _children =
        new(StringComparer.Ordinal);
    public TactEntryBuilder GetOrAdd(string entryName, EntryType entryType, FileMetaData? entryMetaData = null)
    {
        if (_children.TryGetValue(entryName, out var child)) return child;
        
        child = new TactEntryBuilder(entryName, entryType, entryMetaData, this);
        _children[entryName] = child;

        return child;
    }
    public TactEntry ToTactEntry()
    {
        var me = new TactEntry(name, type, fileMetaData);

        var entries = _children.Values.Select(child => child.ToTactEntry());
        
        foreach (var childEntry in entries)
            me.AddChild(childEntry);
        
        return me;
    }
}