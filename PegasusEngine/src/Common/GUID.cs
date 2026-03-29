namespace PegasusEngine.Core;


/// <summary>
/// Globally Unique Identifier.
/// A zero-allocation wrapper for an ulong (uint64)
/// </summary>
public readonly struct GUID : IEquatable<GUID>
{
    public static readonly GUID INVALID = new(0);

    private readonly ulong _value;

    public GUID()
    {
        _value = (ulong)Random.Shared.NextInt64();
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