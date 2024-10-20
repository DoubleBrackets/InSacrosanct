using UnityEngine;

public abstract class GearBase : MonoBehaviour
{
    protected Protag Protag;
    protected InputProvider InputProvider;

    public void Initialize(InputProvider inputProvider, Protag protag)
    {
        InputProvider = inputProvider;
        Protag = protag;
    }

    public abstract void OnEquip();

    public abstract void Tick();

    public abstract void OnUnequip();

    public abstract void Show(bool visible);

    public abstract bool IsUsing();
}