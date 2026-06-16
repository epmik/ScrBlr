using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Skrbl
{
    public class ResourceSettings
    {
        //public string RootDirectory { get; set; } = string.Empty;
        public string ResourcesDirectoryName { get; set; } = Resources.DefaultResourcesDirectoryName;
        public string FontsDirectoryName { get; set; } = Resources.DefaultFontsDirectoryName;
        public string ShadersDirectoryName { get; set; } = Resources.DefaultShadersDirectoryName;
        public string TexturesDirectoryName { get; set; } = Resources.DefaultTexturesDirectoryName;
        public string ScenesDirectoryName { get; set; } = Resources.DefaultScenesDirectoryName;
        public string SavesDirectoryName { get; set; } = Resources.DefaultSavesDirectoryName;
    }

    public class Resources
    {
        internal const string DefaultResourcesDirectoryName = ".resources";
        internal const string DefaultFontsDirectoryName = ".fonts";
        internal const string DefaultShadersDirectoryName = ".shaders";
        internal const string DefaultTexturesDirectoryName = ".textures";
        internal const string DefaultScenesDirectoryName = ".scenes";
        internal const string DefaultSavesDirectoryName = ".saves";

        protected List<string> RootDirectories { get; set; } = new List<string>();

        protected List<string> ResourcesDirectories { get; set; } = new List<string>();
        protected List<string> FontsDirectories { get; set; } = new List<string>();
        protected List<string> ShadersDirectories { get; set; } = new List<string>();
        protected List<string> TexturesDirectories { get; set; } = new List<string>();
        protected List<string> ScenesDirectories { get; set; } = new List<string>();
        protected List<string> SavesDirectories { get; set; } = new List<string>();

        public Resources() :
            this(string.Empty)
        {
            
        }

        public Resources(string rootDirectory) :
            this([rootDirectory], new ResourceSettings())
        {

        }

        public Resources(string[] rootDirectories) :
            this(rootDirectories, new ResourceSettings())
        {
        }

        private static string RootDirectoryNameInLowerCase = "Skrbl".ToLowerInvariant();

        private static int DirectorySeparatorCharLength = System.IO.Path.DirectorySeparatorChar.ToString().Length;

        public Resources(string[] searchPaths, ResourceSettings settings)
        {
            foreach (var searchPath in searchPaths)
            {
                if (string.IsNullOrEmpty(searchPath))
                {
                    continue;
                }

                FindDirectories(searchPath, settings);
            }
        }

        private void FindDirectories(string searchPath, ResourceSettings settings)
        {
            if (string.IsNullOrEmpty(searchPath) || !System.IO.Path.IsPathFullyQualified(searchPath))
            {
                var executingDirectoryName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (executingDirectoryName == null)
                {
                    throw new ArgumentException($"Resources constructor failed! The executing assembly directory could not be retrieved: {Assembly.GetExecutingAssembly().Location}");
                }

                if(!string.IsNullOrEmpty(searchPath))
                {
                    searchPath = System.IO.Path.Combine(executingDirectoryName, searchPath);
                }
                else
                {
                    searchPath = executingDirectoryName;
                }                    
            }

            searchPath = FormatPath(searchPath);

            FindDirectories(searchPath, settings.ResourcesDirectoryName, ResourcesDirectories);
            FindDirectories(searchPath, settings.FontsDirectoryName, FontsDirectories);
            FindDirectories(searchPath, settings.ShadersDirectoryName, ShadersDirectories);
            FindDirectories(searchPath, settings.TexturesDirectoryName, TexturesDirectories);
            FindDirectories(searchPath, settings.ScenesDirectoryName, ScenesDirectories);
            FindDirectories(searchPath, settings.SavesDirectoryName, SavesDirectories);

            FindDirectories(searchPath, System.IO.Path.Combine(settings.ResourcesDirectoryName, settings.FontsDirectoryName), FontsDirectories);
            FindDirectories(searchPath, System.IO.Path.Combine(settings.ResourcesDirectoryName, settings.ShadersDirectoryName), ShadersDirectories);
            FindDirectories(searchPath, System.IO.Path.Combine(settings.ResourcesDirectoryName, settings.TexturesDirectoryName), TexturesDirectories);
            FindDirectories(searchPath, System.IO.Path.Combine(settings.ResourcesDirectoryName, settings.ScenesDirectoryName), ScenesDirectories);
            FindDirectories(searchPath, System.IO.Path.Combine(settings.ResourcesDirectoryName, settings.SavesDirectoryName), SavesDirectories);
        }

        private static string FormatPath(string path)
        {
            path = path.Trim().ToLowerInvariant().Replace('/', System.IO.Path.DirectorySeparatorChar);

            if (path.EndsWith(System.IO.Path.DirectorySeparatorChar))
            {
                // remove trailing '\\'
                path = path.Substring(0, path.Length - DirectorySeparatorCharLength);
            }

            return path;
        }

        private void FindDirectories(string searchPath, string searchPatern, List<string> foundDirectories)
        {
            while(true)
            {
                var index = searchPath.LastIndexOf(System.IO.Path.DirectorySeparatorChar);

                if(index == -1)
                {
                    break;
                }

                var directory = searchPath.Substring(index + DirectorySeparatorCharLength).ToLowerInvariant();

                var fullSearchPath = System.IO.Path.Combine(searchPath, searchPatern);

                if (Directory.Exists(fullSearchPath) && !foundDirectories.Contains(fullSearchPath))
                {
                    foundDirectories.Add(fullSearchPath);
                }

                if (directory == RootDirectoryNameInLowerCase)
                {
                    break;
                }

                searchPath = System.IO.Path.GetDirectoryName(searchPath) ?? string.Empty;

                if(string.IsNullOrEmpty(searchPath))
                {
                    break;
                }
            }
        }

        public string? Path(string name)
        {
            var path = Path(RootDirectories, name);

            return path == null ? Path(ResourcesDirectories, name) : path;
        }

        private string? Path(IEnumerable<string> paths, string name, bool mustexist = true)
        {
            foreach (var path in paths)
            {
                var fullPath = System.IO.Path.Combine(path, name);

                if(!mustexist)
                {
                    return fullPath; 
                }

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }

        public string? Shader(string name)
        {
            return Path(ShadersDirectories, name);
        }

        public string? Font(string name)
        {
            return Path(FontsDirectories, name);
        }

        public string? Save(string name)
        {
            return Path(SavesDirectories, name, false);
        }

        public string? Texture(string name)
        {
            return Path(TexturesDirectories, name);
        }

        public string? Scene(string name)
        {
            return Path(ScenesDirectories, name);
        }
    }
}