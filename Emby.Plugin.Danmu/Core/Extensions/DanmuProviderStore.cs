using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Emby.Plugin.Danmu.Core.Extensions
{
    // Keep download identifiers outside Emby's metadata/NFO save pipeline.
    public static class DanmuProviderStore
    {
        private static readonly object Sync = new object();
        private static string directory;

        public static void Initialize(string path)
        {
            lock (Sync)
            {
                Directory.CreateDirectory(path);
                directory = path;
            }
        }

        public static string Get(Guid itemId, string providerId)
        {
            lock (Sync)
            {
                if (directory == null || itemId == Guid.Empty) return null;
                var path = Path.Combine(directory, itemId.ToString("N") + ".xml");
                if (!File.Exists(path)) return null;
                return XDocument.Load(path).Root.Elements("provider")
                    .FirstOrDefault(x => (string)x.Attribute("id") == providerId)?.Value;
            }
        }

        public static void Set(Guid itemId, string providerId, string value)
        {
            if (itemId == Guid.Empty || string.IsNullOrEmpty(providerId) || string.IsNullOrEmpty(value))
                throw new ArgumentException("A persistent item and non-empty provider ID are required.");
            lock (Sync)
            {
                if (directory == null) throw new InvalidOperationException("Danmu provider cache is not initialized.");
                var path = Path.Combine(directory, itemId.ToString("N") + ".xml");
                var doc = File.Exists(path) ? XDocument.Load(path) : new XDocument(new XElement("providers"));
                var node = doc.Root.Elements("provider").FirstOrDefault(x => (string)x.Attribute("id") == providerId);
                if (node == null)
                {
                    node = new XElement("provider", new XAttribute("id", providerId));
                    doc.Root.Add(node);
                }
                node.Value = value;
                var temporary = path + ".tmp";
                try
                {
                    doc.Save(temporary);
                    if (File.Exists(path)) File.Replace(temporary, path, null);
                    else File.Move(temporary, path);
                }
                finally
                {
                    if (File.Exists(temporary)) File.Delete(temporary);
                }
            }
        }
    }
}
