using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HidVanguard.Model
{
    public class AllowedProcess : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string DirPath { get; set; }
        public string Hash { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void PopulateHash()
        {
            if (string.IsNullOrEmpty(Name))
                throw new InvalidOperationException("Name must be populated.");

            if (string.IsNullOrEmpty(DirPath))
                throw new InvalidOperationException("Path must be populated.");

            var path = Path.Combine(DirPath, Name);

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            using (var fs = File.OpenRead(path))
            using (var bs = new BufferedStream(fs, 1024 * 1024 * 4))
            {
                var hasher = new SHA256Managed();
                Hash = Convert.ToBase64String(hasher.ComputeHash(bs));
            }
        }

        public bool ValidateHash()
        {
            if (string.IsNullOrEmpty(Name))
                return false;

            if (string.IsNullOrEmpty(DirPath))
                return false;

            var path = Path.Combine(DirPath, Name);

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            if (string.IsNullOrEmpty(Hash))
                return false;

            string currentHash;

            using (var fs = File.OpenRead(path))
            using (var bs = new BufferedStream(fs, 1024 * 1024 * 4))
            {
                var hasher = new SHA256Managed();
                currentHash = Convert.ToBase64String(hasher.ComputeHash(bs));
            }

            return currentHash == Hash;
        }

        public static AllowedProcess FromString(string s)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException("s must be non-empty.");

            var ap = new AllowedProcess();

            var parts = s.Split('|');

            ap.Name = parts[0];

            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                ap.DirPath = parts[1];
            if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
                ap.Hash = parts[2];

            return ap;
        }
    }
}
