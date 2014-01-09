using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IHI.Server
{
    public static class Extensions
    {
        public static string ToUtf8String(this byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }
        public static byte[] ToUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        private static readonly sbyte[] _hexLookup = new sbyte[]{0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46};
        public static string ToHexString(this byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 2);

            foreach (byte t in data)
            {
                sb.Append(_hexLookup[(t >> 4) & 0x0F]);
                sb.Append(_hexLookup[t & 0x0F]);
            }

            return sb.ToString();
        }

        private static readonly DateTime _unixEpoche = new DateTime(1970, 1, 1, 0, 0, 0);
        public static int GetUnixTimestamp(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(_unixEpoche).TotalSeconds;
        }

        /// <summary>
        ///   Creates a file if it doesn't exist, creating all non-existing parent directories in the process.
        /// </summary>
        /// <param name = "file">The file to create.</param>
        /// <returns>True if the file was created or already existed, false otherwise.</returns>
        public static bool EnsureExists(this FileInfo file)
        {
            if (file.Exists) // Does the file already exist?
                return true; // Yes, nothing needed.

            if (!file.Directory.EnsureExists())
                return false; // Something went wrong, return false.
            
            file.Create().Close(); // All missing parent directories created, create the file.
            return true;
        }

        /// <summary>
        ///   Creates a directory if it doesn't exist, creating all non-existing parent directories in the process.
        /// </summary>
        /// <param name = "directory">The directory to create.</param>
        /// <returns>True if the directory was created or already exists, false otherwise.</returns>
        public static bool EnsureExists(this DirectoryInfo directory)
        {
            if (!directory.Exists)
                directory.Create();

            return true;
        }
    }
}
