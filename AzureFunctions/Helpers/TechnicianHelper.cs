using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AzureFunctions.Models;
using Dapper;

namespace AzureFunctions.Helpers;

public static class TechnicianHelper
{
    public static async Task<TechnicianModel> GetTechnicianAsync(string errandId, IDbConnection connection)
    {
        var technicians = await connection.QueryAsync(
            "SELECT Technicians.Id AS 'TechnicianId', Technicians.Name, Errands.Id AS 'ErrandId' FROM Technicians " +
            "INNER JOIN Errands ON Technicians.Id = Errands.TechnicianId");

        var technicianResult = technicians.FirstOrDefault(technician => technician.ErrandId.ToString() == errandId);
        var technician = new TechnicianModel();
        if (technicianResult == null)
            technician = null;
        else
            technician = new TechnicianModel(technicianResult.TechnicianId, technicianResult.Name);
        return technician;
    }
}