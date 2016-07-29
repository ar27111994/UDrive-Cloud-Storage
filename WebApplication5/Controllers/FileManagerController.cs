using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class FileManagerController : Controller
    {
        // GET: FileManager
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Calendar()
        {
            return View();
        }
        [ValidateInput(false)]
        public ActionResult FileManager5Partial()
        {
            return PartialView("_FileManager5Partial", FileManagerControllerFileManager5Settings.Model);
        }

        public FileStreamResult FileManager5PartialDownload()
        {
            return FileManagerExtension.DownloadFiles("FileManager5", FileManagerControllerFileManager5Settings.Model);
        }

        public ActionResult Shedular()
        {
            return View();
        }
        public ActionResult she()
        {
            return View();
        }

        public ActionResult UploadControlUpload()
        {
            UploadControlExtension.GetUploadedFiles("UploadControl", FileManagerControllerUploadControlSettings.UploadValidationSettings, FileManagerControllerUploadControlSettings.FileUploadComplete);
            return null;
        }
    }
    public class FileManagerControllerUploadControlSettings
    {
        public static DevExpress.Web.UploadControlValidationSettings UploadValidationSettings = new DevExpress.Web.UploadControlValidationSettings()
        {
            AllowedFileExtensions = new string[] { ".jpg", ".jpeg" },
            MaxFileSize = 4000000
        };
        public static void FileUploadComplete(object sender, DevExpress.Web.FileUploadCompleteEventArgs e)
        {
            if (e.UploadedFile.IsValid)
            {
                // Save uploaded file to some location
            }
        }
    }

    public class FileManagerControllerFileManager5Settings
    {
        public const string RootFolder = @"~\App_Data\uploads";

        public static string Model { get { return RootFolder; } }
    }


}