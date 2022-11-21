using System;

namespace Grupp3_Elevator.Models;

public class CreateErrandCommentModel
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public Guid? Author { get; set; }
    public DateTime PostedAt { get; set; }
    public Guid ErrandId { get; set; }

    public CreateErrandCommentModel(Guid id, string content, string author, DateTime postedAt, Guid errandId)
    {
        Id = id;
        Content = content;
        if (Guid.TryParse(author, out Guid authorGuid))
            Author = authorGuid;
        else
            Author = null;
        PostedAt = postedAt;
        ErrandId = errandId;
    }
}