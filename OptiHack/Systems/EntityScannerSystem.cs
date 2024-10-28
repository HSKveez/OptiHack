using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Clothing.Components;
using Content.Shared.Stacks;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Console;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;


namespace OptiHack.Systems;

public class EntityScannerSystem
{
    private readonly IEntityManager _entityManager = IoCManager.Resolve<IEntityManager>();

    public void ScanSlot(IConsoleShell shell, EntityUid? uid, string slot)
    {
        if (uid == null)
        {
            return;
        }

        if (!TryGetContainedItem(slot, uid, out var itemInSlot, out var exceptionItemInSlot))
        {
            shell.WriteLine(exceptionItemInSlot);
            return;
        }
        if (slot != "back" && slot != "belt" && slot != "outerClothing")
        {
            shell.WriteLine($"\"{slot.ToUpper()}\" slot contains:");
            foreach (var item in itemInSlot)
            {
                var entityName = EntityNameFormater(item);
                shell.WriteLine(entityName);
            }
        }
        else
        {
            if (!TryGetStoragebase(itemInSlot[0], out var itemList, out var exceptionStoragebase))
            {
                shell.WriteLine(exceptionStoragebase);
                return;
            }
            shell.WriteLine($"\"{slot.ToUpper()}\" slot contains:");
            foreach (var item in itemList)
            {
                shell.WriteLine($"- {EntityNameFormater(item)}");

                if (TryGetStoragebase(item, out var itemInItemList, out _))
                {
                    foreach (var itemInItem in itemInItemList)
                    {
                        shell.WriteLine($"- - {EntityNameFormater(itemInItem)}");

                        if (TryGetStoragebase(itemInItem, out var itemInItemInItemList, out _))
                        {
                            foreach (var itemInItemInItem in itemInItemInItemList)
                            {
                                shell.WriteLine($"- - - {EntityNameFormater(itemInItemInItem)}");
                            }
                        }
                    }
                }
            }
        }
    }

    public string[] ScanAllSlots(IConsoleShell shell, EntityUid? uid)
    {
        string[] tags = {};
        string[] slots =
        {
            "shoes",
            "gloves",
            "entity_storage",
            "outerClothing",
            "implant",
            "belt",
            "back",
            
        };

        foreach (var slot in slots)
        {
            ScanSlot(shell, uid, slot);
        }
        return tags;
    }

    public EntityUid? TargetUidParser(IConsoleShell shell, string targetUid)
    {
        if (!int.TryParse(targetUid, out var entInt))
        {
            shell.WriteLine("uid must be a number");
            return null;
        }

        var netEntity = new NetEntity(entInt);
        if (!_entityManager.TryGetEntity(netEntity, out var uid))
        {
            shell.WriteLine("invalid uid");
            return null;
        }
        return uid;
    }

    public string EntityNameFormater(EntityUid uid)
    {
        _entityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata);
        var entityName = metadata!.EntityName;
        var entityCount = "";
        var entityCountFlag = "";
        var entityFlag = "";
        if (_entityManager.TryGetComponent<StackComponent>(uid, out var stackComponent))
        {
            entityCount = $"[x{stackComponent.Count}]";
        }
        if (_entityManager.TryGetComponent<GunComponent>(uid, out var gunComponent))
        {
            entityFlag += "[GUN]";
        }

        if (_entityManager.TryGetComponent<SolutionContainerManagerComponent>(uid, out var solutionContainerComponent) && !_entityManager.TryGetComponent<ClothingComponent>(uid, out _))
        {
            var containedChem = "";
            var solutionContainers = solutionContainerComponent.Containers;

            foreach (var solutionContainer in solutionContainers)
            {
                _entityManager.TryGetComponent<ContainerManagerComponent>(uid, out var containerComponent);
                var solutionContainerEntity = containerComponent!.Containers[$"solution@{solutionContainer}"].ContainedEntities[0];
                _entityManager.TryGetComponent<SolutionComponent>(solutionContainerEntity, out var solutionComponent);

                foreach (var contents in solutionComponent!.Solution.Contents)
                {
                    containedChem += contents + " ";
                }
            }

            if (containedChem != "")
            {
                entityFlag += $"[CHEMS:{containedChem}]";
            }
        }

        var entityData = $"{entityCount}{entityFlag} {entityName}";
        return entityData;
    }

    public bool TryGetContainedItem(string slot, EntityUid? uid, out IReadOnlyList<EntityUid> slotEntityUid, out string exception)
    {
        if (!_entityManager.TryGetComponent<ContainerManagerComponent>(uid, out var containerManagerComponent))
        {
            exception = "entity has no containers";
            slotEntityUid = null!;
            return false;
        }

        if (!containerManagerComponent.Containers.Keys.Contains(slot))
        {
            exception = $"entity does not have \"{slot.ToUpper()}\" container";
            slotEntityUid = null!;
            return false;
        }

        if (containerManagerComponent.Containers[slot].ContainedEntities.Count == 0)
        {
            exception = $"\"{slot.ToUpper()}\" slot is empty";
            slotEntityUid = null!;
            return false;
        }

        exception = "";
        slotEntityUid = containerManagerComponent.Containers[slot].ContainedEntities;
        return true;
    }

    public bool TryGetStoragebase(EntityUid itemInSlot, out IReadOnlyList<EntityUid> itemList, out string exception)
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
}
