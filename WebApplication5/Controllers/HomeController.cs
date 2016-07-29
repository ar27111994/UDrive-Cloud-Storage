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
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
               return  RedirectToAction("home","Home");
            }
            
            return View();
        }
        //duplication main move file par masla aya hua  hai.r general sy data fetch karwa k lana hai.with the help of piku...:p


        [HttpGet]
        public ActionResult Feedback()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Feedback(WebApplication5.Models.Feedback feedback)
        {

            ApplicationDbContext db = new ApplicationDbContext();



            if (ModelState.IsValid)
            {
                db.Feedbacks.Add(feedback);
                db.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        //login to home
        public ActionResult EditDoc(string path = null)
        {

            //here we are going to remove storage of edit files and will be updated in home method
            var fileName =path;
            FileInfo fi = new FileInfo(fileName);
            double byteCount = fi.Length;

            double totalSize = byteCount;
            double totalSizeInKb = totalSize / 1024;
            double totalSizeInmb = totalSizeInKb / 1024;
            double totalSizeIngb = totalSizeInmb / 1024;

            double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
            double max = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();
            if (curr < max)
            {
                double updateStorage = curr - totalSizeIngb;

                var um = db.UserManagers.Where(e => e.Email == User.Identity.Name).Single();

                um.CurrentStorage = updateStorage;

                db.Entry(um).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                StorageUpdate SU = new StorageUpdate();
                SU.Email = User.Identity.Name;
                SU.FilePath = path;
                db.StorageUpdate.Add(SU);
                db.SaveChanges();
                ViewData["File"] = path;

                return View();
            }
            else
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Storage Full Upgrade your account!');window.location.href = 'home';</script>");

            }
          
        }
        [HttpPost]
        public ActionResult EditDoc()
        {


            return View();
        }


        public  void storageUpdate(List<string> Storage)
        {
            foreach (var path in Storage)
            {
                var fileName = path;
                FileInfo fi = new FileInfo(fileName);
                double byteCount = fi.Length;

                double totalSize = byteCount;
                double totalSizeInKb = totalSize / 1024;
                double totalSizeInmb = totalSizeInKb / 1024;
                double totalSizeIngb = totalSizeInmb / 1024;

                double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
                double updateStorage = curr + totalSizeIngb;

                var um = db.UserManagers.Where(e => e.Email == User.Identity.Name).Single();

                um.CurrentStorage = updateStorage;

                db.Entry(um).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();
                db.StorageUpdate.RemoveRange(db.StorageUpdate.Where(e => e.Email == User.Identity.Name));
                db.SaveChanges();
            }

        }
        [Authorize]
        public ActionResult home(string folderPath = null)
        {
            List<fileViewModel> files = new List<fileViewModel>();
            string[] dir = null;
            List<UserFiles> shared = null;
            List<fileViewModel> direc = new List<fileViewModel>();
            List<string> StorageUpt = new List<string>();
          StorageUpt= db.StorageUpdate.Where(e => e.Email == User.Identity.Name).Select(fp => fp.FilePath).ToList();
            storageUpdate(StorageUpt);
            var security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
            var Videos = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name +"/"+ "Videos" + "_" + security);
            if (!Directory.Exists(Videos))
            {
                Directory.CreateDirectory(Videos);
            }

            string p = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name);
            if (folderPath != null)
            {

                shared = db.UserFiless.Where(e => e.Email == User.Identity.Name).ToList();
                string[] spliter = folderPath.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);

                string ff =spliter[1];


                p = p + "/" + ff;
                ViewData["p"] = p;

                dir = Directory.GetDirectories(folderPath);
                //  ViewData["directories"] = folderName.Substring(folderName.IndexOf(User.Identity.Name.First<char>()) + User.Identity.Name.Length + 1);
                ViewData["directories"] = folderPath;

                ViewData["folders"] = direc;
                if (db.FileHash.Any(x => x.FolderName == ff))
                {
                    foreach (var i in shared)
                    {
                        var f = db.FileHash.Where(x => x.id == i.id && x.FolderName == ff).SingleOrDefault();
                        if (f != null)
                            files.Add(new fileViewModel { isCheck = false, path = f.path, name = Path.GetFileName(f.path),folderName=f.FolderName });

                    }
                }
                foreach (string i in dir)
                {

                    direc.Add(new fileViewModel { isCheck = false, path = i, name = ViewData["directories"] + "/" + new DirectoryInfo(i).Name });

                }


            }
            else {
                shared = db.UserFiless.Where(e => e.Email == User.Identity.Name).ToList();
                //shared= db.UserFiless.Where(e => e.Email == User.Identity.Name).Select(i=>i.id).SelectMany();
                dir = Directory.GetDirectories(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
                ViewData["directories"] = "";


                foreach (string i in dir)
                {

                    direc.Add(new fileViewModel { isCheck = false, path = i, name = new DirectoryInfo(i).Name });

                }
               

                ViewData["folders"] = direc;

                if (db.FileHash.Any(x => x.FolderName == ""))
                {
                    foreach (var i in shared)
                    {

                        var f = db.FileHash.Where(x => x.id == i.id && x.FolderName == "").SingleOrDefault();
                        if (f != null)
                            files.Add(new fileViewModel { isCheck = false, path = f.path, name = Path.GetFileName(f.path),folderName=f.FolderName });

                    }
                }
            }
            double CS = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();

            CS = Math.Round(CS, 2);
            ViewData["CurrentStorage"] = CS;
            double MS = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();
            // long ds=GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
            MS = Math.Round(MS, 2);
            ViewData["MaxStorage"] = MS;
            double per = ((double)ViewData["CurrentStorage"] / (double)ViewData["MaxStorage"]);
            per = per * 100;
            per = Math.Round(per, 2);
            ViewData["per"] = per;


            files = files.Concat(direc).ToList();
            var count = db.Messages.Count();
            ViewData["countnotification"]=count;
            return View(files);
        }
        //upgradeAccount
        public ActionResult GetMessages()
        {
           MessagesRepository _messageRepository = new MessagesRepository();
          
            
            return PartialView("_MessagesList", _messageRepository.GetAllMessages());
        }
        public ActionResult UpgradeAccount()
        {
            return View(db.packages.ToList());
        }
        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        [Authorize]
        [HttpPost]
        public ActionResult do_upload(HttpPostedFileBase file, string folderName)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    double directorySizeINGb = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
                    //= GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
                    double directorySizeInMB = 1024 * directorySizeINGb;
                    double directorySizeInKB = 1024 * directorySizeInMB;
                    double directorySizeInBytes = 1024 * directorySizeInKB;
                    var fileName = Path.GetFileName(file.FileName);

                    double byteCount = file.ContentLength;
                    double totalSize = byteCount + directorySizeInBytes;
                    double totalSizeInKb = totalSize / 1024;
                    double totalSizeInmb = totalSizeInKb / 1024;
                    double totalSizeIngb = totalSizeInmb / 1024;

                    var max = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();

                    if (totalSizeIngb < max)
                    {
                        //var directorySizeInKiloBytes = totalSize / 1024;
                        //var directorySizeInMbs = directorySizeInKiloBytes / 1024;

                        UserManager um = new UserManager();


                        um = db.UserManagers.Where(u => u.Email.Equals(User.Identity.Name)).Single();
                        um.CurrentStorage = totalSizeIngb;

                        db.Entry(um).State = System.Data.Entity.EntityState.Modified;


                        db.SaveChanges();

                        string lastFolderName = "";


                        var path = "";
                        var reload = "";
                        //  var path = Path.Combine(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name) + "/" + lastFolderName, fileName);
                        if (folderName != "")
                        {
                            if (Directory.Exists(folderName))
                            {
                                try
                                {
                                    path = folderName + "/" + fileName;
                                    reload = folderName;
                                }
                                catch (Exception )
                                {

                                }
                            }
                            else
                            {
                                path = Path.Combine(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name + "/"), folderName);
                                reload = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name);
                            }

                            //  path=Path.GetFullPath(path).Replace("\\", "/");
                            //var fn = Path.GetFileName(path);


                            // path = Path.Combine(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), fileName);
                            char[] email = User.Identity.Name.ToCharArray();

                            string[] folders = folderName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folders[1].Contains("/"))
                            {
                                folders[1] = Path.Combine("", folders[1]);
                            }

                            lastFolderName = folders[1];

                        }
                        else
                        {
                           path = Path.Combine(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), fileName);
                            DirectoryInfo temp = new DirectoryInfo(path);

                        }
                        file.SaveAs(path);
                       

                        //file hash code

                        FilesHash fh = new FilesHash();
                        MD5 md5 = new MD5CryptoServiceProvider();

                        Byte[] originalBytes = System.IO.File.ReadAllBytes(path);

                        string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));
                        int find = db.FileHash.Where(h => h.HashCode.Equals(hash)).Count();
                       // string mail = db.FileHash.Where(h => h.HashCode.Equals(hash)).Select(m => m.path).SingleOrDefault();
                        string pathMatchWithHash = db.FileHash.Where(h => h.HashCode == hash).Select(p => p.path).FirstOrDefault();
                        var extension = @Path.GetExtension(path);
                        var video = new[] { ".mp4", ".avi", ".flv", ".mkv", ".vob", ".wmv", ".xlsx" };

                        if (video.Contains(extension))
                        {
                            if (find == 0)
                            {
                                fh.HashCode = hash;
                                string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();

                                if (!lastFolderName.Contains(security))
                                {
                                    System.IO.File.Delete(path);
                                    return Content("<script language='javascript' type='text/javascript'>alert('video file must be inside Videos folder!');window.location.href = 'home';</script>");

                                }
                                else {
                                    fh.FolderName = lastFolderName;
                                    fh.path = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name + "/" + lastFolderName + "/" + fileName);
                                    db.FileHash.Add(fh);
                                    db.SaveChanges();
                                    var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
                                    var userfiles = new UserFiles()
                                    {
                                        Email = User.Identity.Name,
                                        File = fh,
                                        id = fh.id,
                                        User = user

                                    };
                                    db.UserFiless.Add(userfiles);
                                    db.SaveChanges();

                                    return RedirectToAction("home", "Home", new { folderPath = reload });
                                }
                            }

                            else if (pathMatchWithHash.Contains("uploads\\general"))
                            {
                                string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();

                                if (!lastFolderName.Contains(security))
                                {
                                    return Content("<script language='javascript' type='text/javascript'>alert('video file must be inside videos folder!');window.location.href = 'home';</script>");

                                    
                                }

                                else 
                                {
                                    System.IO.File.Delete(path);
                                    fh.HashCode = hash;

                                    fh.FolderName = lastFolderName;
                                    fh.path = pathMatchWithHash;
                                    db.FileHash.Add(fh);
                                    db.SaveChanges();
                                    var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
                                    var userfiles = new UserFiles()
                                    {
                                        Email = User.Identity.Name,
                                        File = fh,
                                        id = fh.id,
                                        User = user

                                    };
                                    db.UserFiless.Add(userfiles);
                                    db.SaveChanges();
                                    General g = new General();
                                    g.Folder = lastFolderName;
                                    g.PathForUser = pathMatchWithHash;
                                    g.PathInDb = pathMatchWithHash;
                                    db.General.Add(g);
                                    db.SaveChanges();
                                    

                                    return RedirectToAction("home", "Home", new { folderPath = reload });

                                }


                            }
                            else
                            {
                                string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();

                                if (!lastFolderName.Contains(security))
                                {
                                    System.IO.File.Delete(path);
                                    return Content("<script language='javascript' type='text/javascript'>alert('video file must be inside Videos folder!');window.location.href = 'home';</script>");

                                }
                                else {
                                    var oldfilepath = db.FileHash.Where(of => of.HashCode.Equals(hash)).Select(of => of.path).Single();
                                    System.IO.File.Move((string)oldfilepath, Server.MapPath("~/App_Data/uploads/general/" + fileName));
                                    General g = new General();
                                    string u = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name + "/" + lastFolderName + "/" + fileName);

                                    g.PathForUser = u;
                                    g.PathInDb = Server.MapPath("~/App_Data/uploads/general/" + fileName);
                                    g.Folder = lastFolderName;
                                    db.General.Add(g);
                                    db.SaveChanges();
                                    g.PathForUser = oldfilepath;
                                    g.PathInDb= Server.MapPath("~/App_Data/uploads/general/" + fileName);
                                    string f=db.FileHash.Where(p => p.path == pathMatchWithHash).Select(fo => fo.FolderName).Single();
                                    g.Folder = f;
                                    db.General.Add(g);
                                    db.SaveChanges();
                                    System.IO.File.Delete(path);
                                    var change = db.FileHash.Where(lf => lf.HashCode.Equals(hash)).Single();
                                    change.path = Server.MapPath("~/App_Data/uploads/general/" + fileName);
                                    db.Entry(change).State = System.Data.Entity.EntityState.Modified;

                                    db.SaveChanges();





                                    var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();

                                    FilesHash filehash = new FilesHash();
                                    filehash.HashCode = hash;
                                    filehash.path = Server.MapPath("~/App_Data/uploads/general/" + fileName);


                                    filehash.FolderName = lastFolderName;
                                    db.FileHash.Add(filehash);
                                    var userfiles = new UserFiles()
                                    {
                                        Email = User.Identity.Name,
                                        File = filehash,
                                        id = filehash.id,
                                        User = user


                                    };
                                    db.UserFiless.Add(userfiles);
                                    db.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            fh.HashCode = hash;
                            fh.FolderName = lastFolderName;
                            fh.path = Server.MapPath("~/App_Data/uploads/" + User.Identity.Name + "/" + lastFolderName + "/" + fileName);
                            db.FileHash.Add(fh);
                            db.SaveChanges();
                            var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
                            var userfiles = new UserFiles()
                            {
                                Email = User.Identity.Name,
                                File = fh,
                                id = fh.id,
                                User = user

                            };
                            db.UserFiless.Add(userfiles);
                            db.SaveChanges();

                            return RedirectToAction("home", "Home", new { folderPath = reload });

                        }
                    }
                    else
                    {
                        return RedirectToAction("premier");

                    }
                }

            }
            catch (Exception )
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Please Select your file first!');window.location.href = 'home';</script>");

            }

            return RedirectToAction("home");
        }

        public ActionResult foldermaking(string folderName = null)
        {

            try
            {
                String name = Request.Form["path"];
                if (name != null)
                {
                    if (folderName != "")
                    {
                        var folder = Path.GetFullPath(folderName);
                        folder = folder + "/" + name;

                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                    }
                    else
                    {
                        var folder = Path.Combine(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), name);
                        folder = Path.GetFullPath(folder);
                        Directory.CreateDirectory(folder);
                    }
                    return RedirectToAction("home");
                }
            }
            catch (Exception )
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Please Enter Folder Name first!');window.location.href = 'home';</script>");

            }
            return RedirectToAction("home");
        }
        [Authorize]
        public ActionResult premier()
        {
            return View();
        }
        public bool genarateEmail(String from, String to, String cc, String displayName, String password, String subjet, String body)
        {
            bool EmailIsSent = false;

            MailMessage m = new MailMessage();
            SmtpClient sc = new SmtpClient();
            try
            {
                m.From = new MailAddress(from, displayName);
                m.To.Add(new MailAddress(to, displayName));
                m.CC.Add(new MailAddress("xxx@gmail.com", "Display name CC"));

                m.Subject = subjet;
                m.IsBodyHtml = true;
                m.Body = body;


                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new
                System.Net.NetworkCredential(from, password);
                sc.EnableSsl = true;
                sc.Send(m);

                EmailIsSent = true;

            }
            catch (Exception )
            {
                EmailIsSent = false;
            }

            return EmailIsSent;
        }
        public ActionResult Photos()
        {
            Dictionary<string, string> files = new Dictionary<string, string>();
            string[] file = Directory.GetFiles(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), "*.jpg", SearchOption.AllDirectories);


            foreach (string i in file)
            {
                files.Add(i, Path.GetFileName(i));

            }
            ViewData["filename"] = files;
            return View();
        }
     //   public ActionResult Videos()
     //   {
     //       Dictionary<string, string> files = new Dictionary<string, string>();
     //       ViewData["CurrentStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
     //       ViewData["MaxStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();
     //       // long ds=GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
     //       double per = ((double)ViewData["CurrentStorage"] / (double)ViewData["MaxStorage"]);
     //       per = Math.Round(per, 2);
     //       ViewData["per"] = per * 100;

     //       //var allowedExtensions = new[] { ".doc", ".txt", ".docx" };
     //       // string[] file = Directory.GetFiles(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), "*.mp4", SearchOption.AllDirectories);
     //       var file = Directory
     //.EnumerateFiles(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name)) //<--- .NET 4.5
     //.Where(filee => filee.ToLower().EndsWith("mp4") || filee.ToLower().EndsWith("flv"))
     //.ToList();

     //       foreach (string i in file)
     //       {
     //           files.Add(i, Path.GetFileName(i));

     //       }
     //       ViewData["filename"] = files;
     //       return View();
     //   }
     //   public ActionResult Contact()
     //   {
     //       ViewBag.Message = "Your contact page.";

     //       return View();
     //   }

            public ActionResult Videos()
        {

            List<fileViewModel> files = new List<fileViewModel>();
          var  shared = db.UserFiless.Where(e => e.Email == User.Identity.Name).ToList();
           
            foreach (var i in shared)
            {
                var f = db.FileHash.Where(x => x.id == i.id ).SingleOrDefault();
                var video = new[] { ".mp4", ".avi", ".flv", ".mkv", ".vob", ".wmv" };

                    var extension = @Path.GetExtension(f.path);

                if (video.Contains(extension))
                {
                    files.Add(new fileViewModel { isCheck = false, path = f.path, name = Path.GetFileName(f.path), folderName = f.FolderName });
                }
            }
            return View(files);
        }
        public ActionResult PlayVideo(string VidPath)
        {

            ViewData["VidPath"] = VidPath;
            return View();
        }
        public ActionResult rte()
        {

            return PartialView("_rte");

        }
        public ActionResult EditExcel(string path)
        {

            ViewData["File"] = path;
            return View();
        }

        public ActionResult UserProfile()
        {

            var profile = db.UserManagers.Where(e => e.Email == User.Identity.Name).ToList();
            return View(profile);
        }
        public ActionResult EditDocPartial(string File)
        {
            ViewData["filename"] = File;
            return PartialView("_EditDocPartial");
        }

        public ActionResult EditExcelPartial(string File)
        {
            ViewData["filename"] = File;
            return PartialView("_EditExcelPartial");
        }

        public ActionResult ImageGallery()
        {
            ViewData["CurrentStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
            ViewData["MaxStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();
            // long ds=GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
            double per = ((double)ViewData["CurrentStorage"] / (double)ViewData["MaxStorage"]);
            per = Math.Round(per, 2);
            ViewData["per"] = per * 100;


            return View();
        }




        [ValidateInput(false)]
        //public ActionResult ImageGalleryPartial()
        //{
        //    object model = Server.MapPath("~/App_Data/uploads/sajidishaq007@gmail.com");
        //    return PartialView("_ImageGalleryPartial", model);
        //}

        //Display every single image coming from home
        public ActionResult picture(string path)
        {
            ViewData["pic"] = path;
            return View();

        }

        //[HttpPost]
        //public FileResult Download(List<fileViewModel> item)
        //{
        //    var outputstream = new MemoryStream();
        //    try
        //    {

        //        using (var Zip = new ZipFile())
        //        {
        //            foreach (var file in item)
        //            {
        //                if (file.isCheck == true)
        //                {
        //                    string path = file.path;
        //                    if (Directory.Exists(path))
        //                    {
        //                        string[] folders = path.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
        //                        if (!folders[1].Contains("/"))
        //                        {
        //                            folders[1] = Path.Combine("", folders[1]);
        //                        }

        //                        string lastFolderName = folders[1];
        //                        string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
        //                        string ext = Path.GetExtension(path);

        //                        if (path.Contains(security))
        //                        {
        //                            Zip.AddDirectory(file.path);

        //                            List<string> Generalfile = null;

        //                            Generalfile = db.General.Where(p => p.Folder == lastFolderName || p.Folder.Contains(lastFolderName)).Select(p => p.PathInDb).ToList();

        //                            List<string> unique = Generalfile.Distinct().ToList();
        //                            foreach (var GF in unique)
        //                            {


        //                                Zip.AddFile(GF, "Files");
        //                            }
        //                        }

        //                        else
        //                        {
        //                            if (Directory.Exists(file.path))
        //                                Zip.AddDirectory(file.path);
        //                            else
        //                                Zip.AddFile(file.path, "Files");
        //                        }
        //                    }
        //                    else
        //                    Zip.AddFile(path, "Files");
        //                }

        //            }

        //            Zip.Save(outputstream);
        //        }
        //        outputstream.Position = 0;
        //        return File(outputstream, "applicatoin/zip", "Download.zip");
        //        throw new FileNotFoundException("File not found");
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return File(outputstream, "applicatoin/zip", "Download.zip");
        //}


        public ActionResult video()
        {
            return View();
        }

        [HttpPost]
        public FileResult Download(List<fileViewModel> item)
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
                    foreach (var file in item)
                    {
                        if (file.isCheck == true)
                        {

                            string path = file.path;

                           
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
                    }
                    foreach (var file in FilesAndFolder)
                    {


                        //}
                        //List<string> unique = FilesNames.Distinct().ToList();

                        //foreach (var GF in unique)
                        //{
                        string filename=Path.GetFileName(file);
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
                        bool check = Zip.ContainsEntry(lastFolderName+@"\"+filename);

                        if(check==false)
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


        //Folder Rename
        public ActionResult FolderRename(string NewName, string OldName)
        {
            DirectoryInfo newPath = Directory.GetParent(OldName);
            string folderPath = Path.GetFullPath(newPath.FullName);
            OldName = Path.GetFullPath(OldName);
            System.IO.Directory.Move(Path.GetFullPath(OldName), folderPath + "/" + NewName);

            return RedirectToAction("home");
        }
        //File Rename
        public ActionResult Rename(string NewName, string OldName)
        {
            var extension = @Path.GetExtension(OldName);
            DirectoryInfo newPath = Directory.GetParent(OldName);
            string folderPath = Path.GetFullPath(newPath.FullName);
            MD5 md5 = new MD5CryptoServiceProvider();
            OldName = Path.GetFullPath(OldName);
            Byte[] originalBytes = System.IO.File.ReadAllBytes(OldName);

            string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));


            System.IO.File.Move(Path.GetFullPath(OldName), folderPath + "/" + NewName + extension);
            FilesHash fh = new FilesHash();

            fh = db.FileHash.Where(p => p.HashCode.Equals(hash)).Single();
            fh.path = folderPath + "/" + NewName + extension;
            db.Entry(fh).State = System.Data.Entity.EntityState.Modified;


            db.SaveChanges();

            return RedirectToAction("home");

        }
        public ActionResult Operations(List<fileViewModel> files, int op)
        {

            return View();

        }

        //DELETE FILES INSIDE THE FOLDERS AND SUBFOLDERS FILES
        public void DeleteFolder(DirectoryInfo path)
        {
            foreach (FileInfo file in path.GetFiles())
            {
                
                double length = new System.IO.FileInfo(file.FullName).Length;

                
                double totalSizeInKb = length / 1024;
                double totalSizeInmb = totalSizeInKb / 1024;
                double fileSizeIngb = totalSizeInmb / 1024;
                double directorySizeINGb = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
                double updatedirectorysize = directorySizeINGb - fileSizeIngb;
                
                    UserManager um = new UserManager();


                    um = db.UserManagers.Where(u => u.Email.Equals(User.Identity.Name)).Single();
                    um.CurrentStorage = updatedirectorysize;

                    db.Entry(um).State = System.Data.Entity.EntityState.Modified;


                    db.SaveChanges();
                System.IO.File.Delete(file.FullName);
                int find = db.FileHash.Where(p => p.path == file.FullName).Count();
                if (find == 1)
                {
                    db.FileHash.Remove(db.FileHash.Where(p => p.path == file.FullName).Single());
                    db.SaveChanges();
                }
            }

            string[] folders = path.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
            if (!folders[1].Contains("/"))
            {
                folders[1] = Path.Combine("", folders[1]);
            }

            string deletelastFolderName = folders[1];
            List<string> files = null;
           files= db.FileHash.Where(p => p.FolderName == deletelastFolderName).Select(p => p.path).ToList();
            foreach (var filee in files)
            {
                string file = filee;
                double length = new System.IO.FileInfo(file).Length;

                double totalSizeInKb = length / 1024;
                double totalSizeInmb = totalSizeInKb / 1024;
                double filesizeIngb = totalSizeInmb / 1024;
                UserManager um = new UserManager();
                double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
                um = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Single();
                um.CurrentStorage = curr - filesizeIngb;
                db.Entry(um).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


                int find = db.FileHash.Where(p => p.path == file).Count();

                var rem = db.FileHash.Where(p => p.path == file && p.FolderName == deletelastFolderName).Single();


                db.FileHash.Remove(rem);
                db.SaveChanges();

                var genRem = db.General.Where(p => p.PathInDb == file && p.Folder == deletelastFolderName).Single();
                db.General.Remove(genRem);
                db.SaveChanges();
                if (find == 1)
                {
                    System.IO.File.Delete(file);
                }

            }
            foreach (DirectoryInfo dir in path.GetDirectories())
            {
                DirectoryInfo subdr = new DirectoryInfo(dir.FullName);
                DeleteFolder(subdr);
            }

        }
        public ActionResult Delete(List<fileViewModel> collection)
        {


            try
            {
                foreach (var items in collection)
            {
                if (items.isCheck == true)
                {

                        if (Directory.Exists(items.path))
                        {
                            DirectoryInfo path = new DirectoryInfo(items.path);
                            DeleteFolder(path);

                        }
                        else
                        {

                            if(items.path.Contains("uploads\\general"))
                            {
                                //general file case
                                string file = items.path;
                                double length = new System.IO.FileInfo(file).Length;

                                double totalSizeInKb = length / 1024;
                                double totalSizeInmb = totalSizeInKb / 1024;
                                double filesizeIngb = totalSizeInmb / 1024;
                                UserManager um = new UserManager();
                                double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
                                um = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Single();
                                um.CurrentStorage = curr - filesizeIngb;
                                db.Entry(um).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();


                                int find = db.FileHash.Where(p => p.path == file).Count();
                              
                                    var rem = db.FileHash.Where(p => p.path == file&&p.FolderName==items.folderName).Single();


                                    db.FileHash.Remove(rem);
                                    db.SaveChanges();
                                    
                                    var genRem=db.General.Where(p => p.PathInDb == file && p.Folder == items.folderName).Single();
                                    db.General.Remove(genRem);
                                    db.SaveChanges();
                                if (find == 1)
                                {
                                    System.IO.File.Delete(file);
                                }
                               

                            }
                            else
                            {
                                //simple file case
                                string file = items.path;
                                double length = new System.IO.FileInfo(file).Length;

                                double totalSizeInKb = length / 1024;
                                double totalSizeInmb = totalSizeInKb / 1024;
                                double filesizeIngb = totalSizeInmb / 1024;
                                UserManager um = new UserManager();
                                double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
                                um = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Single();
                                um.CurrentStorage = curr - filesizeIngb;
                                db.Entry(um).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                                System.IO.File.Delete(file);

                                int find = db.FileHash.Where(p => p.path == file).Count();
                                if (find == 1)
                                {
                                    var rem = db.FileHash.Where(p => p.path == file).Single();


                                    db.FileHash.Remove(rem);
                                    db.SaveChanges();
                                }
                            }



                        }
                         
                    }

                }
                foreach (var item in collection)
                {
                    if (Directory.Exists(item.path))

                        if (item.isCheck == true)
                        {
                            System.IO.Directory.Delete(item.path, true);
                        }
                }
            }
            catch(Exception )
            {

            }
            return RedirectToAction("home");

        }
        //public ActionResult Delete(string[] selections)
        //{
          
        //    foreach (string file in selections)
        //    {
        //        double s1 = file.Length;
        //        double totalSizeInKb = s1 / 1024;
        //        double totalSizeInmb = totalSizeInKb / 1024;
        //        double filesizeIngb = totalSizeInmb / 1024;
        //        UserManager um = new UserManager();
        //        double curr = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.CurrentStorage).Single();
        //        um = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Single();
        //        um.CurrentStorage = curr - filesizeIngb;
        //        db.Entry(um).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();
        //        file.Delete();
        //    }
        //    foreach (DirectoryInfo dir in di.GetDirectories())
        //    {

              
        //        dir.Delete(true);
        //    }

           


        //    return RedirectToAction("home");
        //}
        //public ActionResult Restore(List<fileViewModel> item)
        //{
        //    foreach (var file in item)
        //    {
        //        if (file.isCheck == true)
        //        {
        //            if (Directory.Exists(file.path))
        //            {
        //                var FolderName = new DirectoryInfo(file.path).Name;
        //                Directory.Move(file.path, Server.MapPath("~/App_Data/uploads/" + User.Identity.Name + "/" + FolderName));

        //            }
        //        }
        //    }
        //    return RedirectToAction("RecycleBin");
        //}
                    
                
        //public ActionResult Recycle(List<fileViewModel> files)
        //{

        //    foreach (var file in files)
        //    {
        //        if (file.isCheck == true)
        //        {
        //            if (Directory.Exists(file.path))
        //            {
        //                var FolderName = new DirectoryInfo(file.path).Name;
        //                string oldPath = file.path;
        //                var NotallowedExtensions = new[] { "Png",".jpeg",".jpg",".mp4",".mp3", ".mp4", ".avi", ".flv", ".mkv", ".vob", ".wmv" };
        //                if (!file.path.Contains(NotallowedExtensions.ToString()))
        //                {
        //                    string newPath = Server.MapPath("~/App_Data/RecycleBin/" + User.Identity.Name + "/" + FolderName);

        //                    Directory.Move(oldPath, newPath);
        //                }
        //                else
        //                {
        //                    DirectoryInfo dir;
        //                    dir.Delete(true);
        //                }

        //            }



        //            else {
        //                string fullPath = file.path;
        //                if (System.IO.File.Exists(fullPath))
        //                {
        //                    MD5 md5 = new MD5CryptoServiceProvider();

        //                    Byte[] originalBytes = System.IO.File.ReadAllBytes(fullPath);

        //                    string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));

        //                    System.IO.File.Move(fullPath, Server.MapPath("~/App_Data/RecycleBin/" + User.Identity.Name + "/" + Path.GetFileName(fullPath)));


        //                    int id = db.FileHash.Where(h => h.HashCode.Equals(hash)).Select(i => i.id).Single();
        //                    FilesHash fh = new FilesHash();
        //                    fh = db.FileHash.Where(i => i.id == id).Single();
        //                    db.FileHash.Remove(fh);
        //                    UserFiles uf = new UserFiles();
        //                    uf = db.UserFiless.Where(i => i.id == id).Single();
        //                    db.UserFiless.Remove(uf);
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
               
        //    }
        //    return RedirectToAction("home");
        //}
        ////Get
        //public ActionResult RecycleBin()
        //{
        //    List<fileViewModel> direc = new List<fileViewModel>();
        //    List<fileViewModel> files = new List<fileViewModel>();
        //    string[] file = Directory.GetFiles(Server.MapPath("~/App_Data/RecycleBin/" + User.Identity.Name));


        //    foreach (string i in file)
        //    {
        //        files.Add(new fileViewModel { isCheck = false, path = i, name = Path.GetFileName(i) });


        //    }
        //  string [] dir = Directory.GetDirectories(Server.MapPath("~/App_Data/RecycleBin/" + User.Identity.Name));
        //    ViewData["directories"] = Server.MapPath("~/App_Data/RecycleBin/" + User.Identity.Name);
        //    foreach (string i in dir)
        //    {

        //        direc.Add(new fileViewModel { isCheck = false, path = i, name = ViewData["directories"] + "/" + new DirectoryInfo(i).Name });

        //    }
        //    ViewData["filename"] = files;
        //    ViewData["CurrentStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
        //    ViewData["MaxStorage"] = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();
        //    // long ds=GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
        //    double per = ((double)ViewData["CurrentStorage"] / (double)ViewData["MaxStorage"]);
        //    per = Math.Round(per, 2);
        //    ViewData["per"] = per * 100;

        //    files = files.Concat(direc).ToList();
        //    return View(files);

        //}
        //[ValidateInput(false)]
        public ActionResult ImageGalleryPartial1()
        {
            object model = @"~/App_Data/uploads/"+User.Identity.Name;
            return PartialView("_ImageGalleryPartial1", model);
        }
        //PDF viewer
        public FileResult Pdf(string path)
        {
            path = Path.GetFullPath(path);
            return File(path, "application/pdf");

        }
        //Get getting directories send to view
        public ActionResult Cut()
        {
            Dictionary<string, string> folder = new Dictionary<string, string>();
            string[] Dirs = Directory.GetDirectories(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name),"*.*",SearchOption.AllDirectories);
            foreach (var Dir in Dirs)
            {


                DirectoryInfo temp = new DirectoryInfo(Dir);

                folder.Add(Dir, temp.Name);

            }
            ViewData["Direc"] = new SelectList(folder.ToList(), "Key", "Value");

            return View();
        }

        public bool CutFolder(DirectoryInfo source, DirectoryInfo target)
        {
            string[] folders = target.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
            if (!folders[1].Contains("/"))
            {
                folders[1] = Path.Combine("", folders[1]);
            }

            string lastFolderName = folders[1];
            string[] Sourcefolders = source.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
            if (!Sourcefolders[1].Contains("/"))
            {
                Sourcefolders[1] = Path.Combine("", Sourcefolders[1]);
            }

            string SourceFolderName = Sourcefolders[1];
            List<string> Generalfiles = null;

            Generalfiles = db.FileHash.Where(p => p.FolderName == SourceFolderName).Select(p => p.path).ToList();
            foreach (var filePath in Generalfiles)
            {
              string fname=  Path.GetFileName(filePath);

                //this is the code for general file for which we have to only change last folder name
                //*************************************************************************////////****************///////////////////////////////
                if (filePath.Contains("uploads\\general"))
                {
                    string newDirectory = target.FullName + @"\" + fname;
                    string oldDirectory = source + @"\" + fname;

                    string[] foldersss = source.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                    if (!foldersss[1].Contains("/"))
                    {
                        foldersss[1] = Path.Combine("", foldersss[1]);
                    }
                    string oldfolder = foldersss[1];
                    string[] newfolders = target.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                    if (!newfolders[1].Contains("/"))
                    {
                        newfolders[1] = Path.Combine("", newfolders[1]);
                    }
                    string targetfolder = newfolders[1];
                    string fileInGeneralPath = folders[1];

                       int find = db.FileHash.Where(p => p.path == filePath && p.FolderName == targetfolder).Count();
                        if (find == 0)
                        {
                       string pathindb= db.General.Where(p => p.PathForUser == oldDirectory).Select(p => p.PathInDb).Single();
                        var chg=db.FileHash.Where(p => p.path == pathindb && p.FolderName == oldfolder).Single();
                                chg.FolderName = targetfolder;
                                db.SaveChanges();
                           
                        }
                   var ch = db.General.Where(p => p.PathForUser == oldDirectory).Single();

                    ch.PathForUser = newDirectory;
                    ch.Folder = targetfolder;

                    db.Entry(ch).State = System.Data.Entity.EntityState.Modified;


                    db.SaveChanges();

                }

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                else {
                   

                        FilesHash fh = new FilesHash();
                    string fil = Path.GetFileName(filePath);
                       
                        string fileFind = target + @"\" + fil;

                      
                        int count = db.FileHash.Where(p => p.path == fileFind).Count();
                        if (count == 0)
                        {
                            var change = db.FileHash.Where(p => p.path.Equals(filePath)).Single();//for updating the record



                           // string newPath = Path.Combine(target + "/" + Path.GetFileName(source.Name));//new file path
                            string newpath = Path.Combine(target.FullName, fil);
                            change.path = newpath;
                            string[] folderss = target.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folderss[1].Contains("/"))
                            {
                                folderss[1] = Path.Combine("", folderss[1]);
                            }


                            change.FolderName = folderss[1];




                            db.Entry(change).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            System.IO.File.Move(filePath, fileFind);
                        }
                        else {
                            return false;
                        }

                    }
                }
                foreach (DirectoryInfo dir in source.GetDirectories())
                {
                    DirectoryInfo subdr = new DirectoryInfo(dir.FullName);
                    CutFolder(subdr, target.CreateSubdirectory(dir.Name));
                }

                return true;
            
        }
        [HttpPost]
        public ActionResult Cut(string Direc, string[] selections, string[] selectionsFolder)
        {

            bool del = true;


            foreach (var FilesOrDirectory in selections)
            {
                string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
                if (FilesOrDirectory.Contains("uploads\\general") && !Direc.Contains(security))
                {
                    return Content("<script language='javascript' type='text/javascript'>alert('Viseo File must be inside Videos Folder!');window.location.href = 'home';</script>");

                }
                if (FilesOrDirectory.Contains(security) && !Direc.Contains(security))
                {
                    return Content("<script language='javascript' type='text/javascript'>alert('Viseo File must be inside Videos Folder!');window.location.href = 'home';</script>");

                }

                if (Directory.Exists(FilesOrDirectory))
                {
                    try
                    {
                        string[] folderss = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                        if (!folderss[1].Contains(@"\"))
                        {
                            folderss[1] = Path.Combine("", folderss[1]);
                        }
                        string fold = folderss[1];
                        foreach (var item in selections)
                        {


                            string[] sourc = item.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!sourc[1].Contains(@"\"))
                            {
                                sourc[1] = Path.Combine("", sourc[1]);
                            }
                            string s = sourc[1];
                            //if coming is a general file then we have to change in general folder also
                        //    List<General> temp = null;
                        //    temp = db.General.Where(f => f.Folder == s).ToList();
                        //    foreach (var fg in temp)
                        //    {
                        //        fg.Folder = fold;
                        //        fg.PathForUser = Direc;
                        //        db.Entry(fg).State = System.Data.Entity.EntityState.Modified;
                        //    }




                        //    db.SaveChanges();

                        }
                        IEnumerable<string> fo = selectionsFolder;
                        IEnumerator<string> folder = (IEnumerator<string>)fo.GetEnumerator();
                        folder.MoveNext();

                        foreach (string item in selections)
                        {
                            string securitty = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
                            var extension = @Path.GetExtension(item);
                            var video = new[] { ".mp4", ".avi", ".flv", ".mkv", ".vob", ".wmv", ".xlsx" };

                            if (video.Contains(extension) && !Direc.Contains(securitty))
                            {

                                return Content("<script language='javascript' type='text/javascript'>alert('Your Destination Must be in Videos Folder!');window.location.href = 'home';</script>");



                            }
                            else {
                                if (Directory.Exists(item))
                                {
                                    DirectoryInfo source = new DirectoryInfo(item);
                                    DirectoryInfo destination = new DirectoryInfo(Direc);

                                    del= CutFolder(source, destination);
                                    if (del == false)
                                    {
                                        return Content("<script language='javascript' type='text/javascript'>alert('Files of Folder Already Exist!');window.location.href = 'home';</script>");

                                    }
                                }
                               
                                else
                                {
                                    string folderName = folder.Current.ToString();

                                    DirectoryInfo temppath = new DirectoryInfo(item);

                                    if (item.ToString().Contains(@"uploads\general"))
                                    {
                                        //files in general
                                        FilesHash f = new FilesHash();
                                        string[] folders = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                                        if (!folders[1].Contains(@"\"))
                                        {
                                            folders[1] = Path.Combine("", folders[1]);
                                        }
                                        //string temp = Path.GetFullPath(item);
                                        //temp.Replace("\\",@"\");

                                        if (Direc.Contains(security))
                                        {
                                            DirectoryInfo folde = new DirectoryInfo(folderName);

                                            string fol = folde.ToString();
                                            f = db.FileHash.Where(e => e.path == temppath.FullName && e.FolderName == fol).Single();
                                        }
                                        else
                                        {
                                            return Content("<script language='javascript' type='text/javascript'>alert('video file must be inside  Videos folder!');window.location.href = 'home';</script>");

                                        }
                                        f.FolderName = folders[1];
                                        db.Entry(f).State = System.Data.Entity.EntityState.Modified;


                                        db.SaveChanges();
                                    }
                                    else {
                                        //Files not in general
                                        //TO INSERT DATA INTO FILE HASH 
                                        FilesHash fh = new FilesHash();

                                        var change = db.FileHash.Where(p => p.path == item).Single();//for updating the record
                                        string newPath = Path.Combine(Direc + @"\" + Path.GetFileName(item));//new file path
                                        change.path = newPath;
                                        string[] folders = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                                        if (!folders[1].Contains("/"))
                                        {
                                            folders[1] = Path.Combine("", folders[1]);
                                        }


                                        change.FolderName = folders[1];

                                        string CurrentUser = User.Identity.Name;


                                        db.Entry(change).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        System.IO.File.Move(item, Direc + "/" + Path.GetFileName(item));

                                    }
                                }
                            }
                            folder.MoveNext();

                        }
                        foreach (var items in selections)
                        {
                            DirectoryInfo itemo = new DirectoryInfo(items);

                            if (Directory.Exists(itemo.FullName))
                            {


                                System.IO.Directory.Delete(itemo.FullName, true);


                            }
                        }
                        return Content("<script language='javascript' type='text/javascript'>alert('Sucessfully moved!');window.location.href = 'home';</script>");

                    }

                    catch (Exception )
                    {
                        return Content("<script language='javascript' type='text/javascript'>alert('Copied Failed !');window.location.href = 'home';</script>");

                    }
                }
                else
                {


                    if (!FilesOrDirectory.Contains("uploads\\general"))
                    {


                        string[] folders = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                        if (!folders[1].Contains("/"))
                        {
                            folders[1] = Path.Combine("", folders[1]);
                        }

                        string Folder = folders[1];
                        string FileName=Path.GetFileName(FilesOrDirectory);
                        string path = Direc + @"\" + FileName;
                        int find = db.FileHash.Where(p => p.path == path && p.FolderName == Folder).Count();
                        if (find==0)
                        {
                            //TO INSERT DATA INTO FILE HASH 
                            FilesHash fh = new FilesHash();

                            var change = db.FileHash.Where(p => p.path == FilesOrDirectory).FirstOrDefault();//for updating the record
                            string newPath = Path.Combine(Direc + @"\" + Path.GetFileName(FilesOrDirectory));//new file path
                            change.path = newPath;
                           

                            change.FolderName = folders[1];

                            string CurrentUser = User.Identity.Name;


                            db.Entry(change).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            System.IO.File.Move(FilesOrDirectory, Direc + "/" + Path.GetFileName(FilesOrDirectory));
                        }
                        else
                        {
                            return Content("<script language='javascript' type='text/javascript'>alert('File Already Exist!');window.location.href = 'home';</script>");

                        }
                    }
                   
                    else
                    {
                        string[] folders = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                        if (!folders[1].Contains("/"))
                        {
                            folders[1] = Path.Combine("", folders[1]);
                        }



                        foreach (var GeneralFileOldFolderName in selectionsFolder)
                        {
                            foreach (var item in selections)
                            {
                                if (item.Contains("uploads\\general"))
                                {
                                    int count = db.FileHash.Where(p => p.path == item && p.FolderName == GeneralFileOldFolderName).Count();
                                    if (count != 0)//for handling null exception 
                                    {
                                        var update = db.FileHash.Where(p => p.path == item && p.FolderName == GeneralFileOldFolderName).SingleOrDefault();
                                        string f = folders[1];
                                        update.FolderName = f;
                                        db.Entry(update).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        //dpdaing general table
                                        var updateGeneral = db.General.Where(p => p.PathInDb == item && p.Folder == GeneralFileOldFolderName).Single();
                                        string fe = folders[1];
                                        updateGeneral.Folder = fe;
                                        string newPath = Path.Combine(Direc + @"\" + Path.GetFileName(item));//new file path

                                        updateGeneral.PathForUser = newPath;
                                        db.Entry(updateGeneral).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                }

            }
           
            return RedirectToAction("home", "Home", new { folderPath = Direc });
        }


        //

        //public void CopyAll(DirectoryInfo source, DirectoryInfo target)
        //{
        //    string newDirectory = target.FullName + "/" + source.Name;
        //    Directory.CreateDirectory(newDirectory);
        //    //var Filehash = db.FileHash.Where(p => p.path == source.ToString()).Select(h => h.HashCode).Single();
        //    string folder = target.ToString();
        //    // Copy each file into the new directory.
        //    foreach (FileInfo fi in source.GetFiles())
        //    {
        //        string path=fi.FullName;
        //        string oldFilePath = source.ToString() + "/" + fi;
        //         var Filehash = db.FileHash.Where(p => p.path == oldFilePath).Select(h => h.HashCode).Single();
        //        fi.CopyTo(Path.Combine(newDirectory, fi.Name), true);
        //               FilesHash fh = new FilesHash();
        //               // fh.HashCode = Filehash;


        //                string oldFolderName = new DirectoryInfo(folder).Name;

        //                string CurrentUser = User.Identity.Name;
        //        fh.HashCode = Filehash;

        //                if (oldFolderName == CurrentUser)
        //                {
        //                    fh.FolderName = "";
        //                }

        //                else {

        //                    fh.FolderName = oldFolderName;
        //                }
        //        string filename = Path.GetFileName(path);
        //                fh.path =newDirectory + "/" + filename;
        //                db.FileHash.Add(fh);
        //                db.SaveChanges();
        //                var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
        //                var userfiles = new UserFiles()
        //                {
        //                    Email = User.Identity.Name,
        //                    File = fh,
        //                    id = fh.id,
        //                    User = user

        //                };
        //                db.UserFiless.Add(userfiles);
        //                db.SaveChanges();


        //    }

        //    // Copy each subdirectory using recursion.
        //    foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        //    {
        //        DirectoryInfo nextTargetSubDir =
        //            target.CreateSubdirectory(diSourceSubDir.Name);
        //        CopyAll(diSourceSubDir, nextTargetSubDir);
        //    }
        //}


        //private static bool CopyDirectory(string SourcePath, string DestinationPath, bool overwriteexisting)
        //       {
        //           bool ret = true;
        //           try
        //           {
        //               SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
        //               DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

        //               if (Directory.Exists(SourcePath))
        //               {
        //                   if (Directory.Exists(DestinationPath) == false)
        //                       Directory.CreateDirectory(DestinationPath);

        //                   foreach (string fls in Directory.GetFiles(SourcePath))
        //                   {
        //                       FileInfo flinfo = new FileInfo(fls);
        //                       flinfo.CopyTo(DestinationPath + flinfo.Name, overwriteexisting);
        //                   }
        //                   foreach (string drs in Directory.GetDirectories(SourcePath))
        //                   {
        //                       DirectoryInfo drinfo = new DirectoryInfo(drs);
        //                       if (CopyDirectory(drs, DestinationPath + drinfo.Name, overwriteexisting) == false)
        //                           ret = false;
        //                   }

        //               }
        //               else
        //               {
        //                   ret = false;
        //               }
        //           }
        //           catch (Exception ex)
        //           {
        //               ret = false;
        //           }
        //           return ret;
        //       }
        //Get Directories send to view

        public ActionResult Share()
        {
            return View();
        }
        public ActionResult Copy()
        {
            Dictionary<string, string> folder = new Dictionary<string, string>();
            string[] Dirs = Directory.GetDirectories(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), "*.*", SearchOption.AllDirectories);
            foreach (var Dir in Dirs)
            {


                DirectoryInfo temp = new DirectoryInfo(Dir);

                folder.Add(Dir, temp.Name);

            }
            ViewData["Direc"] = new SelectList(folder.ToList(), "Key", "Value");

            return View();
        }

        public bool CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            string[] folders = target.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
            if (!folders[1].Contains("/"))
            {
                folders[1] = Path.Combine("", folders[1]);
            }

            string lastFolderName = folders[1];
            string[] Sourcefolders = source.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
            if (!Sourcefolders[1].Contains("/"))
            {
                Sourcefolders[1] = Path.Combine("", Sourcefolders[1]);
            }

            string SourceFolderName = Sourcefolders[1];
            List<string> Generalfiles = null;

            Generalfiles = db.FileHash.Where(p => p.FolderName == SourceFolderName).Select(p => p.path).ToList();
            foreach (var filePath in Generalfiles)
            {
                string filename = Path.GetFileName(filePath);
                string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
                
                if (filePath.Contains("uploads\\general"))
                {
                    if (filePath.Contains("uploads\\general") && !target.ToString().Contains(security))
                    {

                    }
                    else
                    {
                        string item = target + @"\" + filename;
                        string sourcewithfile = source + @"\" + filename;
                        string[] folderss = target.ToString().Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                        if (!folderss[1].Contains(@"\"))
                        {
                            folderss[1] = Path.Combine("", folderss[1]);
                        }
                        string oldFolderName = folderss[1];

                        int findFileInFolder = db.FileHash.Where(f => f.FolderName == lastFolderName && f.path == item).Count();
                        if (findFileInFolder == 0)
                        {
                            FilesHash fh = new FilesHash();

                            MD5 md5 = new MD5CryptoServiceProvider();




                           string pathforhash= db.General.Where(p => p.PathForUser == sourcewithfile && p.Folder == SourceFolderName).Select(p => p.PathInDb).First();

                            Byte[] originalBytes = System.IO.File.ReadAllBytes(pathforhash);

                            string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));

                            string[] foldersss = target.FullName.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folders[1].Contains("/"))
                            {
                                folders[1] = Path.Combine("", folders[1]);
                            }

                            string lastFolderrName = folders[1];
                            int count = db.FileHash.Where(p => p.path == item && p.FolderName == lastFolderName).Count();//checking alreay exist or not
                            if (count == 0)
                            {

                                fh.path = pathforhash;
                                fh.HashCode = hash;
                                fh.FolderName = lastFolderName;
                                db.FileHash.Add(fh);
                                db.SaveChanges();
                                General g = new General();
                                g.PathForUser = target + @"\" + Path.GetFileName(item);
                                g.PathInDb = pathforhash;
                                g.Folder = lastFolderName;
                                db.General.Add(g);
                                db.SaveChanges();
                                var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();

                                var userfiles = new UserFiles()
                                {
                                    Email = User.Identity.Name,
                                    File = fh,
                                    id = fh.id,
                                    User = user

                                };
                                db.UserFiless.Add(userfiles);
                                db.SaveChanges();

                            }
                        }


                    }
                }
                else
                {
                   
                    string filepath = target + @"\" + filename;
                    int find = db.FileHash.Where(p => p.path == filepath && p.FolderName == lastFolderName).Count();
                    if (find == 0)
                    {
                        string oldFilePath = source+@"\"+ filename;
                        double directorySizeINGb = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
                        //= GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
                        double directorySizeInMB = 1024 * directorySizeINGb;
                        double directorySizeInKB = 1024 * directorySizeInMB;
                        double directorySizeInBytes = 1024 * directorySizeInKB;

                        double byteCount = filepath.Length;
                        double totalSize = byteCount + directorySizeInBytes;
                        double totalSizeInKb = totalSize / 1024;
                        double totalSizeInmb = totalSizeInKb / 1024;
                        double totalSizeIngb = totalSizeInmb / 1024;

                        var max = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();

                        if (totalSizeIngb < max)
                        {
                            //var directorySizeInKiloBytes = totalSize / 1024;
                            //var directorySizeInMbs = directorySizeInKiloBytes / 1024;

                            UserManager um = new UserManager();


                            um = db.UserManagers.Where(u => u.Email.Equals(User.Identity.Name)).Single();
                            um.CurrentStorage = totalSizeIngb;

                            db.Entry(um).State = System.Data.Entity.EntityState.Modified;


                            db.SaveChanges();


                            MD5 md5 = new MD5CryptoServiceProvider();

                            Byte[] originalBytes = System.IO.File.ReadAllBytes(oldFilePath);

                            string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));

                            // var Filehash = db.FileHash.Where(p => p.HashCode == hash).Select(h => h.HashCode).Single();
                            FilesHash fh = new FilesHash();
                            fh.HashCode = hash;
                            string folder = target.ToString();

                            string[] folderss = folder.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folderss[1].Contains(@"\"))
                            {
                                folderss[1] = Path.Combine("", folderss[1]);
                            }
                            string oldFolderName = folderss[1];

                            string CurrentUser = User.Identity.Name;


                            if (oldFolderName == CurrentUser)
                            {
                                fh.FolderName = "";
                            }


                            if (!oldFolderName.Contains("/"))
                            {
                                oldFolderName = Path.Combine(@"\", oldFolderName);
                            }


                            fh.FolderName = oldFolderName;


                            fh.path = target + @"\" + Path.GetFileName(oldFilePath);
                            db.FileHash.Add(fh);
                            db.SaveChanges();
                            var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
                            var userfiles = new UserFiles()
                            {
                                Email = User.Identity.Name,
                                File = fh,
                                id = fh.id,
                                User = user

                            };
                            db.UserFiless.Add(userfiles);
                            db.SaveChanges();

                            System.IO.File.Copy(filePath, filepath);

                          //  filepath.CopyTo(Path.Combine(filepath, filename));
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

            }
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                DirectoryInfo subdr = new DirectoryInfo(dir.FullName);
                CopyFolder(subdr, target.CreateSubdirectory(dir.Name));
            }


          
            return true;
        }




        //    foreach (FileInfo file in source.GetFiles())
        //    {

        //        string filepath = target + @"\" + file.Name;
        //        int find = db.FileHash.Where(p => p.path == filepath && p.FolderName == lastFolderName).Count();
        //        if (find == 0) {
        //            string oldFilePath = Path.GetFullPath(Path.Combine(source.ToString(), file.ToString()));
        //            double directorySizeINGb = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(c => c.CurrentStorage).Single();
        //            //= GetDirectorySize(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name));
        //            double directorySizeInMB = 1024 * directorySizeINGb;
        //            double directorySizeInKB = 1024 * directorySizeInMB;
        //            double directorySizeInBytes = 1024 * directorySizeInKB;

        //            double byteCount = file.Length;
        //            double totalSize = byteCount + directorySizeInBytes;
        //            double totalSizeInKb = totalSize / 1024;
        //            double totalSizeInmb = totalSizeInKb / 1024;
        //            double totalSizeIngb = totalSizeInmb / 1024;

        //            var max = db.UserManagers.Where(e => e.Email.Equals(User.Identity.Name)).Select(m => m.MaxStorage).Single();

        //            if (totalSizeIngb < max)
        //            {
        //                //var directorySizeInKiloBytes = totalSize / 1024;
        //                //var directorySizeInMbs = directorySizeInKiloBytes / 1024;

        //                UserManager um = new UserManager();


        //                um = db.UserManagers.Where(u => u.Email.Equals(User.Identity.Name)).Single();
        //                um.CurrentStorage = totalSizeIngb;

        //                db.Entry(um).State = System.Data.Entity.EntityState.Modified;


        //                db.SaveChanges();


        //                MD5 md5 = new MD5CryptoServiceProvider();

        //                Byte[] originalBytes = System.IO.File.ReadAllBytes(oldFilePath);

        //                string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));

        //                // var Filehash = db.FileHash.Where(p => p.HashCode == hash).Select(h => h.HashCode).Single();
        //                FilesHash fh = new FilesHash();
        //                fh.HashCode = hash;
        //                string folder = target.ToString();

        //                string[] folderss = folder.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
        //                if (!folderss[1].Contains(@"\"))
        //                {
        //                    folderss[1] = Path.Combine("", folderss[1]);
        //                }
        //                string oldFolderName = folderss[1];

        //                string CurrentUser = User.Identity.Name;


        //                if (oldFolderName == CurrentUser)
        //                {
        //                    fh.FolderName = "";
        //                }


        //                if (!oldFolderName.Contains("/"))
        //                {
        //                    oldFolderName = Path.Combine(@"\", oldFolderName);
        //                }


        //                fh.FolderName = oldFolderName;


        //                fh.path = target + @"\" + Path.GetFileName(oldFilePath);
        //                db.FileHash.Add(fh);
        //                db.SaveChanges();
        //                var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
        //                var userfiles = new UserFiles()
        //                {
        //                    Email = User.Identity.Name,
        //                    File = fh,
        //                    id = fh.id,
        //                    User = user

        //                };
        //                db.UserFiless.Add(userfiles);
        //                db.SaveChanges();


        //                file.CopyTo(Path.Combine(target.FullName, file.Name));
        //            }
        //        }

        //    else
        //    {
        //        return false;
        //    }

        //}
        //    foreach (DirectoryInfo dir in source.GetDirectories())
        //    {
        //        DirectoryInfo subdr = new DirectoryInfo(dir.FullName);
        //        CopyFolder(subdr, target.CreateSubdirectory(dir.Name));
        //    }
        //    return true;

        //}
        [HttpPost]
        public ActionResult Copy(string Direc, string[] selections)
        {
            bool del = true;
            try
            {

                foreach (string item in selections)
                {

                    string security = db.Users.Where(e => e.Email == User.Identity.Name).Select(s => s.SecurityStamp).Single();
                    if(item.Contains("uploads\\general")&& !Direc.Contains(security))
                    {
                        return Content("<script language='javascript' type='text/javascript'>alert('Viseo File must be inside Videos Folder!');window.location.href = 'home';</script>");

                    }
                    if (item.Contains(security) && !Direc.Contains(security))
                    {
                        return Content("<script language='javascript' type='text/javascript'>alert('Viseo File must be inside Videos Folder!');window.location.href = 'home';</script>");

                    }
                    if (Directory.Exists(item))
                    {       
                        DirectoryInfo source = new DirectoryInfo(item);
                        DirectoryInfo destination = new DirectoryInfo(Direc);

                        del=CopyFolder(source,destination);
                        if(del==false)
                        {
                            return Content("<script language='javascript' type='text/javascript'>alert('File inside Folder connot copied try another location!');window.location.href = 'home';</script>");

                        }
                    }
                    else
                    {
                        if (!item.Contains("uploads\\general"))
                        {


                            string[] folderss = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folderss[1].Contains(@"\"))
                            {
                                folderss[1] = Path.Combine("", folderss[1]);
                            }
                            string oldFolderName = folderss[1];
                            string FileName = Path.GetFileName(item);
                            string path = Direc + @"\" + FileName;

                            int find = db.FileHash.Where(p => p.path == path && p.FolderName == oldFolderName).Count();
                            if (find==0)
                            {
                                //TO INSERT DATA INTO FILE HASH 
                                var Filehash = db.FileHash.Where(p => p.path == item).Select(h => h.HashCode).First();
                                FilesHash fh = new FilesHash();
                                fh.HashCode = Filehash;
                                string folder = Direc;
                                //string oldFolderName = new DirectoryInfo(folder).Name;
                                //if(!oldFolderName.Contains(@"\"))
                                //{
                                //    oldFolderName = Path.Combine("",oldFolderName);
                                //}

                                string CurrentUser = User.Identity.Name;


                                if (oldFolderName == CurrentUser)
                                {
                                    fh.FolderName = "";
                                }

                                else {

                                    fh.FolderName = oldFolderName;
                                }
                                string pathe = Direc + @"\" + Path.GetFileName(item);
                                int count = db.FileHash.Where(p => p.path == pathe && p.FolderName == oldFolderName).Count();
                                if (count == 0)
                                {
                                    fh.path = path;
                                    db.FileHash.Add(fh);
                                    db.SaveChanges();
                                    var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();
                                    var userfiles = new UserFiles()
                                    {
                                        Email = User.Identity.Name,
                                        File = fh,
                                        id = fh.id,
                                        User = user

                                    };
                                    db.UserFiless.Add(userfiles);
                                    db.SaveChanges();

                                    // oldpath.path = Direc + "/" + Path.GetFileName(item);
                                    //DirectoryInfo oldFolderName = Directory.GetParent(Direc);
                                    // oldpath.FolderName = new DirectoryInfo(Direc).Name;//use to get last directory name to save in db so we can fetch data 
                                    // db.Entry(oldpath).State = System.Data.Entity.EntityState.Modified;
                                    // db.SaveChanges();
                                    System.IO.File.Copy(item, Direc + "/" + Path.GetFileName(item));
                                }
                            }
                            else
                            {
                                return Content("<script language='javascript' type='text/javascript'>alert('File Already Exist!');window.location.href = 'home';</script>");

                            }
                        }
                        else
                        {

                            string[] folderss = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                            if (!folderss[1].Contains(@"\"))
                            {
                                folderss[1] = Path.Combine("", folderss[1]);
                            }
                            string oldFolderName = folderss[1];

                            int findFileInFolder = db.FileHash.Where(f => f.FolderName == oldFolderName && f.path == item).Count();
                            if (findFileInFolder==0)
                            {
                                FilesHash fh = new FilesHash();

                                MD5 md5 = new MD5CryptoServiceProvider();

                                Byte[] originalBytes = System.IO.File.ReadAllBytes(item);

                                string hash = string.Concat(md5.ComputeHash(originalBytes).Select(x => x.ToString("X2")));

                                string[] folders = Direc.Split(new string[] { User.Identity.Name }, StringSplitOptions.None);
                                if (!folders[1].Contains("/"))
                                {
                                    folders[1] = Path.Combine("", folders[1]);
                                }

                                string lastFolderName = folders[1];
                                int count = db.FileHash.Where(p => p.path == item && p.FolderName == lastFolderName).Count();//checking alreay exist or not
                                if (count == 0)
                                {

                                    fh.path = item;
                                    fh.HashCode = hash;
                                    fh.FolderName = lastFolderName;
                                    db.FileHash.Add(fh);
                                    db.SaveChanges();
                                    General g = new General();
                                    g.PathForUser = Direc + @"\" + Path.GetFileName(item);
                                    g.PathInDb = item;
                                    g.Folder = lastFolderName;
                                    db.General.Add(g);
                                    db.SaveChanges();
                                    var user = db.UserManagers.Where(x => x.Email.Equals(User.Identity.Name)).Single();

                                    var userfiles = new UserFiles()
                                    {
                                        Email = User.Identity.Name,
                                        File = fh,
                                        id = fh.id,
                                        User = user

                                    };
                                    db.UserFiless.Add(userfiles);
                                    db.SaveChanges();

                                }
                            }
                            else
                            {
                                return Content("<script language='javascript' type='text/javascript'>alert('File Already Exist!');window.location.href = 'home';</script>");

                            }
                        }
                    }

                }

            }
            catch (Exception )
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Copied Failed !');window.location.href = 'home';</script>");

            }
            return Content("<script language='javascript' type='text/javascript'>alert('Sucessfully copied!');window.location.href = 'home';</script>");

                 }

    }
}