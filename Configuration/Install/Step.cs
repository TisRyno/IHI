#region Usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace IHI.Server.Install
{
    public class Step
    {
        #region Property: Path
        /// <summary>
        /// 
        /// </summary>
        public string Path
        {
            get;
            private set;
        }
        #endregion

        #region Property: Type
        /// <summary>
        /// 
        /// </summary>
        public Type Type
        {
            get;
            private set;
        }
        #endregion

        #region Property: Title
        /// <summary>
        /// 
        /// </summary>
        public string Title
        {
            get;
            set;
        }
        #endregion

        #region Property: Description
        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get;
            set;
        }
        #endregion

        #region Property: Default
        /// <summary>
        /// 
        /// </summary>
        public string Default
        {
            get;
            set;
        }
        #endregion

        private List<string> _examples;
        public Step AddExample(string example)
        {
            _examples.Add(example);
            return this;
        }
        public Step RemoveExample(string example)
        {
            _examples.Remove(example);
            return this;
        }

        public IEnumerable<string> Examples
        {
            get
            {
                return _examples.ToArray();
            }
        }

        #region Method: Step (Constructor)
        public Step(string path, Type type, string title, string description, string @default = "")
        {
            Path = path;
            Type = type;
            Title = title;
            Description = description;
            Default = @default;

            _examples = new List<string>();
        }
        #endregion
    }
}