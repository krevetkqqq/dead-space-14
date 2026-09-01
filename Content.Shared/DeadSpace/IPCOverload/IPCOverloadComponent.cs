using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.DeadSpace.IPCOverload;

[RegisterComponent]
public sealed partial class IPCOverloadComponent : Component
{
    [DataField]
    public float DoAfterTime = 3f;
    [DataField]
    public float OverloadTime = 10f;
    [DataField]
    public float BatteryCost = 0f;
    [ViewVariables(VVAccess.ReadOnly)]
    public IPCOverloadState State = IPCOverloadState.FirstTick;
    public TimeSpan LastTime = TimeSpan.Zero;
}
public enum IPCOverloadState
{
    FirstTick,
    SecondTick,
    ThirdTick
}

[Serializable, NetSerializable]
public sealed partial class IPCOverloadDoAfter : SimpleDoAfterEvent { }
