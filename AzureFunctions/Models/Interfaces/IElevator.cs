using System;

namespace AzureFunctions.Models.Interfaces;

public interface IElevator
{
    public enum ElevatorStatus
    {
        Disabled /*Elevator off, doors closed*/,
        Idle /*Elevator on, doors closed, not running*/,
        Running /*Elevator on, doors closed, running*/,
        Error /*Elevator error*/
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ElevatorStatus Status { get; set; }
    public bool DoorStatus { get; set; }
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
}