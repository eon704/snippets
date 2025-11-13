///
/// EnergySystem script for a Mars Reborn project.
///

using System.Collections.Generic;
using System.Linq;
using Zenject;


public class EnergySystem
{
    #region Zenject
    private readonly SignalBus signalBus;
    #endregion

    private int maxAvailablePower;
    private int availablePower;

    private readonly List<Building> _poweredBuildings = new();

    public EnergySystem(SignalBus bus, bool debugInfinitePower)
    {
        this.signalBus = bus;
#if UNITY_EDITOR
        if (debugInfinitePower)
        {
            maxAvailablePower = int.MaxValue;
            availablePower = int.MaxValue;
        }
#endif
    }

    public void IncreaseMaxAvailablePower(int maxPower)
    {
        maxAvailablePower = maxPower;
        int powerConsumption = _poweredBuildings.Sum(building => building.Data.energyConsumption);
        availablePower = maxAvailablePower - powerConsumption;
        signalBus.Fire(new EnergyUpdateSignal(maxAvailablePower, availablePower));
    }

    public bool CanPowerBuilding(Building building)
    {
        if (building.Data.energyConsumption > 0)
        {
            return availablePower >= building.Data.energyConsumption;
        }

        return true;
    }

    public bool PowerBuildingUp(Building building)
    {
        // Ignore buildings that don't consume energy
        if (building.Data.energyConsumption <= 0) return true;

        if (!CanPowerBuilding(building))
        {
            return false;
        }

        availablePower -= building.Data.energyConsumption;
        _poweredBuildings.Add(building);
        signalBus.Fire(new EnergyUpdateSignal(maxAvailablePower, availablePower));
        return true;
    }

    public void PowerBuildingDown(Building building)
    {
        // Ignore buildings that don't consume energy
        if (building.Data.energyConsumption <= 0) return;

        // Check if the building is already powered
        if (!_poweredBuildings.Contains(building)) return;


        availablePower += building.Data.energyConsumption;
        _poweredBuildings.Remove(building);
        signalBus.Fire(new EnergyUpdateSignal(maxAvailablePower, availablePower));
    }
}

public class EnergyUpdateSignal
{
    public int MaxAvailablePower;
    public int AvailablePower;

    public EnergyUpdateSignal(int maxAvailablePower, int availablePower)
    {
        MaxAvailablePower = maxAvailablePower;
        AvailablePower = availablePower;
    }
}
