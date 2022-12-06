using System;
using System.Collections.Generic;
using AzureFunctions.Models.Interfaces;

namespace AzureFunctions.Models;

public class ElevatorDetailedModel : IElevator
{
    public ElevatorDetailedModel()
    {
        
    }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ConnectionString { get; set; }
    public string Name { get; set; } = "";
    public IElevator.ElevatorStatus Status { get; set; } = IElevator.ElevatorStatus.Disabled;
    public bool DoorStatus { get; set; } = false;
    public int CurrentLevel { get; set; } = 0;
    public int TargetLevel { get; set; } = 0;

    public List<ErrandModel> Errands { get; set; } = new List<ErrandModel>();
}