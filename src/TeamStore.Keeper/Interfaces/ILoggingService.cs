using System;
using System.Collections.Generic;
using System.Text;

namespace TeamStore.Keeper.Interfaces
{
    public interface ILoggingService
    {
        void Information(string message);
        void Warning(string message);
        void Error(string message);
    }
}
