using System;
using System.Threading.Tasks;

namespace ChatiqueWebGateway
{
    interface IAuth
    {
        void Go();
        Task GoAsync();
    }
}
