using UnityEngine;

public interface IKillable
{
    public bool Killable { get; }

    public Transform AnchorPoint { get; }
    public void Kill();
}