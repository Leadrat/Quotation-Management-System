using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Api.Controllers.Imports;
using CRM.Application.Common.Persistence;
using CRM.Application.Imports.Dtos;
using CRM.Application.Imports.Services;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Services;
using CRM.Infrastructure.Services.Imports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CRM.Tests.Integration.Imports
{
    public class ImportTemplatesFlowTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private static (ImportTemplatesController, ImportChatController) NewControllers(AppDbContext db)
        {
            var fileStorage = new FileStorageService(NullLogger<FileStorageService>.Instance);
            var storage = new ImportStorageService(fileStorage);
            var parser = new ParseService();
            var mapping = new MappingService();

            var importsCtrl = new ImportTemplatesController(db, storage, parser);

            // GeminiClient is not used in this flow; we can pass a dummy HttpClient but not call chat
            var gemini = new CRM.Application.Imports.LLM.GeminiClient(new System.Net.Http.HttpClient());
            var chatCtrl = new ImportChatController(db as IAppDbContext, gemini, mapping);
            return (importsCtrl, chatCtrl);
        }

        private static IFormFile FakeFormFile(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, name: "file", fileName: fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };
        }

        [Fact]
        public async Task Import_EndToEnd_Flow_Succeeds()
        {
            using var db = NewDb();
            var (imports, chat) = NewControllers(db);
            var ct = CancellationToken.None;

            // 1) Upload and create session
            var formFile = FakeFormFile("sample.docx", "dummy content");
            var created = await imports.Create(formFile, ct);
            var createdResult = Assert.IsType<CreatedResult>(created);
            var sessionDto = Assert.IsType<ImportSessionDto>(createdResult.Value);
            Assert.Equal("Parsed", sessionDto.Status);

            // 2) Save mappings (required fields)
            var mappings = new SaveMappingsRequest
            {
                Mappings = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["company"] = new { name = "MyCo" },
                    ["customer"] = new { name = "ACME" }
                }
            };
            var saveMappings = await chat.SaveMappings(sessionDto.ImportSessionId, mappings, ct);
            var saveOk = Assert.IsType<OkObjectResult>(saveMappings);

            // 3) Generate preview
            var gen = await imports.Generate(sessionDto.ImportSessionId, ct);
            var genOk = Assert.IsType<OkObjectResult>(gen);
            var genObj = genOk.Value as System.Collections.Generic.IDictionary<string, object>;
            Assert.NotNull(genObj);
            Assert.True(genObj!.ContainsKey("previewPath"));

            // 4) Preview exists and returns file
            var preview = await imports.Preview(sessionDto.ImportSessionId, ct);
            var fileResult = Assert.IsType<FileContentResult>(preview);
            Assert.Equal("text/plain", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);

            // 5) Save template
            var saved = await imports.SaveTemplate(sessionDto.ImportSessionId, new ImportTemplatesController.SaveTemplateBody("Test Template", "generic"), ct);
            var savedCreated = Assert.IsType<CreatedResult>(saved);
            var savedObj = savedCreated.Value as System.Collections.Generic.IDictionary<string, object>;
            Assert.NotNull(savedObj);
            Assert.True(savedObj!.ContainsKey("templateId"));
        }
    }
}
