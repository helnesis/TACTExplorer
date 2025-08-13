using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace TACTSharp.GUI.Models.Controls;



/// <summary>
/// Describes a section of a hex view, containing an offset and a byte array.
/// </summary>
/// <param name="Offset">Offset</param>
/// <param name="Bytes">Data</param>
public sealed record HexSection(long Offset, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>
    /// Returns the end of this section.
    /// </summary>
    public long End => Offset + Bytes.Length;
    public string FormattedOffset => Offset.ToString("X8");
    public string FormattedBytes => Convert.ToHexString(Bytes.Span);
    
    public string FormattedAscii => Encoding.ASCII.GetString(Bytes.Span);

}
