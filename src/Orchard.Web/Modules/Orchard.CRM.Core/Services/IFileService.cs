namespace Orchard.CRM.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public interface IFileService : IDependency
    {
        bool AddFile(string fileName, Stream stream, string contentTypeForFile, Guid guid, Dictionary<string, string> errors);
        bool AddFile(string fileName, Stream stream, string contentTypeForFile, Guid guid, Dictionary<string, string> errors, out int filesCount);
    }
}