using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using TACTSharp.GUI.Enums;
using TACTSharp.GUI.Messages;
using TACTSharp.GUI.Messages.ActionMessages;
using TACTSharp.GUI.Models;
using TACTSharp.GUI.Services.Listfile;
using TACTSharp.GUI.Services.Tact;
using TACTSharp.GUI.ViewModels.Controls;

namespace TACTSharp.GUI.ViewModels;

public partial class MainWindowViewModel(ITactService tact, IListfileService listfile, IDialogService dialogService) : ViewModelBase
{
    [ObservableProperty] private string _searchText = string.Empty;
    
    [ObservableProperty] private TactEntry? _root;
    [ObservableProperty] private HierarchicalTreeDataGridSource<TactEntry>? _hierarchicalTreeDataGridSource;

    [ObservableProperty] private TactEntry? _selectedEntry;
    [ObservableProperty] private TactEntry? _childSelectedEntry;
    
    [ObservableProperty] private HierarchicalTreeDataGridSource<TactEntry>? _childHierarchicalTreeDataGridSource;
    
    private static IconConverter? _fileIconConverter;
    public static IValueConverter FileIconConverter
    {
        get
        {
            if (_fileIconConverter is not null) return _fileIconConverter;
            
            using var fileStream = AssetLoader.Open(new Uri("avares://TACTSharp.GUI/Assets/file.png"));
            using var folderStream = AssetLoader.Open(new Uri("avares://TACTSharp.GUI/Assets/folder.png"));
            var fileIcon = new Bitmap(fileStream);
            var folderIcon = new Bitmap(folderStream);
            
            _fileIconConverter = new IconConverter(fileIcon, folderIcon);

            return _fileIconConverter;
        }
    }

    [RelayCommand]
    private async Task Extract()
    {
        if (SelectedEntry is null) return;
        
        var dialog = await dialogService.ShowOpenFolderDialogAsync(this);

        if (dialog is null) return;
        
        var entry = SelectedEntry;
        
        // Prioritize child selection if available.
        if (ChildSelectedEntry is not null) entry = ChildSelectedEntry;
        
        await InternalExtract(entry.Type == EntryType.Directory ? entry.Files : [entry], dialog.LocalPath);
    }


    private async Task InternalExtract(IReadOnlyList<TactEntry> entries, string extractTo)
    {
        if (entries.Count == 0 || tact.Instance?.Root is null) return;

        foreach (var entry in entries)
        {
            var fid = entry.FileMetaData?.FileDataId ?? 0;

            if (fid != 0)
            {
                // reconstruct the path
                var p = entry.ReconstitutePath();
                var filePath = Path.Combine(extractTo, p);
                
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                
                var fileBytes = tact.Instance.OpenFileByFDID(fid);
                
                await using var stream = File.Create(filePath);
                await stream.WriteAsync(fileBytes);
            }
            
            if (entry.Files.Count > 0)
            {
                await InternalExtract(entry.Files, extractTo);
            }
        }
        
    }
    
    
    [RelayCommand]
    private static async Task OpenStorageConfigurationAsync(StorageType storageType)
        => await WeakReferenceMessenger.Default.Send(new OpenStorageMessage(storageType));


    partial void OnSearchTextChanged(string value)
    {
        if (Root is null || ChildHierarchicalTreeDataGridSource is null || SelectedEntry is null) return;
        var normalizedTerm = value.ToLowerInvariant();

        var filteredEntries = ChildHierarchicalTreeDataGridSource
            .Items.Where(p => p.Name.Contains(normalizedTerm))
            .ToList();
        
        if (filteredEntries is { Count: > 0 } && !string.IsNullOrEmpty(normalizedTerm))
        {
            CreateChildHierarchicalTreeDataGridSource(filteredEntries);
        }
        else
        {
            var children = SelectedEntry.Files.Concat(SelectedEntry.Directories);
            CreateChildHierarchicalTreeDataGridSource(children);

        }
    }
    private void CreateChildHierarchicalTreeDataGridSource(IEnumerable<TactEntry> tactEntry)
    {
        
        ChildHierarchicalTreeDataGridSource = new HierarchicalTreeDataGridSource<TactEntry>(tactEntry)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<TactEntry>(
                    new TemplateColumn<TactEntry>("Name", "Name"),
                    x => x.All,
                    x => x.All.Any()
                ),
                new TextColumn<TactEntry, string>("Extension", x => x.FileMetaData != null ? x.FileMetaData.Value.Type : string.Empty),
                new TextColumn<TactEntry, uint>("FileDataId", x => x.FileMetaData != null ? x.FileMetaData.Value.FileDataId : 0),
                new TextColumn<TactEntry, RootInstance.LocaleFlags>("LocaleFlags", x => x.FileMetaData != null ? x.FileMetaData.Value.LocaleFlags : default),
                new TextColumn<TactEntry, RootInstance.ContentFlags>("ContentFlags", x => x.FileMetaData != null ? x.FileMetaData.Value.ContentFlags : default),
                new TextColumn<TactEntry, string>("Size", x => x.FileMetaData != null ? $"{Math.Ceiling(x.FileMetaData.Value.Size.Size)} {x.FileMetaData.Value.Size.Unit}" : ""),
                new TextColumn<TactEntry, EntryType>("Type", x => x.Type),

            }
        };

        if (ChildHierarchicalTreeDataGridSource.RowSelection != null)
            ChildHierarchicalTreeDataGridSource.RowSelection.SelectionChanged += ChildSelectionChanged;
    }
    public void CreateHierarchicalTreeDataGridSource(TactEntry tactEntry)
    {
        HierarchicalTreeDataGridSource = new HierarchicalTreeDataGridSource<TactEntry>(tactEntry.Directories)
        {
            Columns =
            {
                new HierarchicalExpanderColumn<TactEntry>(
                    new TemplateColumn<TactEntry>("Name", "Name"),
                    x => x.Directories,
                    x => x.Directories.Count > 0
                ),
                new TextColumn<TactEntry, EntryType>("Type", x => x.Type),
            }
        };

        if (HierarchicalTreeDataGridSource.RowSelection != null)
            HierarchicalTreeDataGridSource.RowSelection.SelectionChanged += SelectionChanged;
    }
    
    private void SelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<TactEntry> e)
    {
        if (HierarchicalTreeDataGridSource == null) return;
        
        var rowSelection = GetRowSelection(HierarchicalTreeDataGridSource);

        if (rowSelection.SelectedItem is null) return;
        
        SelectedEntry = rowSelection.SelectedItem;
        ChildSelectedEntry = null;
        var children = SelectedEntry.Files.Concat(SelectedEntry.Directories);
        CreateChildHierarchicalTreeDataGridSource(children);
    }
    
    private async void ChildSelectionChanged(object? sender, TreeSelectionModelSelectionChangedEventArgs<TactEntry> e)
    {
        if (ChildHierarchicalTreeDataGridSource == null) return;
        var rowSelection = GetRowSelection(ChildHierarchicalTreeDataGridSource);
        
        if (rowSelection.SelectedItem is null) return;
        
        ChildSelectedEntry = rowSelection.SelectedItem;

        if (ChildSelectedEntry is not { Type: EntryType.File, FileMetaData: { } fileMetaData }) return;
        
        // Handle file visualization based on file type.
        var fileBytes = tact.Instance!.OpenFileByFDID(fileMetaData.FileDataId);
        
        switch (fileMetaData.Type)
        {
            case ".m2":
            case ".wmo":
                await WeakReferenceMessenger.Default.Send(new FileVisualizationMessage<HexControlViewModel>(fileBytes));
                break;
        }

    }
    
    private static ITreeDataGridRowSelectionModel<TactEntry> GetRowSelection(ITreeDataGridSource source)
    {
        return source.Selection as ITreeDataGridRowSelectionModel<TactEntry> ??
               throw new InvalidOperationException("Expected a row selection model.");
    }
    
    private class IconConverter(Bitmap file, Bitmap folder) : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is EntryType entryType)
                return entryType == EntryType.File ? file : folder;
            
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


