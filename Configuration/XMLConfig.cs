#region Usings

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

#endregion

namespace IHI.Server.Configuration
{
    public class XmlConfig
    {
        private readonly XDocument _document;
        private readonly string _path;
        
        /// <summary>
        ///   Loads an Xml config file into memory.
        /// </summary>
        /// <param name = "path">The path to the Xml config file.</param>
        internal XmlConfig(string path)
        {
            new FileInfo(path).EnsureExists();
            
            try
            {
                _document = XDocument.Load(path);
            }
            catch (XmlException)
            {
                _document = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("config"));
                _document.Save(path);
            }

            _path = path;
        }

        /// <summary>
        ///   Save changes to the configuration file.
        /// </summary>
        private void Save()
        {
            _document.Save(_path);
        }

        public ConfigValue<T> GetValue<T>(string path, T fallback)
        {
            string[] pathParts = path.Split(new[] { ':' });

            XElement recentElement = _document.Element("config");
            foreach (string pathPart in pathParts)
            {
                recentElement = recentElement.Element(pathPart);
                if (recentElement == null)
                    break;
            }

            if (recentElement == null)
            {
                return new ConfigValue<T>(path, fallback, false);
            }

            string stringValue = recentElement.Value;
            T value;
            if (typeof(T) == typeof(string))
            {
                value = (T)(stringValue as object);
                return new ConfigValue<T>(path, value, false);
            }
            value = (T)(TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(stringValue));
            return new ConfigValue<T>(path, value, false);
        }
        public bool HasValue(string path)
        {
            string[] pathParts = path.Split(new[] { ':' });

            XElement recentElement = _document.Element("config");
            foreach (string pathPart in pathParts)
            {
                recentElement = recentElement.Element(pathPart);
                if (recentElement == null)
                    return false;
            }
            return true;
        }

        public bool TryParseValue<T>(string stringValue, out T value)
        {
            try
            {
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
                value = (T)tc.ConvertFromInvariantString(null, stringValue);
                return true;
            }
            catch (Exception)
            {
                try
                {
                    value = (T)typeof(T).GetMethod("Parse").Invoke(null, new object[] { stringValue });
                    return true;
                }
                catch (Exception)
                {
                    value = default(T);
                    return false;
                }
            }
        }

        /// <returns>Returns true if the node doesn't already exist or overwrite is enabled, otherwise false is returned.</returns>
        public bool SetValue<T>(string path, T value, bool overwrite = false)
        {
            string[] pathParts = path.Split(new[] { ':' });

            bool created = false;
            XElement recentElement = _document.Element("config");
            foreach (string pathPart in pathParts)
            {
                XElement nextElement = recentElement.Element(pathPart);
                if (nextElement == null)
                {
                    nextElement = new XElement(pathPart);
                    recentElement.Add(nextElement);
                    created = true;
                }
                recentElement = nextElement;
            }

            if (created && !overwrite)
                return false;
            recentElement.SetValue(value);
            Save();
            return true;
        }
    }
}