using System;

namespace Launcher.Interfaces
{
    public interface IGetPassword
    {
        string GetPassword();
    }

    public interface IPassword : IGetPassword
    {
        void SetPassword(string pass);
        event EventHandler PassChanged;
    } 
}
