namespace ViewModels.Interfaces;

public interface IConfirmed
{
    bool Confirm(string message, string caption = "");
}