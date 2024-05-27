namespace FileShare.Business.Abstraction;

public interface IConnectionManager
{
    Task ConnectAsServerAsync();
}