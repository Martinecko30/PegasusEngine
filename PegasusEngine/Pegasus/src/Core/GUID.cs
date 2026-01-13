namespace PegasusEngine.Pegasus.Core;


/// <summary>
/// Globally Unique Identifier.
/// A wrapper for an ulong (u-int 64)
/// </summary>
public class GUID : IEquatable<GUID>
{
    public static readonly GUID INVALID = new(0);

    private readonly ulong _value;

    public GUID()
    {
        byte[] buffer = new byte[8];
        Random.Shared.NextBytes(buffer);
        _value = BitConverter.ToUInt64(buffer, 0);
    }
    
    public GUID(ulong value) => _value = value;

    public bool Equals(GUID other) => _value == other._value;
    public override bool Equals(object? obj) => obj is GUID other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    
    public static bool operator ==(GUID left, GUID right) => left.Equals(right);
    public static bool operator !=(GUID left, GUID right) => !left.Equals(right);

    public static implicit operator ulong(GUID guid) => guid._value;
    public static explicit operator GUID(ulong guid) => new(guid);
    
    public override string ToString() => _value.ToString();
}