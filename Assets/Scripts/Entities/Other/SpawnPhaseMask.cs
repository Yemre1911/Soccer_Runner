using System;

[Flags]
public enum SpawnPhaseMask
{
    None = 0,
    Starting = 1 << 0,
    Wave = 1 << 1,
    Break = 1 << 2,
    Ending = 1 << 3,
    Any = ~0
}