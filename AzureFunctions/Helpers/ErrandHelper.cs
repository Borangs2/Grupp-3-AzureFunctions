using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureFunctions.Models;
using AzureFunctions.Models.Interfaces;
using Dapper;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Amqp.Serialization;

namespace AzureFunctions.Helpers;

public static class ErrandHelper
{
    public static async Task<List<ErrandModel>> GetElevatorErrands(string elevatorId, IDbConnection connection)
    {
        var elevatorErrands = new List<ErrandModel>();

        //Gets all errand properties
        var errandResult = await connection.QueryAsync("SELECT * FROM Errands WHERE ElevatorModelId = @ElevatorId", new { ElevatorId = elevatorId });
        foreach (var errand in errandResult.ToList())
        {
            var addErrand = new ErrandModel()
            {
                Id = errand.Id,
                Title = errand.Title,
                Description = errand.Description,
                CreatedAt = errand.CreatedAt,
                CreatedBy = errand.CreatedBy,
                LastEdited = errand.LastEdited,
                Status = Enum.Parse<ErrandStatus>(errand.Status),
                Comments = new List<ErrandCommentModel>()
            };

            //Gets the technician
            addErrand.Technician = await TechnicianHelper.GetErrandTechnicianAsync(errand.Id.ToString(), connection);

            //Gets all comments
            errand.Comments = await CommentHelper.GetErrandCommentsAsync(errand.Id.ToString(), connection);

            elevatorErrands.Add(addErrand);
        }
        return elevatorErrands;
    }
}