using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using AzureFunctions.Models;
using Dapper;

namespace AzureFunctions.Helpers;

public static class CommentHelper
{
    public static async Task<List<ErrandCommentModel>> GetErrandCommentsAsync(string errandId, IDbConnection connection)
    {
        var comments = await connection.QueryAsync(
            "SELECT ErrandComments.Id AS 'CommentId', ErrandComments.Content,ErrandComments.PostedAt,ErrandComments.Author,Errands.Id AS 'ErrandId' FROM ErrandComments " +
            "INNER JOIN Errands ON ErrandComments.ErrandModelId = Errands.Id WHERE ErrandComments.ErrandModelId = @ErrandId", new { ErrandId = errandId });

        var errandComments = new List<ErrandCommentModel>();
        foreach (var comment in comments)
        {
            var addComment = new ErrandCommentModel(
                comment.CommentId,
                comment.Content,
                comment.Author,
                comment.PostedAt);
            errandComments.Add(addComment);
        }

        return errandComments;
    }
}