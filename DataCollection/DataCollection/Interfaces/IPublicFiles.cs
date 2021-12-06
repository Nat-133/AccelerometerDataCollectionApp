using System;
using System.Collections.Generic;
using System.Text;

namespace DataCollection.Services
{
    public interface IPublicFiles
    {
        void SaveData(bool drunk, List<float[]> data);
    }
}
