using FileShare.Business.Models.Abstraction;

namespace FileShare.Business.Models;

public class MessageModel:IModel
{
    public string? Message { get; set; }
    public string Endpoint { get; set; }
}