using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DeadSpace.IPCOverload;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Robust.Shared.Timing;

namespace Content.Server.DeadSpace.IPCOverload;

public sealed partial class IPCOverloadSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IPCOverloadComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<IPCOverloadDoAfter>(OnDoAfter);
    }
    private void OnUse(EntityUid uid, IPCOverloadComponent component, UseInHandEvent args)
    {
        args.Handled = true;

        var doAfter = new DoAfterArgs(EntityManager, args.User, component.DoAfterTime, new IPCOverloadDoAfter(), eventTarget: args.User, target: uid)
        {
            BlockDuplicate = true,
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnDropItem = true,
            BreakOnWeightlessMove = true
        };
        Console.WriteLine("sosal?");
        Console.WriteLine(args.User.ToString() + uid.ToString());
        _doAfter.TryStartDoAfter(doAfter);
    }
    private void OnDoAfter(IPCOverloadDoAfter args)
    {
        if (args.Handled || args.Cancelled)
            return;
        _popup.PopupEntity("sosal?", args.User);
        Console.WriteLine("sosal?");
    }
}
