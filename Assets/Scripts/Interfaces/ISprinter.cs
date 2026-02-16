// ISprinter.cs
public interface ISprinter
{
    void StartSprint();
    void StopSprint();
    bool IsSprinting { get; }
}   