using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using ScaleUP.Services.MediaAPI.Constants;
using ScaleUP.Services.MediaAPI.Helper;
using ScaleUP.Services.MediaAPI.Manager;
using ScaleUP.Services.MediaAPI.Persistence;
using ScaleUP.Services.MediaDTO.Transaction;
using ScaleUP.Services.MediaModels.Transaction;
using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ScaleUP.Services.MediaAPI.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class DocController : ControllerBase
    {
        private IHostEnvironment _hostingEnvironment;

        private readonly ApplicationDbContext _context;
        private readonly MediaGrpcManager _mediaGrpcManager;
        private readonly static Serilog.ILogger logger = Log.ForContext<DocController>();
        //private readonly IPublisher _publisher;
        public DocController(ApplicationDbContext context, IHostEnvironment hostingEnvironment, MediaGrpcManager mediaGrpcManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _mediaGrpcManager = mediaGrpcManager;
        }



        [HttpPost("PostSingleFile")]
        public async Task<ActionResult<FileUploadReply>> PostSingleFile([FromForm] FileUploadDTO fileDetails)
        {
            //if (string.IsNullOrEmpty(EnvironmentConstants.EnvironmentName) || EnvironmentConstants.EnvironmentName.ToLower() == "development")
            //{
            return await PostSingleFileLocal(fileDetails);
            //}
            //else
            //{
            //    return await PostSingleFileAzure(fileDetails);
            //}
        }

        [HttpPost("PostMultipleFile")]
        public async Task<ActionResult<FileUploadReplyDc>> PostMultipleFile([FromForm] MultiFileUploadDTO fileDetails)
        {
            //if (string.IsNullOrEmpty(EnvironmentConstants.EnvironmentName) || EnvironmentConstants.EnvironmentName.ToLower() == "development")
            //{
            return await PostMultiFileLocal(fileDetails);
            //}
            //else
            //{
            //    return await PostMultFileAzure(fileDetails);
            //}
        }


        [HttpPost("PostLocalFile")]
        public async Task<ActionResult<FileUploadReply>> PostLocalFile([FromForm] FileUploadDTO fileDetails)
        {
            return await PostSingleFileLocal(fileDetails);
        }



        [HttpPost("GetFilePath")]
        public async Task<ImagePath> GetFilePath(int id)
        {
            return await _mediaGrpcManager.GetFilePathPrivate(id);
        }



        public class ImagePath
        {
            public string Path { get; set; }
        }



        private async Task<ActionResult<FileUploadReply>> PostSingleFileLocal(FileUploadDTO fileDetails)
        {
            FileUploadReply fileUploadReply = new FileUploadReply();
            fileUploadReply.Message = "File not uploaded.";
            if (fileDetails == null)
            {
                return BadRequest();
            }

            string FileNameGuid = Guid.NewGuid().ToString();

            string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : fileDetails.SubFolderName);
            string RealPath = "/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/");
            string DetailFilePath = $"{EnvironmentConstants.MediaServiceLocalBaseURL}{RealPath}{FileNameGuid}.{fileDetails.FileDetails.FileName.Split('.').ToList().Last()}";


            DocInfo docInfo = new DocInfo
            {
                IsActive = true,
                IsDeleted = false,
                IsValidForLifeTime = fileDetails.IsValidForLifeTime,
                FileExtension = fileDetails.FileDetails.FileName.Split('.').ToList().Last(),
                Name = FileNameGuid,
                RelativePath = DetailFilePath, //"/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/"),
                ValidityInDays = fileDetails.IsValidForLifeTime ? null : fileDetails.ValidityInDays,
            };

            if (!Directory.Exists(uploadFilePath))
            {
                Directory.CreateDirectory(uploadFilePath);
            }

            string filePath = Path.Combine(uploadFilePath, docInfo.Name + "." + docInfo.FileExtension);
            using (Stream fileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileDetails.FileDetails.CopyToAsync(fileStream);
            }

            _context.DocInfos.Add(docInfo);
            await _context.SaveChangesAsync();

            if (docInfo.Id > 0)
            {
                //var imageObj = await _mediaGrpcManager.GetFilePathPrivateLocal((int)docInfo.Id);
                fileUploadReply.Message = "File uploaded successfully.";
                fileUploadReply.status = true;
                fileUploadReply.DocId = docInfo.Id;
                fileUploadReply.FilePath = $"{EnvironmentConstants.MediaServiceLocalBaseURL}{docInfo.RelativePath}{docInfo.Name}.{docInfo.FileExtension}";
            }

            return Ok(fileUploadReply);

        }

        private async Task<ActionResult<FileUploadReplyDc>> PostMultiFileLocal(MultiFileUploadDTO fileDetails)
        {
            FileUploadReplyDc fileUploadReply = new FileUploadReplyDc
            {
                Message = "File not uploaded.",
                DocList = new List<DocDc>()
            };
            if (fileDetails == null || fileDetails.FileDetails.Count == 0)
            {
                return BadRequest();
            }

            string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : fileDetails.SubFolderName);
            string RealPath = "/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/");

            foreach (var fileDetail in fileDetails.FileDetails)
            {
                string FileNameGuid = Guid.NewGuid().ToString();

                string DetailFilePath = $"{EnvironmentConstants.MediaServiceLocalBaseURL}{RealPath}{FileNameGuid}.{fileDetail.FileName.Split('.').ToList().Last()}";


                DocInfo docInfo = new DocInfo
                {
                    IsActive = true,
                    IsDeleted = false,
                    IsValidForLifeTime = fileDetails.IsValidForLifeTime,
                    FileExtension = fileDetail.FileName.Split('.').ToList().Last(),
                    Name = FileNameGuid,
                    RelativePath = DetailFilePath, //"/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/"),
                    ValidityInDays = fileDetails.IsValidForLifeTime ? null : fileDetails.ValidityInDays,
                };

                if (!Directory.Exists(uploadFilePath))
                {
                    Directory.CreateDirectory(uploadFilePath);
                }

                string filePath = Path.Combine(uploadFilePath, docInfo.Name + "." + docInfo.FileExtension);
                using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await fileDetail.CopyToAsync(fileStream);
                }
                _context.DocInfos.Add(docInfo);

                await _context.SaveChangesAsync();

                if (docInfo.Id > 0)
                {
                    fileUploadReply.DocList.Add(new DocDc
                    {
                        DocId = docInfo.Id.ToString(),
                        FilePath = $"{docInfo.RelativePath}{docInfo.Name}.{docInfo.FileExtension}"
                    });
                }
            }

            if (fileUploadReply.DocList.Count > 0)
            {
                fileUploadReply.Message = "File uploaded successfully.";
                fileUploadReply.status = true;
            }
            return Ok(fileUploadReply);

        }
        private async Task<ActionResult<FileUploadReply>> PostSingleFileAzure(FileUploadDTO fileDetails)
        {
            FileUploadReply fileUploadReply = new FileUploadReply();
            fileUploadReply.Message = "File not uploaded.";
            if (fileDetails == null)
            {
                return BadRequest();
            }

            try
            {
                var filestream = fileDetails.FileDetails.OpenReadStream() as FileStream;
                string fileName = Guid.NewGuid().ToString();
                string fileExt = fileDetails.FileDetails.FileName.Split('.').ToList().Last();
                string filefullName = fileName + "." + fileExt;
                string filePath = "";
                using (var ms = new MemoryStream())
                {
                    await fileDetails.FileDetails.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    filePath = await AzureBlobService.UploadFile(ms, filefullName);
                }
                //string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : fileDetails.SubFolderName);

                //logger.Information(uploadFilePath);

                DocInfo docInfo = new DocInfo
                {
                    IsActive = true,
                    IsDeleted = false,
                    IsValidForLifeTime = fileDetails.IsValidForLifeTime,
                    FileExtension = fileExt,
                    Name = fileName,
                    RelativePath = filePath,//"/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/"),
                    ValidityInDays = fileDetails.IsValidForLifeTime ? null : fileDetails.ValidityInDays,
                };

                //if (!Directory.Exists(uploadFilePath))
                //{
                //    Directory.CreateDirectory(uploadFilePath);
                //}

                //string filePath = Path.Combine(uploadFilePath, docInfo.Name + "." + docInfo.FileExtension);
                //using (Stream fileStream = new FileStream(filePath, FileMode.Create))
                //{
                //    await fileDetails.FileDetails.CopyToAsync(fileStream);
                //}

                _context.DocInfos.Add(docInfo);
                await _context.SaveChangesAsync();

                if (docInfo.Id > 0)
                {
                    //var imageObj = await _mediaGrpcManager.GetFilePathPrivate((int)docInfo.Id);
                    fileUploadReply.Message = "File uploaded successfully.";
                    fileUploadReply.status = true;
                    fileUploadReply.DocId = docInfo.Id;
                    fileUploadReply.FilePath = docInfo.RelativePath;
                }

                //await _uploadService.PostFileAsync(fileDetails.FileDetails, fileDetails.FileType);
                return Ok(fileUploadReply);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<ActionResult<FileUploadReplyDc>> PostMultFileAzure(MultiFileUploadDTO fileDetails)
        {
            FileUploadReplyDc fileUploadReply = new FileUploadReplyDc
            {
                Message = "File not uploaded.",
                DocList = new List<DocDc>()
            };
            if (fileDetails == null)
            {
                return BadRequest();
            }

            try
            {
                foreach (var fileDetail in fileDetails.FileDetails)
                {
                    var filestream = fileDetail.OpenReadStream() as FileStream;
                    string fileName = Guid.NewGuid().ToString();
                    string fileExt = fileDetail.FileName.Split('.').ToList().Last();
                    string filefullName = fileName + "." + fileExt;
                    string filePath = "";
                    using (var ms = new MemoryStream())
                    {
                        await fileDetail.CopyToAsync(ms);
                        ms.Seek(0, SeekOrigin.Begin);
                        filePath = await AzureBlobService.UploadFile(ms, filefullName);
                    }
                    //string uploadFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Uploads", fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary", string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : fileDetails.SubFolderName);

                    //logger.Information(uploadFilePath);

                    DocInfo docInfo = new DocInfo
                    {
                        IsActive = true,
                        IsDeleted = false,
                        IsValidForLifeTime = fileDetails.IsValidForLifeTime,
                        FileExtension = fileExt,
                        Name = fileName,
                        RelativePath = filePath,//"/Uploads/" + (fileDetails.IsValidForLifeTime ? "LifeTime" : "Ordinary") + "/" + (string.IsNullOrEmpty(fileDetails.SubFolderName) ? "" : $"{fileDetails.SubFolderName}/"),
                        ValidityInDays = fileDetails.IsValidForLifeTime ? null : fileDetails.ValidityInDays,
                    };

                    _context.DocInfos.Add(docInfo);
                    await _context.SaveChangesAsync();

                    if (docInfo.Id > 0)
                    {
                        fileUploadReply.DocList.Add(new DocDc
                        {
                            DocId = docInfo.Id.ToString(),
                            FilePath = $"{docInfo.RelativePath}"
                        });
                    }
                }
                fileUploadReply.Message = "File uploaded successfully.";
                fileUploadReply.status = true;
                return Ok(fileUploadReply);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }



    public abstract class WkhtmlDrivernew
    {
        public static byte[] Convert(string wkhtmlPath, string switches, string html)
        {
            string text = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(wkhtmlPath, "Windows", "wkhtmltopdf.exe") : ((!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) ? Path.Combine(wkhtmlPath, "Linux", "wkhtmltopdf") : Path.Combine(wkhtmlPath, "Mac", "wkhtmltopdf")));
            if (!File.Exists(text))
            {
                throw new Exception("wkhtmltopdf not found, searched for " + text);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switches = "-q " + switches + " -";
                if (!string.IsNullOrEmpty(html))
                {
                    switches += " -";
                    html = SpecialCharsEncode(html);
                }

                using Process process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = text,
                    Arguments = switches,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                };
                process.Start();
                if (!string.IsNullOrEmpty(html))
                {
                    using StreamWriter streamWriter = process.StandardInput;
                    streamWriter.WriteLine(html);
                }

                using MemoryStream memoryStream = new MemoryStream();
                using (Stream stream = process.StandardOutput.BaseStream)
                {
                    byte[] array = new byte[4096];
                    int count;
                    while ((count = stream.Read(array, 0, array.Length)) > 0)
                    {
                        memoryStream.Write(array, 0, count);
                    }
                }

                string message = process.StandardError.ReadToEnd();
                if (memoryStream.Length == 0L)
                {
                    throw new Exception(message);
                }

                process.WaitForExit();
                return memoryStream.ToArray();
            }

            switches = "-q " + switches;
            if (!string.IsNullOrEmpty(html))
            {
                html = SpecialCharsEncode(html);
            }

            Guid guid = Guid.NewGuid();
            File.WriteAllText($"{guid}.html", html);
            switches += $" {guid}.html {guid}.pdf";
            using Process process2 = new Process();
            process2.StartInfo = new ProcessStartInfo
            {
                FileName = text,
                Arguments = switches,
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            process2.Start();
            using MemoryStream memoryStream2 = new MemoryStream();
            string message2 = process2.StandardError.ReadToEnd();
            process2.WaitForExit();
            using (FileStream fileStream = new FileStream($"{guid}.pdf", FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream2);
            }

            File.Delete($"{guid}.pdf");
            File.Delete($"{guid}.html");
            if (memoryStream2.Length == 0L)
            {
                throw new Exception(message2);
            }

            return memoryStream2.ToArray();
        }

        private static string SpecialCharsEncode(string text)
        {
            char[] array = text.ToCharArray();
            StringBuilder stringBuilder = new StringBuilder(text.Length + (int)((double)text.Length * 0.1));
            char[] array2 = array;
            foreach (char value in array2)
            {
                int num = System.Convert.ToInt32(value);
                if (num > 127)
                {
                    stringBuilder.AppendFormat("&#{0};", num);
                }
                else
                {
                    stringBuilder.Append(value);
                }
            }

            return stringBuilder.ToString();
        }
    }

}
