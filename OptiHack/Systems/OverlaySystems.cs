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
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly SharedTransformSystem _transformSystem;
    private readonly EntityLookupSystem _entityLookup;
    private readonly ContainerContentAnalyzerSystem _containerContentAnalyzer = new();
    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public OptiHackOverlayDraw(SharedTransformSystem transformSystem, EntityLookupSystem entityLookup)
    {
        IoCManager.InjectDependencies(this);
        _transformSystem = transformSystem;
        _entityLookup = entityLookup;
        _font = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 10);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (args.ViewportControl == null)
            return;

        var query = _entityManager.EntityQueryEnumerator<ActorComponent, SpriteComponent, MetaDataComponent>();
        var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();
        var viewport = args.WorldAABB;
        var uiScale = _userInterfaceManager.RootControl.UIScale;

        if (!xformQuery.TryGetComponent(_playerManager.LocalEntity, out var localPlayerXform))
            return;
        
        var baseLineOffsets = new Vector2[]
        {
            new(0f, 11f),    // entityName
            new(0f, 0f),   // sessionName
            new(0f, 22f)   // inventoryWarnings
        };

        while (query.MoveNext(out var uid, out var actorComponent, out var spriteComponent, out var metadata))
        {
            if (!xformQuery.TryGetComponent(uid, out var xform) || xform.MapUid != localPlayerXform.MapUid)
                continue;

            var aabb = _entityLookup.GetWorldAABB(uid);
            if (!aabb.Intersects(in viewport))
                continue;
            
            var centerToTopRight = aabb.TopRight - aabb.Center;
            var rotatedOffset = new Angle(-_eyeManager.CurrentEye.Rotation).RotateVec(centerToTopRight);
            var screenCoordinates = _eyeManager.WorldToScreen(aabb.Center + rotatedOffset) + new Vector2(1f, 7f);
            
            var playerSessionName = actorComponent.PlayerSession.Name;
            var playerEntityName = metadata.EntityName;
            var playerInventoryWarnings = _containerContentAnalyzer.ScanAllFlags(uid);
            
            for (var i = 0; i < baseLineOffsets.Length; i++)
            {
                var (text, color) = i switch
                {
                    0 => (playerSessionName, Color.Yellow),
                    1 => (playerEntityName, Color.Cyan),
                    _ => (playerInventoryWarnings, Color.OrangeRed)
                };

                args.ScreenHandle.DrawString(
                    _font,
                    screenCoordinates + (baseLineOffsets[i] * uiScale),
                    text,
                    uiScale,
                    color
                );
            }
        }
    }
}

public sealed class OptiHackOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;

    private OptiHackOverlayDraw? _overlay;
    private bool _enabled;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value) return;
            _enabled = value;

            if (_enabled)
            {
                _overlay ??= new OptiHackOverlayDraw(_transformSystem, _entityLookup);
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
}