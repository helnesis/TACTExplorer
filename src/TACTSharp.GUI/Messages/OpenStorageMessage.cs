using CommunityToolkit.Mvvm.Messaging.Messages;
using TACTSharp.GUI.Enums;
using TACTSharp.GUI.Models;
using TACTSharp.GUI.ViewModels;
using TACTSharp.GUI.ViewModels.Configuration;

namespace TACTSharp.GUI.Messages;

public sealed class OpenStorageMessage(StorageType storageType) : AsyncRequestMessage<OpenStorageViewModel>
{
    public StorageType StorageType => storageType;
}

public sealed class CloseStorageMessage;