using CommunityToolkit.Mvvm.Messaging.Messages;
using TACTSharp.GUI.ViewModels.Controls.Base;

namespace TACTSharp.GUI.Messages.ActionMessages;

public sealed class FileVisualizationMessage<T>(byte[] payload) : AsyncRequestMessage<T> where T : IFileVisualizationViewModel
{
    public byte[] Payload { get; set; } = payload;
}