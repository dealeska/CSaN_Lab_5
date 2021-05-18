using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.StaticFiles;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace FileStorage.Controllers
{
    [Route("FileStorage")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<StorageController> _logger;
        private readonly string _path = @"C:\Storage";

        private const string CopyHeader = "X-Copy-From";        
         
        public StorageController(ILogger<StorageController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{*path}")]
        public ActionResult GetFile(string path)
        {
            string fullPath = FileStorageService.GetFullPath(path);
            
            if (Directory.Exists(fullPath))
            {
                try
                {
                    List<string> content = FileStorageService.GetDirectoryInfo(fullPath);                  
                    return new JsonResult(content);
                }
                catch
                {
                    return StatusCode(500);
                }

            }
            else if (System.IO.File.Exists(fullPath))
            {
                try
                {                    
                    FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                    return File(fileStream, FileStorageService.GetMimeType(fullPath), Path.GetFileName(fullPath));
                }
                catch
                {
                    return StatusCode(500);
                }

            }

            return NotFound();
        }


        [HttpDelete("{*fileName}")]
        public ActionResult DeleteFile(string fileName)
        {
            string fullPath;

            if (fileName == null)
            {                 
                return BadRequest();
            }
            else
            {
                fullPath = FileStorageService.GetFullPath(fileName);
            }

            try
            {
                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    return Ok();
                }
                else if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    return Ok();
                }
            }
            catch
            {
                return StatusCode(500);
            }

            return NotFound();
        }


        [HttpHead("{*fileName}")]
        public ActionResult GetFileHeader(string fileName)
        {
            string fullPath = FileStorageService.GetFullPath(fileName);

            try
            {                
                FileInfo fileInfo = new FileInfo(fullPath);
                if (fileInfo.Exists)
                {
                    Response.Headers.Add("Name", fileInfo.Name);
                    Response.Headers.Add("Path", fileInfo.DirectoryName);
                    Response.Headers.Add("Content-Type", FileStorageService.GetMimeType(fullPath));
                    Response.Headers.Add("Size", fileInfo.Length.ToString());
                    Response.Headers.Add("Creation-date", fileInfo.CreationTime.ToString());
                    Response.Headers.Add("Changed-date", fileInfo.LastWriteTime.ToString());
                    return Ok();
                }
            }
            catch
            {
                return StatusCode(500);
            }

            return NotFound();
        }
        

        [HttpPut("{*path}")]
        public ActionResult PutFile(string path)
        {
            string fullPath = FileStorageService.GetFullPath(path);

            IFormFileCollection formFiles;
            try
            {
                formFiles = Request.Form.Files;
            }
            catch
            {
                formFiles = null;
            }

            try
            {                
                if (Request.Headers.ContainsKey(CopyHeader))
                {                    
                    int code = FileStorageService.CopyFile(Request.Headers[CopyHeader], fullPath);

                    if (code == FileStorageService.ok)
                    {
                        return Ok();
                    }
                    else if (code == FileStorageService.bad)
                    {
                        return BadRequest();
                    }
                    else
                    {
                        return NotFound();
                    }
                }                
                else if (Directory.Exists(fullPath))
                {
                    var file = formFiles[0];
                    using (FileStream fs = new FileStream(Path.Combine(fullPath, file.FileName), FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }

                    return Ok();
                }
            }
            catch
            {
                return StatusCode(500);
            }

            return NotFound();
        }
    }
}
