using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IHI.Server.Events;
using IHI.Server.Habbos;

namespace IHI.Server.Events
{
    public class FuserightEventArgs : HabboEventArgs
    {
        private readonly HashSet<string> _fuserights;

        public FuserightEventArgs(Habbo habbo) : base(habbo)
        {
            _fuserights = new HashSet<string>();
        }

        public ICollection<string> GetFuserights()
        {
            return _fuserights;
        }

        public FuserightEventArgs AddFuseright(string fuseRight)
        {
            _fuserights.Add(fuseRight);
            return this;
        }
    }
}
