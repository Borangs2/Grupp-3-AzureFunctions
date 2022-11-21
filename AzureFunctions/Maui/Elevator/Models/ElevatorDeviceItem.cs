using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grupp3_Elevator.Models;
public class ElevatorDeviceItem
{
    public enum ElevatorStatus
    {
        Disabled /*Elevator off, doors closed*/,
        Idle /*Elevator on, doors closed, not running*/,
        Running /*Elevator on, doors closed, running*/,
        Error /*Elevator error*/
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public ElevatorStatus Status { get; set; } = ElevatorStatus.Disabled;
    public bool DoorStatus { get; set; } = false;
    public int CurrentLevel { get; set; } = 0;
    public int TargetLevel { get; set; } = 0;

}