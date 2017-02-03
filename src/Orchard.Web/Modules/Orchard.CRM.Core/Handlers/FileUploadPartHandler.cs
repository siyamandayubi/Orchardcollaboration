using Orchard.Alias;
using Orchard.ContentManagement.Handlers;
using Orchard.CRM.Core.Models;
using Orchard.CRM.Core.Settings;
using Orchard.Data;
using Orchard.MediaLibrary.Services;
using Orchard.Tokens;
using System.Linq;
using System.Web.Routing;

namespace Orchard.CRM.Core.Handlers
{
    public class FileUploadPartHandler : ContentHandler
    {
        private readonly IAliasService _aliasService;
        private readonly ITokenizer _tokenizer;
        private readonly IMediaLibraryService _mediaService;

        public FileUploadPartHandler(IRepository<FileUploadPartRecord> repository,
            IAliasService aliasService,
            ITokenizer tokenizer,
            IMediaLibraryService mediaService)
        {
            _aliasService = aliasService;
            _tokenizer = tokenizer;
            _mediaService = mediaService;
            Filters.Add(StorageFilter.For(repository));
        }
    }
}
