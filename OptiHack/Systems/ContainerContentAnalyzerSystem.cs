using System.Text;
using Content.Shared.Stacks;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;


namespace OptiHack.Systems;

public class ContainerContentAnalyzerSystem
{
    private readonly IEntityManager _entityManager = IoCManager.Resolve<IEntityManager>();


    private static readonly HashSet<string> ContrabandItems = new()
    {
        "EnergyDagger",
        "ToolboxElectricalTurretFilled",
        "EnergySword",
        "EnergySwordDouble",
        "BloodDagger",
        "EnergyCrossbowMini",
        "BaseBallBatHomeRun",
        "BetrayalDagger",
        "EmpGrenade",
        "WhiteholeGrenade",
        "GrenadeIncendiary",
        "PenExploding",
        "GrenadeShrapnel",
        "MobGrenadePenguin",
        "SupermatterGrenade",
        "SyndieMiniBomb",
        "SyndiCrewMonitorEmpty",
        "DehydratedSpaceCarp",
        "SyndicateJawsOfLife",
        "CyberPen",
        "ClothingMaskGasVoiceChameleon",
        "FlashlightEmp",
        "AgentIDCard",
        "Hypopen",
        "ChameleonProjector",
        "ReinforcementRadioSyndicateAncestor",
        "Emag",
        "ExperimentalSyndicateTeleporter",
        "ClothingShoesChameleonNoSlips",
        "ClothingEyesThermalVisionGogglesSyndie",
        "ClothingEyesNightVisionGogglesSyndie",
        "ClothingOuterVestWeb",
        "ClothingOuterHardsuitSyndie",
        "ClothingOuterHardsuitSyndieElite",
        "ClothingOuterHardsuitJuggernaut",
        "ThievingGloves",
        "ClothingHandsGlovesNorthStar",
        "StorageImplant",
        "ScramImplant",
        "FreedomImplant",
        "DnaScramblerImplant",
        "EmpImplant",
        "UplinkImplant"
    };

    private static readonly HashSet<string> Slots = new ()
    {
        "shoes",
        "gloves",
        "entity_storage",
        "outerClothing",
        "implant",
        "belt",
        "back",
        "body_part_slot_right hand", //виздены хуесосы не могут понять как буквы писать
        "body_part_slot_right_hand",
        "body_part_slot_left hand",
        "body_part_slot_left_hand",
        "pocket1",
        "pocket2",
        "suitstorage",
        "id"
    };
    
    
    public List<string> ScanSlot(EntityUid? uid, string slot)
    {
        var output = new List<string>();
        if (uid == null) return output;

        if (!TryGetContainedItems(slot, uid, out var items, out var exception))
        {
            if (!string.IsNullOrEmpty(exception)) output.Add(exception);
            return output;
        }

        if (IsSpecialSlot(slot))
        {
            ProcessSpecialSlot(slot, items, output);
        }
        else
        {
            output.Add($"{slot}:");
            output.AddRange(ProcessContainerItems(items));
        }

        return output;
    }
    
    private bool IsSpecialSlot(string slot) => 
        slot is "back" or "belt" or "outerClothing" or "shoes" or "id";

    private void ProcessSpecialSlot(string slot, IReadOnlyList<EntityUid> items, List<string> output)
    {
        switch (slot)
        {
            case "shoes" or "id":
                var containerType = slot == "shoes" ? "item" : "PDA-id";
                if (TryGetContainedItems(containerType, items.FirstOrDefault(), out var subItems, out _))
                {
                    output.Add($"{EntityNameFormater(items.FirstOrDefault())}:");
                    output.AddRange(subItems.Select(item => $"| {EntityNameFormater(item)}"));
                }
                break;
            
            case "back" or "belt" or "outerClothing":
                if (TryGetContainedItems("storagebase",items.FirstOrDefault(), out var storageItems, out _))
                {
                    output.Add($"{EntityNameFormater(items.FirstOrDefault())}:");
                    output.AddRange(ProcessContainerItems(storageItems));
                }
                break;
        }
    }
    
    private List<string> ProcessContainerItems(IReadOnlyList<EntityUid> items, int depth = 1, HashSet<string>? parentFlags = null)
    {
        var result = new List<string>();
        foreach (var item in items)
        {
            var itemFlags = parentFlags != null 
                ? new HashSet<string>(parentFlags) 
                : new HashSet<string>();
            
            ScanFlags(item, itemFlags);
        
            result.Add($"{new string('|', depth)} {EntityNameFormater(item, itemFlags)}");
        
            if (TryGetContainedItems("storagebase", item, out var nestedItems, out _))
            {
                result.AddRange(ProcessContainerItems(nestedItems, depth + 1, itemFlags));
            }
        }
        return result;
    }


    public List<string> ScanAllSlots(EntityUid? uid)
    {
        var result = new List<string>();
        foreach (var slot in Slots)
        {
            var output = ScanSlot(uid, slot);
            result.AddRange(output);
        }

        return result;
    }
    
    

    public EntityUid? TargetUidParser(string targetUid)
    {
        if (!int.TryParse(targetUid, out var entInt))
            return null;

        var netEntity = new NetEntity(entInt);
        if (!_entityManager.TryGetEntity(netEntity, out var uid))
            return null;
        
        return uid;
    }

    private string EntityNameFormater(EntityUid uid, HashSet<string>? flags = null)
    {
        var sb = new StringBuilder();
        _entityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata);
        
        var itemFlags = new HashSet<string>();
        ScanFlags(uid, itemFlags);
        
        if (flags != null) itemFlags.UnionWith(flags);
        
        sb.Append(metadata?.EntityName);
        
        if (_entityManager.TryGetComponent<StackComponent>(uid, out var stack))
        {
            sb.Append($" {{x{stack.Count}}}");
        }
        
        foreach (var flag in itemFlags.OrderBy(f => f))
        {
            sb.Append($" {{{flag}}}");
        }

        return sb.ToString();
    }

    private bool TryGetContainedItems(string slot, EntityUid? uid, 
        out IReadOnlyList<EntityUid> items, out string exception)
    {
        items = null!;
        exception = string.Empty;

        if (!_entityManager.TryGetComponent<ContainerManagerComponent>(uid, out var containerManager))
        {
            exception = "entity has no containers";
            return false;
        }

        if (!containerManager.Containers.TryGetValue(slot, out var container) || 
            container.ContainedEntities.Count == 0)
            return false;

        items = container.ContainedEntities;
        return true;
    }

    public string ScanAllFlags(EntityUid? uid)
    {
        var flags = new HashSet<string>();
        
        foreach (var slot in Slots)
        {
            if (uid == null || !TryGetContainedItems(slot, uid, out var items, out _))
                continue;
            
            foreach (var item in items)
            {
                ScanItemRecursive(item, flags);
            }
        }
        
        return string.Join(" ", flags);
    }
    
    private void ScanItemRecursive(EntityUid item, HashSet<string> flags)
    {
        var localFlags = new HashSet<string>();
        ScanFlags(item, localFlags);
        flags.UnionWith(localFlags);
    
        if (TryGetContainedItems("storagebase", item, out var nestedItems, out _))
        {
            foreach (var nestedItem in nestedItems)
            {
                ScanItemRecursive(nestedItem, flags);
            }
        }
    }

    private void ScanFlags(EntityUid uid, HashSet<string> flags)
    {
        if (_entityManager.HasComponent<GunComponent>(uid))
        {
            flags.Add("GUN");
        }
        
        if (_entityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata) && 
            ContrabandItems.Contains(metadata.EntityPrototype?.ID ?? string.Empty))
        {
            flags.Add("ANTAG");
        }
    }
}