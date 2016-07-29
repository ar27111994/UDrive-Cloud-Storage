using DevExpress.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication5.Models;
using System.Security.Cryptography;
using System.Globalization;


using System.Text;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Owin;
using Ionic.Zip;
using System.Net.Mail;
using System.Web.UI;
using System.Diagnostics;

namespace WebApplication5.Controllers
{
    public class CollabrationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Collabration
        public ActionResult Index()
        {
            return RedirectToAction("Home","home");
        }

        
       
        public FileResult DownloadShareFile(string  item)
        {
           
          
            //List<string> FilesAndFolder = null;
            List<string> FilesAndFolder = new List<string>();
            List<string> FilesNames = new List<string>();
            string lastFolderName;
            var outputstream = new MemoryStream();
            try
            {

               

               

                using (var Zip = new ZipFile())
                {

                    ///Zip.AddDirectory(item);


                    List<string> items = new List<string>();
                    items.Add(item.ToString());
                    foreach (var file in items)
                    {
                       
                            string path = item.ToString();


                            if (Directory.Exists(path))
                            {
                                string[] folders = path.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                                if (!folders[1].Contains("/"))
                                {
                                    folders[1] = Path.Combine("", folders[1]);
                                }

                                lastFolderName = folders[1];


                                var files = db.FileHash.Where(p => p.FolderName == lastFolderName || p.FolderName.Contains(lastFolderName)).Select(p => p.path).ToList();

                                foreach (string filee in files)
                                {
                                    FilesAndFolder.Add(filee);

                                }
                            }
                            else
                            {
                                FilesAndFolder.Add(path);
                            }
                        
                    }
                    foreach (var file in FilesAndFolder)
                    {


                        //}
                        //List<string> unique = FilesNames.Distinct().ToList();

                        //foreach (var GF in unique)
                        //{
                        string filename = Path.GetFileName(file);
                        string directory = Path.GetDirectoryName(file);
                        if (file.Contains(User.Identity.Name))
                        {
                            string[] folders = directory.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folders[1].Contains("/"))
                            {
                                folders[1] = Path.Combine("", folders[1]);
                            }

                            lastFolderName = folders[1];
                        }
                        else
                        {
                            lastFolderName = "Videos";

                        }
                        bool check = Zip.ContainsEntry(lastFolderName + @"\" + filename);

                        if (check == false)
                            Zip.AddFile(file, lastFolderName);
                    }





                    Zip.Save(outputstream);
                }
                outputstream.Position = 0;
                return File(outputstream, "applicatoin/zip", "Download.zip");
                throw new FileNotFoundException("File not found");
            }
            catch (Exception )
            {

            }
            return File(outputstream, "applicatoin/zip", "Download.zip");
        }






        
        [HttpPost]
        public ContentResult FileShare(string email, string[] selections)
        {
            try {
                foreach (var item in selections)
                {

                    System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                       new System.Net.Mail.MailAddress("Admin@UDrive.com", "UDrive"),
                       new System.Net.Mail.MailAddress(email));
                    m.Subject = "UDrive File Share";
                    m.Body = string.Format("Dear<BR/> You Have recieved , please click on the below link to Download your file: < a href =\"{1}\" title =\"User Email Confirm\">{1}</a>",
                                         User.Identity.Name, Url.Action("DownloadShareFile", "Collabration",
                                          new { item = item }, Request.Url.Scheme));



                    m.IsBodyHtml = true;
                    System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new System.Net.NetworkCredential("sajidishaq007@gmail.com", "03445244144");
                    smtp.EnableSsl = true;
                    smtp.Send(m);
                }
                return Content("<script language='javascript' type='text/javascript'>alert('File Shared Successfully!');window.location.href = 'Index';</script>");
            }
            catch (Exception )
            {
                return Content("<script language='javascript' type='text/javascript'>alert('File Shared Failed!');window.location.href = 'Index';</script>");

            }
        }
    }
}