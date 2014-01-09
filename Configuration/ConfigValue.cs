using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace IHI.Server.Configuration
{
    public struct ConfigValue<T>
    {
        public string Path
        {
            get;
            private set;
        }

        public T Value
        {
            get;
            private set;
        }

        public bool Exists
        {
            get;
            private set;
        }

        internal ConfigValue(string path, T value, bool exists) : this()
        {
            Path = path;
            Value = value;
            Exists = exists;
        }
    }
}
