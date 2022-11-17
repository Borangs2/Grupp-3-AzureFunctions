using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grupp3_Elevator.Models;

public class ErrandCommentModel
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid Author { get; set; }
    public DateTime PostedAt { get; set; }

}