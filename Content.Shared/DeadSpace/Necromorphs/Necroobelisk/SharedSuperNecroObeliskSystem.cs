// Мёртвый Космос, Licensed under custom terms with restrictions on public hosting and commercial use, full text: https://raw.githubusercontent.com/dead-space-server/space-station-14-fobos/master/LICENSE.TXT

using Robust.Shared.Timing;
using Content.Shared.DeadSpace.Necromorphs.Sanity;
using Content.Shared.Mobs.Components;
using System.Linq;
using Content.Shared.DeadSpace.Administration.GamePreset;

namespace Content.Shared.DeadSpace.Necromorphs.Necroobelisk;

public abstract class SharedSuperNecroobeliskSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedSanitySystem _sharedSanity = default!;



    public override void Initialize()
    {
        SubscribeLocalEvent<SuperMatterialNecroObeliskComponent, EntityUnpausedEvent>(OnNecroobeliskUnpause);
        SubscribeLocalEvent<SuperMatterialNecroObeliskComponent, ComponentShutdown>(OnNecroobeliskStop);
    }

    private void OnNecroobeliskUnpause(EntityUid uid, SuperMatterialNecroObeliskComponent component, ref EntityUnpausedEvent args)
    {
        component.NextPulseTime += args.PausedTime;
        component.NextCheckTimeSanity += args.PausedTime;
        Dirty(uid, component);
    }
    private void OnNecroobeliskStop(EntityUid uid, SuperMatterialNecroObeliskComponent component, ref ComponentShutdown args)
    {
        if (component.MobsInRange == null) return;
        foreach (var i in component.MobsInRange)
        {
            if (HasComp<SanityOverlayComponent>(i)) RemComp<SanityOverlayComponent>(i);
        }
    }

    private void SanityCheckOrConvergence(EntityUid uid, SuperMatterialNecroObeliskComponent component)
    {
        var entities = _lookup.GetEntitiesInRange<MobStateComponent>(_transform.GetMapCoordinates(uid, Transform(uid)), component.RangeSanity);
        foreach (var entity in component.MobsInRange)
        {
            if (!entities.Contains(entity))
            {
                if (HasComp<SanityOverlayComponent>(entity)) RemComp<SanityOverlayComponent>(entity);
            }
        }
        if (entities.Count > 8) component.Percents -= 10;
        foreach (var (entity, comp) in entities)
        {
            if (component.IsStageConvergence)
            {
                var necroobeliskAbsorbEvent = new NecroobeliskAbsorbEvent(entity);
                RaiseLocalEvent(uid, ref necroobeliskAbsorbEvent);
            }

            if (HasComp<ImmunNecroobeliskComponent>(entity))
                continue;

            if (!TryComp<SanityComponent>(entity, out var sanityComponent))
                continue;

            if (component.IsActive)
                _sharedSanity.TryAddSanityLvl(entity, -component.SanityDamage / entities.Count, sanityComponent);

            if (sanityComponent.SanityLevel <= 0)
            {
                RemComp<SanityOverlayComponent>(entity);
                var sanityLostEvent = new SanityLostEvent(entity);
                RaiseLocalEvent(uid, ref sanityLostEvent);
                return;
            }
            EnsureComp<SanityOverlayComponent>(entity);
            component.MobsInRange.Add((entity, comp));
        }

        if (component.MobsAbsorbed >= component.MobsForStageConvergence)
        {
            var necroMoonAppearanceEvent = new NecroMoonAppearanceEvent();
            RaiseLocalEvent(uid, ref necroMoonAppearanceEvent);
        }

        component.NextCheckTimeSanity = _gameTiming.CurTime + component.CheckDurationSanity;
    }

    private void NecroobeliskPulse(EntityUid uid, SuperMatterialNecroObeliskComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_gameTiming.IsFirstTimePredicted)
            return;

        if (!component.IsActive)
            return;

        var ev = new NecroobeliskPulseEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var necroobeliskQuery = EntityQueryEnumerator<SuperMatterialNecroObeliskComponent>();
        while (necroobeliskQuery.MoveNext(out var ent, out var necroobelisk))
        {
            if (necroobelisk.SanityCheckExecuted && _gameTiming.CurTime > necroobelisk.NextCheckTimeSanity)
            {
                necroobelisk.NextCheckTimeSanity = _gameTiming.CurTime + necroobelisk.CheckDurationSanity;
            }
            if (_gameTiming.CurTime > necroobelisk.NextPulseTime)
            {
                NecroobeliskPulse(ent, necroobelisk);
                necroobelisk.NextPulseTime = _gameTiming.CurTime + necroobelisk.TimeUtilPulse;
            }
            if (!necroobelisk.SanityCheckExecuted && _gameTiming.CurTime > necroobelisk.NextCheckTimeSanity)
            {
                SanityCheckOrConvergence(ent, necroobelisk);
                necroobelisk.SanityCheckExecuted = true;
            }

        }
    }

    public virtual void UpdateState(EntityUid uid, SuperMatterialNecroObeliskComponent component)
    {
        if (component.IsActive)
        {
            _appearance.SetData(uid, NecroobeliskVisuals.Unactive, false);
            _appearance.SetData(uid, NecroobeliskVisuals.Active, true);
        }
        if (!component.IsActive)
        {
            _appearance.SetData(uid, NecroobeliskVisuals.Unactive, true);
            _appearance.SetData(uid, NecroobeliskVisuals.Active, false);
        }
    }
}
