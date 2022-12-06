using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using AzureFunctions.Models;
using AzureFunctions.Models.Interfaces;
using Microsoft.Azure.Devices;
using Microsoft.IdentityModel.Tokens;

namespace AzureFunctions.Helpers;

public static class ElevatorHelper
{
    private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");

    /// <summary>
    /// Gets an <see cref="IElevator"/> using <paramref name="elevatorId"/>
    /// </summary>
    /// <param name="elevatorId"></param>
    /// <returns>An <see cref="IElevator"/></returns>
    public static async Task<T> GetElevatorDeviceAsync<T>(string elevatorId) where T : IElevator, new()
    {
        var twin = await _registryManager.GetTwinAsync(elevatorId);

        T elevator = new T();

        elevator.Id = Guid.Parse(twin.DeviceId);

        try { elevator.Name = twin.Properties.Reported["deviceName"]; }
        catch { elevator.Name = "Name unknown"; }

        try { elevator.Status = twin.Properties.Reported["status"]; }
        catch { elevator.Status = IElevator.ElevatorStatus.Disabled; }

        try { elevator.DoorStatus = twin.Properties.Reported["doorStatus"]; }
        catch { elevator.DoorStatus = false; }

        try { elevator.CurrentLevel = twin.Properties.Reported["currentLevel"]; }
        catch { elevator.CurrentLevel = 0; }

        try { elevator.TargetLevel = twin.Properties.Reported["targetLevel"]; }
        catch { elevator.TargetLevel = 0; }

        return elevator;
    }

    
}