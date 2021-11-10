using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IRequester
    {
        byte[] Request(string url);
    }
}
