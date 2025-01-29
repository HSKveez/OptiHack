using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Player;

namespace OptiHack.Systems;

public sealed class OptiHackOverlayDraw : Overlay
{
    private readonly ISharedPlayerManager _playerManager = IoCManager.Resolve<ISharedPlayerManager>();
    private readonly IEntityManager _entityManager;
    private readonly SharedTransformSystem _transformSystem;
    private readonly IEyeManager _eyeManager;
    private readonly IUserInterfaceManager _userInterfaceManager;
    private readonly EntityLookupSystem _entityLookup;
    private readonly ContainerContentAnalyzerSystem _containerContentAnalyzer = new();

    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public OptiHackOverlayDraw(IEntityManager entityManager, IResourceCache resourceCache, SharedTransformSystem transformSystem, IEyeManager eyeManager, IUserInterfaceManager userInterfaceManager, EntityLookupSystem entityLookup)
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
        xformQuery.TryGetComponent(_playerManager.LocalEntity, out var localPlayerXform);
        
        while (query.MoveNext(out var uid, out var actorComponent, out var spriteComponent, out var metadata))
        {
            if (!xformQuery.TryGetComponent(uid, out var xform))
            {
                continue;
            }

            if (xform.MapUid != localPlayerXform!.MapUid)
            {
                continue;
            }
            
            if (_entityManager.HasComponent<ActorComponent>(uid))
            {
                var playerSessionName = actorComponent.PlayerSession.Name;
                var playerEntityName = metadata.EntityName;
                var playerInventoryWarnings = _containerContentAnalyzer.ScanAllFlags(uid); 
                
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
                
                x = 0f;
                y -= -22f;
                lineoffset = new Vector2(x, y) * uiScale;
                
                args.ScreenHandle.DrawString(_font, screenCoordinates + lineoffset, playerInventoryWarnings, uiScale, Color.OrangeRed);
            }
        }
    }
}

public sealed class OptiHackOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    private OptiHackOverlayDraw? _overlay;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;

            _enabled = value;

            if (_enabled)
            {
                _overlay = new OptiHackOverlayDraw(_entityManager, _resourceCache, _transformSystem, _eyeManager, _userInterfaceManager, _entityLookup);
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