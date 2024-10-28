using System.Numerics;
using Content.Shared.Administration;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Player;

namespace OptiHack.Commands;

[AnyCommand]
public class OptiHackOverlayCommand : IConsoleCommand
{
    public string Command => "optihack.overlay";
    public string Description => "Shows optihack overlay";
    public string Help => "optihack.overlay";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var overlaySystem = EntitySystem.Get<OptiHackSystem>();
        overlaySystem.Enabled ^= true;
    }
}

public sealed class OptiHackOverlaySystem : Overlay
{
    private readonly IEntityManager _entityManager;
    private readonly SharedTransformSystem _transformSystem;
    private readonly IEyeManager _eyeManager;
    private readonly IUserInterfaceManager _userInterfaceManager;
    private readonly EntityLookupSystem _entityLookup;

    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public OptiHackOverlaySystem(IEntityManager entityManager, IResourceCache resourceCache, SharedTransformSystem transformSystem, IEyeManager eyeManager, IUserInterfaceManager userInterfaceManager, EntityLookupSystem entityLookup)
    {
        _entityManager = entityManager;
        _transformSystem = transformSystem;
        _eyeManager = eyeManager;
        _font = new VectorFont(resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 10);
        _userInterfaceManager = userInterfaceManager;
        _entityLookup = entityLookup;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;
        
        var query = _entityManager.EntityQueryEnumerator<ActorComponent, SpriteComponent, MetaDataComponent>();
        var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();
        var viewport = args.WorldAABB;

        while (query.MoveNext(out var uid, out var actorComponent, out var spriteComponent, out var metadata))
        {
            if (!xformQuery.TryGetComponent(uid, out var xform))
                continue;
            
            if (_entityManager.HasComponent<ActorComponent>(uid))
            {
                var playerSessionName = actorComponent.PlayerSession.Name;
                var playerEntityName = metadata.EntityName;
                
                var aabb = _entityLookup.GetWorldAABB(uid);

                if (!aabb.Intersects(in viewport))
                {
                    continue;
                }
                
                var uiScale = _userInterfaceManager.RootControl.UIScale;
                var screenCoordinates = _eyeManager.WorldToScreen(aabb.Center +
                                                                  new Angle(-_eyeManager.CurrentEye.Rotation).RotateVec(
                                                                      aabb.TopRight - aabb.Center)) + new Vector2(1f, 7f);
                
                var x = 0f;
                var y = 11f;
                var lineoffset = new Vector2(x, y) * uiScale;

                args.ScreenHandle.DrawString(_font, screenCoordinates + lineoffset, playerSessionName, uiScale, Color.Yellow);

                x = 0f;
                y -= 11f;
                lineoffset = new Vector2(x, y) * uiScale;
                
                args.ScreenHandle.DrawString(_font, screenCoordinates + lineoffset, playerEntityName, uiScale, Color.Cyan);
            }
        }
    }
}

public sealed class OptiHackSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    private OptiHackOverlaySystem? _overlay;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;

            _enabled = value;

            if (_enabled)
            {
                _overlay = new OptiHackOverlaySystem(_entityManager, _resourceCache, _transformSystem, _eyeManager, _userInterfaceManager, _entityLookup);
                _overlayManager.AddOverlay(_overlay);
            }
            else
            {
                if (_overlay == null) return;
                _overlayManager.RemoveOverlay(_overlay);
                _overlay = null;
            }
        }
    }

    private bool _enabled;
}