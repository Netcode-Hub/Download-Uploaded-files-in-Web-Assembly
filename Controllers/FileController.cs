using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORMExplained.Server.Data;
using ORMExplained.Server.Models;
using System.Net;

namespace ORMExplained.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IWebHostEnvironment env;
        private readonly AppDbContext appDbContext;

        public FileController(IWebHostEnvironment env, AppDbContext appDbContext)
        {
            this.env = env;
            this.appDbContext = appDbContext;
        }

        [HttpPost]
        public async Task<ActionResult<List<UploadResult>>> UploadFile(List<IFormFile> files)
        {
            List<UploadResult> uploadResults = new();

            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;

                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);

                trustedFileNameForFileStorage = Path.GetRandomFileName();
                var path = Path.Combine(env.ContentRootPath, "UploadsFolder", trustedFileNameForFileStorage);

                await using FileStream fs = new(path, FileMode.Create);
                await file.CopyToAsync(fs);

                uploadResult.StoredFileName = trustedFileNameForFileStorage;
                uploadResult.ContentType = file.ContentType;
                uploadResults.Add(uploadResult);
                appDbContext.Uploads.Add(uploadResult);
            }
            await appDbContext.SaveChangesAsync();
            return Ok(uploadResults);
        }



        [HttpGet("{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var uploadResult = await appDbContext.Uploads.FirstOrDefaultAsync(f => f.StoredFileName.Equals(fileName));

            var path = Path.Combine(env.ContentRootPath, "UploadsFolder", fileName);
            var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
                memory.Position = 0;
                return File(memory, uploadResult.ContentType, Path.GetFileName(path));
            }
        }
    }
}
