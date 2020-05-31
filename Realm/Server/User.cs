using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Realm.Server
{
    class User
    {
        public string Id { get; set; }
        public IPEndPoint FullInfoIP { get; set; }
        public string Name { get; set; }
    }
}
