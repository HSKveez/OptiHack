using Content.Shared.Camera;
using Content.Shared.Overlays;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;

namespace OptiHack.Patches;

public sealed class KiroshiSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnLocalPlayerAttached);
    }

    void OnLocalPlayerAttached(LocalPlayerAttachedEvent args)
    {
        var localPlayer = args.Entity;
        ShowJobIcons(localPlayer);
        ShowMindShieldIcons(localPlayer);
        ShowHealthBars(localPlayer);
        ShowHealthIcons(localPlayer);
        ShowSyndicateIcons(localPlayer);
        ShowCriminalRecordIcons(localPlayer);
        RemoveRecoil(localPlayer);
    }

    void ShowJobIcons(EntityUid uid)
    {
        if (!HasComp<ShowJobIconsComponent>(uid))
        {
            var component = new ShowJobIconsComponent
            {
                NetSyncEnabled = false
            };
            AddComp(uid, component);
        }
    }

    void ShowMindShieldIcons(EntityUid uid)
    {
        if (!HasComp<ShowMindShieldIconsComponent>(uid))
        {
            var component = new ShowMindShieldIconsComponent
            {
                NetSyncEnabled = false
            };
            AddComp(uid, component);
        }
    }

    void ShowCriminalRecordIcons(EntityUid uid)
    {
        if (!HasComp<ShowCriminalRecordIconsComponent>(uid))
        {
            var component = new ShowCriminalRecordIconsComponent
            {
                NetSyncEnabled = false
            };
            AddComp(uid, component);
        }
    }

    void ShowSyndicateIcons(EntityUid uid)
    {
        if (!HasComp<ShowSyndicateIconsComponent>(uid))
        {
            var component = new ShowSyndicateIconsComponent
            {
                NetSyncEnabled = false
            };
            AddComp(uid, component);
        }
    }

    void ShowHealthBars(EntityUid uid)
    {
        if (!HasComp<ShowHealthBarsComponent>(uid))
        {
            var component = new ShowHealthBarsComponent
            {
                NetSyncEnabled = false,
                DamageContainers =
                {
                    "Biological",
                    "Inorganic",
                    "Silicon"
                }
            };
            AddComp(uid, component);
        }
    }
    
    void ShowHealthIcons(EntityUid uid)
    {
        if (!HasComp<ShowHealthIconsComponent>(uid))
        {
            var component = new ShowHealthIconsComponent
            {
                NetSyncEnabled = false,
                DamageContainers =
                {
                    "Biological",
                    "Inorganic",
                    "Silicon"
                }
            };
            AddComp(uid, component);
        }
    }
    
    void RemoveRecoil(EntityUid localPlayer)
    {
        if (TryComp(localPlayer, out CameraRecoilComponent? component))
        {
            component.NetSyncEnabled = false;
            RemComp(localPlayer, component);
        }
    }
}