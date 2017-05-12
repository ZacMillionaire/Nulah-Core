using Microsoft.Extensions.FileProviders;
using NulahCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NulahCore {
    public static class ResourceLoader {

        public static string LoadEmbeddedResource(Type ExecutingAssembly, string ResourceName) {
            string resource = string.Empty;

            // find the resource from the name
            string resourceFullyQualified = ExecutingAssembly.GetTypeInfo().Assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(ResourceName));
            if(resourceFullyQualified == null) {
                return resource;
            }

            using(Stream stream = ExecutingAssembly.GetTypeInfo().Assembly.GetManifestResourceStream(resourceFullyQualified)) {
                if(stream != null) {
                    resource = new StreamReader(stream).ReadToEnd();
                }
            }
            return resource;
        }

        public static string LoadContentResource(string FilePath, AppSetting Settings) {

            string resource = string.Empty;

            var a = new PhysicalFileProvider(Settings.ContentRoot);
            var File = a.GetFileInfo(FilePath);

            if(File.Exists) {
                resource = new StreamReader(File.CreateReadStream()).ReadToEnd();
            }
            return resource;
        }
    }
}
