using UnityEngine;

public interface ICupInfo
{
    bool IsOccupied { get; }
    float TeaFillAmount { get; }
    bool TeaFull { get; }
    int BobaFillAmount { get; }
    bool BobaFull { get; }
    bool IsSealed {  get; }

}
