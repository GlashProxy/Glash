using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glash.Server
{
    public interface IAgentManager
    {
        bool Login(LoginInfo loginInfo);
    }
}
