namespace TACTSharp.GUI.Models.Controls;

public enum Endianness
{
    BigEndian,
    LittleEndian
}

public enum InspectorNumberFormat
{
    Binary,
    Decimal,
    Octal,
    Hexadecimal,
}

public sealed record HexViewerSettings(
    Endianness Endianess,
    InspectorNumberFormat InspectorNumberFormat
    );