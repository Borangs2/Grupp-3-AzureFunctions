using System;
using System.ComponentModel.DataAnnotations.Schema;
using AzureFunctions.Models.Interfaces;

namespace AzureFunctions.Models;
public class ElevatorDeviceItem : IElevator
{
    public ElevatorDeviceItem()
    {
        
    }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public IElevator.ElevatorStatus Status { get; set; } = IElevator.ElevatorStatus.Disabled;
    public bool DoorStatus { get; set; } = false;
    public int CurrentLevel { get; set; } = 0;
    public int TargetLevel { get; set; } = 0;

}