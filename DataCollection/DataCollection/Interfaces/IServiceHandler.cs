using System;
using System.Collections.Generic;
using System.Text;

namespace DataCollection.Interfaces
{
    /// <summary>
    /// interface for starting the background process that
    /// will record accelerometer data
    /// </summary>
    public interface IServiceHandler
    {
        void StartService();

        void StopService();
    }
}
