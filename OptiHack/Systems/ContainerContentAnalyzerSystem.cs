using Content.Shared.Stacks;
using Content.Shared.StoreDiscount.Components;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;


namespace OptiHack.Systems;

public class ContainerContentAnalyzerSystem
{
    private readonly IEntityManager _entityManager = IoCManager.Resolve<IEntityManager>();

    public List<string> ScanSlot(EntityUid? uid, string slot)
    {
        List<string> output = new();
        if (uid == null)
        {
            return output;
        }
        
        if (!TryGetContainedItem(slot, uid, out var itemInSlot, out var exceptionItemInSlot))
        {
            if (exceptionItemInSlot != "")
            {
                output.Add(exceptionItemInSlot);
            }
            return output;
        }
        
        if (slot != "back" && slot != "belt" && slot != "outerClothing")
        {
            output.Add(slot + ":");
            foreach (var item in itemInSlot)
            {
                output.Add($"{EntityNameFormater(item)}");
            }
        }
        
        if(slot == "shoes")
        {
            if (!TryGetContainedItem(slot, uid, out var shoesSlot, out _))
            {
                return output;
            }
            if (!TryGetContainedItem("item", shoesSlot[0], out var itemsInShoes, out _))
            {
                return output;
            }

            foreach (var item in itemsInShoes)
            {
                output.Add($"| {EntityNameFormater(item)}");
            }
        }
        else if(slot == "id")
        {
            if (!TryGetContainedItem(slot, uid, out var shoesSlot, out _))
            {
                return output;
            }
            if (!TryGetContainedItem("PDA-id", shoesSlot[0], out var itemsInShoes, out _))
            {
                return output;
            }

            foreach (var item in itemsInShoes)
            {
                output.Add($"| {EntityNameFormater(item)}");
            }
        }
        else
        {
            if (!TryGetStoragebase(itemInSlot[0], out var itemList, out var _))
            {
                return output;
            }
            output.Add($"\"{EntityNameFormater(itemInSlot[0])}\" slot contains:");
            foreach (var item in itemList)
            {
                output.Add($"| {EntityNameFormater(item)}");

                if (TryGetStoragebase(item, out var itemInItemList, out _))
                {
                    foreach (var itemInItem in itemInItemList)
                    {
                        output.Add($"| | {EntityNameFormater(itemInItem)}");

                        if (TryGetStoragebase(itemInItem, out var itemInItemInItemList, out _))
                        {
                            foreach (var itemInItemInItem in itemInItemInItemList)
                            {
                                output.Add($"| | | {EntityNameFormater(itemInItemInItem)}");
                            }
                        }
                    }
                }
            }
        }
        return output;
    }

    public List<string> ScanAllSlots(EntityUid? uid)
    {
        string[] slots =
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
        
        List<string> output = new();
        List<string> result = new();
        foreach (var slot in slots)
        {
             output = ScanSlot(uid, slot);
             foreach (var line in output)
             {
                 result.Add(line);
             }
        }

        return result;
    }
    
    

    public EntityUid? TargetUidParser(string targetUid)
    {
        if (!int.TryParse(targetUid, out var entInt))
        {
            return null;
        }

        var netEntity = new NetEntity(entInt);
        if (!_entityManager.TryGetEntity(netEntity, out var uid))
        {
            return null;
        }
        return uid;
    }

    private string EntityNameFormater(EntityUid uid)
    {
        _entityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata);
        var entityName = metadata!.EntityName;
        var entityCount = "";
        var entityFlag = "";
        var contrabandItems = new List<string>
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
        
        if (_entityManager.TryGetComponent<StackComponent>(uid, out var stackComponent))
        {
            entityCount = $"{{x{stackComponent.Count}}}";
        }
        
        if (_entityManager.TryGetComponent<GunComponent>(uid, out _))
        {
            entityFlag += "{GUN}";
        }

        if (_entityManager.TryGetComponent<StoreDiscountComponent>(uid, out _))
        {
            entityFlag += "{UPLINK}";
        }
        
        if (contrabandItems.Contains(metadata!.EntityPrototype.ID))
        {
            entityFlag += "{ANTAG}";    
        }
        
        var entityData = $"{entityCount}{entityFlag} {entityName}";
        return entityData;
    }

    private bool TryGetContainedItem(string slot, EntityUid? uid, out IReadOnlyList<EntityUid> slotEntityUid, out string exception)
    {
        if (!_entityManager.TryGetComponent<ContainerManagerComponent>(uid, out var containerManagerComponent))
        {
            exception = "entity has no containers";
            slotEntityUid = null!;
            return false;
        }

        if (!containerManagerComponent.Containers.Keys.Contains(slot))
        {
            exception = "";
            slotEntityUid = null!;
            return false;
        }

        if (containerManagerComponent.Containers[slot].ContainedEntities.Count == 0)
        {
            exception = "";
            slotEntityUid = null!;
            return false;
        }

        exception = "";
        slotEntityUid = containerManagerComponent.Containers[slot].ContainedEntities;
        return true;
    }

    private bool TryGetStoragebase(EntityUid itemInSlot, out IReadOnlyList<EntityUid> itemList, out string exception)
    {
        if (!_entityManager.TryGetComponent<ContainerManagerComponent>(itemInSlot, out var slotContainer))
        {
            exception = "";
            itemList = null!;
            return false;;
        }

        if (!slotContainer.Containers.Keys.Contains("storagebase"))
        {
            exception = "";
            itemList = null!;
            return false;
        }
        exception = "";
        itemList = slotContainer.Containers["storagebase"].ContainedEntities;
        return true;
    }

    public string ScanAllFlags(EntityUid? uid)
    {
        string flags = "";
        var flagList = new List<string>();
        
        string[] slots =
        {
            "shoes",
            "gloves",
            "entity_storage",
            "outerClothing",
            "id",
            "implant",
            "belt",
            "back",
            "body_part_slot_right hand",//виздены хуесосы не могут понять как буквы писать
            "body_part_slot_right_hand",
            "body_part_slot_left hand",
            "body_part_slot_left_hand",
            "pocket1",
            "pocket2",
            "suitstorage"
        };
        
        foreach (var slot in slots)
        {
            if (uid == null)
            {
                continue;
            }

            if (!TryGetContainedItem(slot, uid, out var itemInSlot, out _))
            {
                continue;
            }
            if (slot != "back" && slot != "belt" && slot != "outerClothing")
            {
                foreach (var item in itemInSlot)
                {
                    ScanFlag(item, ref flagList);
                }
            }
            
            if(slot == "shoes")
            {
                if (!TryGetContainedItem(slot, uid, out var shoesSlot, out _))
                {
                    continue;
                }
                if (!TryGetContainedItem("item", shoesSlot[0], out var itemsInShoes, out _))
                {
                    continue;
                }

                foreach (var item in itemsInShoes)
                {
                    ScanFlag(item, ref flagList);
                }
            }
            else if(slot == "id")
            {
                if (!TryGetContainedItem(slot, uid, out var idSlot, out _))
                {
                    continue;
                }
                if (!TryGetContainedItem("PDA-id", idSlot[0], out var itemsInPda, out _))
                {
                    continue;
                }

                foreach (var item in itemsInPda)
                {
                    ScanFlag(item, ref flagList);
                }
            }
            else
            {
                ScanFlag(itemInSlot[0], ref flagList);
                if (!TryGetStoragebase(itemInSlot[0], out var itemList, out _))
                {
                    continue;
                }
                
                foreach (var item in itemList)
                {
                    ScanFlag(item, ref flagList);

                    if (TryGetStoragebase(item, out var itemInItemList, out _))
                    {
                        foreach (var itemInItem in itemInItemList)
                        {
                            ScanFlag(itemInItem, ref flagList);

                            if (TryGetStoragebase(itemInItem, out var itemInItemInItemList, out _))
                            {
                                foreach (var itemInItemInItem in itemInItemInItemList)
                                {
                                    ScanFlag(itemInItemInItem, ref flagList);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        foreach (var flag in flagList)
        {
            flags += flag + " ";
        }
        
        return flags;
    }

    private void ScanFlag(EntityUid? uid, ref List<string> flagList)
    {
        _entityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata);
        var contrabandItems = new List<string>
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
            "UplinkImplant",
            "C4",
            "ClusterGrenade",
            "ExGrenade"
        };

        if (contrabandItems.Contains(metadata!.EntityPrototype.ID) && !flagList.Contains("ANTAG"))
        {
            flagList.Add("ANTAG");
        }
        
        if (_entityManager.TryGetComponent<StoreDiscountComponent>(uid, out _) && !flagList.Contains("UPLINK"))
        {
            flagList.Add("UPLINK");
        }
        
        if (_entityManager.TryGetComponent<GunComponent>(uid, out _) && !flagList.Contains("GUN"))
        {
            flagList.Add("GUN");
        }
    }
}
