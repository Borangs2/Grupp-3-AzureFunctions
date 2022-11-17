using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Grupp3_Elevator.Models;

public enum ErrandStatus
{
    NotStarted,
    InProgress,
    Done
}

public class ErrandModel
{

    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public ErrandStatus Status { get; set; }
    public DateTime LastEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }

    public TechnicianModel Technician { get; set; }
    public List<ErrandCommentModel> Comments { get; set; }
}