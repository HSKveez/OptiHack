using Content.Shared.Camera;
using Content.Shared.Overlays;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace OptiHack.Systems;

public sealed class KiroshiSystem
{
    private readonly IEntityManager _entityManager = IoCManager.Resolve<IEntityManager>();
    private readonly ISharedPlayerManager _playerManager = IoCManager.Resolve<ISharedPlayerManager>();
    private bool _enabled = false;

    public void ToggleIcons()
    {
        var uid = _playerManager.LocalEntity;
        if (!_enabled)
        {
            ShowAllIcons(uid);
            _enabled = true;
        }
        else
        {
           HideAllIcons(uid); 
           _enabled = false;
        }
    }
    
    private void ShowAllIcons(EntityUid? uid)
    {
        if (!_entityManager.HasComponent<ShowJobIconsComponent>(uid))
        {
            var jobIconsComponent = new ShowJobIconsComponent
            {
                NetSyncEnabled = false
            };
            
            _entityManager.AddComponent(uid!.Value, jobIconsComponent);
        }
        
        if (!_entityManager.HasComponent<ShowMindShieldIconsComponent>(uid))
        {
            var mindShieldIconsComponent = new ShowMindShieldIconsComponent
            {
                NetSyncEnabled = false
            };
            _entityManager.AddComponent(uid.Value, mindShieldIconsComponent);
        }
        
        if (!_entityManager.HasComponent<ShowCriminalRecordIconsComponent>(uid))
        {
            var criminalRecordIconsComponent = new ShowCriminalRecordIconsComponent
            {
                NetSyncEnabled = false
            };
            _entityManager.AddComponent(uid.Value, criminalRecordIconsComponent);
        }
        
        if (!_entityManager.HasComponent<ShowSyndicateIconsComponent>(uid))
        {
            var syndicateIconsComponent = new ShowSyndicateIconsComponent
            {
                NetSyncEnabled = false
            };
            _entityManager.AddComponent(uid.Value, syndicateIconsComponent);
        }
        
        if (!_entityManager.HasComponent<ShowHealthBarsComponent>(uid))
        {
            var healthBarsComponent = new ShowHealthBarsComponent
            {
                NetSyncEnabled = false,
                DamageContainers =
                {
                    "Biological"
                }
            };
            _entityManager.AddComponent(uid.Value, healthBarsComponent);
        }
        
        if (!_entityManager.HasComponent<ShowHealthIconsComponent>(uid))
        {
            var healthIconsComponent = new ShowHealthIconsComponent
            {
                NetSyncEnabled = false,
                DamageContainers =
                {
                    "Biological"
                }
            };
            _entityManager.AddComponent(uid.Value, healthIconsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out CameraRecoilComponent? recoilComponent))
        {
            recoilComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, recoilComponent);
        }
    }

    private void HideAllIcons(EntityUid? uid)
    {
        if (_entityManager.TryGetComponent(uid, out ShowJobIconsComponent? jobIconsComponent))
        {
            jobIconsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, jobIconsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out ShowMindShieldIconsComponent? mindShieldIconsComponent))
        {
            mindShieldIconsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, mindShieldIconsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out ShowCriminalRecordIconsComponent? criminalRecordIconsComponent))
        {
            criminalRecordIconsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, criminalRecordIconsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out ShowSyndicateIconsComponent? syndicateIconsComponent))
        {
            syndicateIconsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, syndicateIconsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out ShowHealthBarsComponent? healthBarsComponent))
        {
           healthBarsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, healthBarsComponent);
        }
        
        if (_entityManager.TryGetComponent(uid, out ShowHealthIconsComponent? healthIconsComponent))
        {
            healthIconsComponent.NetSyncEnabled = false;
            _entityManager.RemoveComponent(uid.Value, healthIconsComponent);
        }
        
        if (!_entityManager.HasComponent<CameraRecoilComponent>(uid))
        {
            var cameraRecoilComponent = new CameraRecoilComponent
            {
                NetSyncEnabled = false
            };
            _entityManager.AddComponent(uid.Value, cameraRecoilComponent);
        }
    }
}