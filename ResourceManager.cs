using UnityEngine;
using System;
using System.Collections.Generic;
using Zenject;


public class ResourceManager : IDisposable, IInitializable 
{
    private readonly bool debugInfiniteResource = false;
    private readonly bool debugHighIncome = false;

    public Dictionary<ResourceType, ResourceData> Resources { get; private set; }

    private readonly List<IResourceBuilding> _resourceBuildings = new();

    private readonly SignalBus _signalBus;

    public ResourceManager(SignalBus signalBus, bool debugInfiniteResource = false, bool debugHighIncome = false)
    {
        _signalBus = signalBus;
        _signalBus.Subscribe<SecondTickSignal>(OnSecondTick);
        this.debugInfiniteResource = debugInfiniteResource;
        this.debugHighIncome = debugHighIncome;
        InitializeResources();
    }

    public void Initialize()
    {
        foreach (var resource in Resources.Values)
        {
            _signalBus.Fire(new ResourceChangedSignal(resource.type, resource.amount, resource.maxAmount));
        }
    }

    public void Dispose()
    {
        _signalBus.Unsubscribe<SecondTickSignal>(OnSecondTick);
    }

    public void AddResourceBuilding(IResourceBuilding building)
    {
        _resourceBuildings.Add(building);
    }

    public void RemoveResourceBuilding(IResourceBuilding building)
    {
        _resourceBuildings.Remove(building);
    }

    // Called every second via signal
    private void OnSecondTick()
    {
        if (debugHighIncome)
        {
            foreach (ResourceType type in Resources.Keys)
            {
                AddResources(type, 100);
            }
            return;
        }
        
        _resourceBuildings.ForEach(building => building.OnResourceTick());
    }

    private void InitializeResources()
    {
        Resources = new Dictionary<ResourceType, ResourceData>();

        // Инициализация начальных ресурсов
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            ResourceData data = new ResourceData
            {
                type = type,
                amount = 0f,
                maxAmount = 9000f
            };


            Resources.Add(type, data);
        }

        Resources[ResourceType.Metal].amount = 100f;
        Resources[ResourceType.Gas].amount = 100f;
        Resources[ResourceType.Science].amount = 100f;

#if UNITY_EDITOR
        if (debugInfiniteResource)
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                Resources[type].amount = 9000f;
            }
        }
#endif
    }

    public bool CanAfford(ResourceAmount resourceAmount)
    {
        return Resources[resourceAmount.Type].amount >= resourceAmount.Amount;
    }

    public bool CanAfford(CostEntry costEntry) 
    {
        foreach (var resourceAmount in costEntry.Cost) 
        {
            if (Resources[resourceAmount.Type].amount < resourceAmount.Amount) 
            {
                return false;
            }
        }
        return true;
    }

    public void SpendResources(CostEntry costEntry) 
    {
        if (!CanAfford(costEntry)) return;

        foreach (var resourceAmount in costEntry.Cost) 
        {
            Resources[resourceAmount.Type].amount -= resourceAmount.Amount;
            _signalBus.Fire(new ResourceChangedSignal(resourceAmount.Type, Resources[resourceAmount.Type].amount, Resources[resourceAmount.Type].maxAmount));
        }
    }

    public void AddResources(ResourceType type, float amount) 
    {
        if (!Resources.TryGetValue(type, out var resource)) return;

        resource.amount += amount;
        Resources[type].amount = Mathf.Clamp(Resources[type].amount, 0f, Resources[type].maxAmount);
        _signalBus.Fire(new ResourceChangedSignal(type, Resources[type].amount, Resources[type].maxAmount));
    }

    // Метод для получения количества ресурсов как целое число для UI
    public int GetResourceAmountForUI(ResourceType type)
    {
        if (Resources.TryGetValue(type, out var resource))
        {
            return Mathf.FloorToInt(resource.amount);
        }
        return 0;
    }

    // Метод для получения максимального количества ресурсов как целое число для UI
    public int GetMaxResourceAmountForUI(ResourceType type)
    {
        if (Resources.TryGetValue(type, out var resource))
        {
            return Mathf.FloorToInt(resource.maxAmount);
        }
        return 0;
    }
    
    public void AddResources(CostEntry costEntry) 
    {
        foreach (var resourceAmount in costEntry.Cost) 
        {
            if (resourceAmount.Amount > 0)
            {
                AddResources(resourceAmount.Type, resourceAmount.Amount);
            }
        }
    }
}

[Serializable]
public class ResourceData 
{
   public ResourceType type;
   public float amount;
   public float maxAmount = 1000f;
}

public enum ResourceType 
{
   Metal,
   Gas,
   Science,
}

[Serializable]
public class ResourceAmount
{
    public ResourceType Type = ResourceType.Metal;
    public float Amount = 0;
}

[Serializable]
public class MetalAmount : ResourceAmount
{
    public new ResourceType Type => ResourceType.Metal;
}

[Serializable]
public class GasAmount : ResourceAmount
{
    public new ResourceType Type => ResourceType.Gas;
}

[Serializable]
public class ScienceAmount : ResourceAmount
{
    public new ResourceType Type => ResourceType.Science;
}


[Serializable]
public struct CostEntry
{
    public MetalAmount metalAmount;
    public GasAmount gasAmount;
    public ScienceAmount scienceAmount;

    public List<ResourceAmount> Cost => new()
    {
        metalAmount,
        gasAmount,
        scienceAmount
    };
}

public class ResourceChangedSignal
{
    public ResourceType Type { get; }
    public float Amount { get; }
    public float MaxAmount { get; }

    public ResourceChangedSignal(ResourceType type, float amount, float maxAmount)
    {
        Type = type;
        Amount = amount;
        MaxAmount = maxAmount;
    }
}
