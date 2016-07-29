using DevExpress.Web.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;

using System.Web.Http.Description;
using System.Web.Mvc;
using WebApplication5.Models;

namespace WebApplication5.Areas.Admin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        // GET: Admin/Admin
        ApplicationDbContext db = new ApplicationDbContext();
        //  var IdentityManager = new AuthenticationIdentityManager(new IdentityStore(new ApplicationDbContext()));

      
        public ActionResult Index()
        {
            if(User.Identity.Name!="sajidishaq007@gmail.com")
            {
               return  RedirectToAction("home", "Home", new { area = "" });
            }

            long directorySizeinbytes = GetDirectorySize(Server.MapPath("~/App_Data/uploads"));
           long directorySizeinkb = (long)directorySizeinbytes / 1024;
            long directorySizeinmb = directorySizeinkb / 1024;
            if (directorySizeinmb < 1024)
            {
                ViewData["directorySizeinmb"] = directorySizeinmb;
                ViewData["unit"] = "MB";
            }
            else {
                double dsg = directorySizeinmb / 1024.00;
                dsg = Math.Round(dsg, 2);
                ViewData["directorySizeinmb"] = dsg;
                ViewData["unit"] = "GB";
            }
            Directory.EnumerateFiles(Server.MapPath("~/App_Data/uploads"), "*.*").Count();
            //System.IO.Directory.GetFiles("Path", "*.txt",).Count()
            ViewData["user"]=db.Users.ToList().Count();
            string dir = Server.MapPath("~/App_Data/uploads");
            //string[] file = Directory.GetFiles(Server.MapPath("~/App_Data/uploads/" + User.Identity.Name), "*.jpg", SearchOption.AllDirectories);

            string[] files;
            int numFiles;
            files = System.IO.Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
            numFiles = files.Length;
            ViewData["numFiles"] = numFiles;

            //chart
            //Getting size of extention type file
            ViewData["Docx"] = GetDirectoryWord(Server.MapPath("~/App_Data/uploads"));
            ViewData["Excel"] = GetDirectoryExcel(Server.MapPath("~/App_Data/uploads"));
            ViewData["Jpg"] = GetDirectoryJpg(Server.MapPath("~/App_Data/uploads"));
            ViewData["Jpeg"] = GetDirectoryJpeg(Server.MapPath("~/App_Data/uploads"));
            ViewData["mp4"] = GetDirectorymp4(Server.MapPath("~/App_Data/uploads"));
            ViewData["Root"] = GetDirectoryRoot(Server.MapPath("~/App_Data/uploads"));

            //converting to mb
            ViewData["DocxInKB"] = Convert.ToDouble(ViewData["Docx"]) / 1024;
            ViewData["DocxInMb"] = (double)ViewData["DocxInKB"] / 1024;

            ViewData["RootInKB"] = Convert.ToDouble(ViewData["Docx"]) / 1024;
            ViewData["RootInMb"] = (double)ViewData["RootInKB"] / 1024;
            ViewData["RootInGB"] = (double)ViewData["RootInMb"] / 1024;

            ViewData["mp4InKB"] = Convert.ToDouble(ViewData["mp4"]) / 1024;
            ViewData["mp4InMb"] = (double)ViewData["mp4InKB"] / 1024;


            ViewData["ExcelInKB"] = Convert.ToDouble(ViewData["Excel"]) / 1024;
            ViewData["ExcelInMb"] = (double)ViewData["ExcelInKB"] / 1024;


            ViewData["JpgInKB"] = Convert.ToDouble(ViewData["Jpg"]) / 1024;
            ViewData["JpgInMb"] = (double)ViewData["JpgInKB"] / 1024;


            ViewData["JpegInKB"] = Convert.ToDouble(ViewData["Jpeg"] )/ 1024;
            ViewData["JpegInMb"] = Convert.ToDouble(ViewData["JpegInKB"] )/ 1024;
            Dictionary<string, double> FileTypes = new Dictionary<string, double>();
            FileTypes.Add("Word", (double)ViewData["DocxInMb"]);

            FileTypes.Add("Excel", (double)ViewData["ExcelInMb"]);

            FileTypes.Add("Jpg", (double)ViewData["JpgInMb"]);
            FileTypes.Add("Jpeg", (double)ViewData["JpegInMb"]);
            FileTypes.Add("mp4", (double)ViewData["mp4InMb"]);
            //layout elements
            int less = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1&&m.Sent==false).Count();

            ViewData["lessStorageCount"] = less;
            //warning count
            int w = WarningCount();
            ViewData["WarningCount"] = w;
             int d= DeleteCount();
            ViewData["DeleteCount"] = d;
            int total =w+d+ less;
            ViewData["TotalCount"] = total;            
            //Package chart data
            var Products = (from a in db.packages select new { a.PackageName,a.OnlineStorage,a.Price,a.MoneyBack }).ToList();
            var json = JsonConvert.SerializeObject(Products, Formatting.Indented);
            ViewBag.json = json;
            return View();
       
        }
        public JsonResult Grid()
        {
            var data = db.packages.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult dummy()
        {
            return View(db.packages.ToArray());
        }
        public ActionResult QuickMail(string emailto, string subject, string Message)
        {

            if (emailto != null)
            {
                string email = emailto;
                string subj = subject;
                string body = Message;

                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();

                m.From = new MailAddress("sajidishaq007@gmail.com", "UDrive");
                m.To.Add(new MailAddress(email, "UDrive"));

                m.Subject = subject;
                m.IsBodyHtml = true;
                m.Body = body;


                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new
                System.Net.NetworkCredential("sajidishaq007@gmail.com", "03445244144");
                sc.EnableSsl = true;
                sc.Send(m);
                return RedirectToAction("Index");

            }
            return View("C:/Users/sajid khan/documents/visual studio 2015/Projects/WebApplication5/WebApplication5/Areas/Admin/Views/Admin/Index.cshtml");
        }
     private int DeleteCount()
        {
            DateTime mydate = DateTime.Now;

            //UserManager um = new UserManager();
            var year = db.UserManagers.ToList();
            int count = 0;
            foreach (var y in year)
            {
                if (y.LastLoginYear == mydate.Year)
                {

                    if ((mydate.Month - y.LastLoginMonth) > 6 && y.LoginWarning == true)
                    {
                        count++;
                    }
                   
                }

                else
                {
                    if (((12 - y.LastLoginMonth) + mydate.Month)  > 6 && y.LoginWarning == true)
                    {
                        count++;
                    }
                }
               
               
            }

            return count;
        }
        private int WarningCount()
        {
            DateTime mydate = DateTime.Now;

            //UserManager um = new UserManager();

            

            var year = db.UserManagers.ToList();
            int count=0;
            foreach (var y in year)
            {
                if (y.LastLoginYear == mydate.Year)
                {
                    
                    if((mydate.Month-y.LastLoginMonth)>5&&y.LoginWarning==false)
                    {
                        count++;
                    }
                    //count = (from ummm in db.UserManagers
                    //         where ((mydate.Month - ummm.LastLoginMonth) > 5 && ummm.LoginWarning == false)
                    //         select ummm).Count();

                }
                else
                {
                    if (((12 - y.LastLoginMonth) + mydate.Month) > 5 && y.LoginWarning == false)
                        count++;
                }
            }
            return count;
            
        }
        // GET: Admin/Packages/Create
        public ActionResult CreatePackage()
        {
            return View();
        }
        // GET: Admin/Packages/Details/5
        public ActionResult PackageDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Package package = db.packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePackage([Bind(Include = "PackageId,PackageName,OnlineStorage,MoneyBack,Price")] Package package)
        {
            if (ModelState.IsValid)
            {
                db.packages.Add(package);
                db.SaveChanges();
                return RedirectToAction("PackageIndex");
            }

            return View(package);
        }


        // GET: Admin/Packages
        public ActionResult PackageIndex()
        {
            return View(db.packages.ToList());
        }
        public ActionResult Login()
        {

            return View();
        }
        // GET: Admin/Packages/Edit/5
        public ActionResult PackageEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Package package = db.packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PackageEdit([Bind(Include = "PackageId,PackageName,OnlineStorage,MoneyBack,Price")] Package package)
        {
            if (ModelState.IsValid)
            {
                db.Entry(package).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PackageIndex");
            }
            return View(package);
        }
        public ActionResult PackageDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Package package = db.packages.Find(id);
            if (package == null)
            {
                return HttpNotFound();
            }
            return View(package);
        }

        // POST: Admin/Packages/Delete/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Package package = db.packages.Find(id);
            db.packages.Remove(package);
            db.SaveChanges();
            return RedirectToAction("PackageIndex");
        }

        //less storage compose email
        public ActionResult ComposeEmail(string em)
        {
            if (em != null)
            {
                string email = em;
                string subject = "Less Storage";
                string body = "your StorGE getting low please upgrade your account";

                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();

                m.From = new MailAddress("sajidishaq007@gmail.com", "UDrive");
                m.To.Add(new MailAddress(email, "UDrive"));

                m.Subject = subject;
                m.IsBodyHtml = true;
                m.Body = body;


                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new
                System.Net.NetworkCredential("sajidishaq007@gmail.com", "03445244144");
                sc.EnableSsl = true;
                sc.Send(m);
                UserManager um = new UserManager();
                um = db.UserManagers.Where(u => u.Email == em).Single();
                um.Sent = true;
                db.Entry(um).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

              
            
               
               
                return RedirectToAction("LessStorageNotification", "Admin");
            }
          
            return View();
          
        }


        //less storage compose email
        public ActionResult ComposeWarningEmail(string em)
        {
            if (em != null)
            {
                string email = em;
                string subject = "Login Notification";
                string body = "Please Login To Your Account To otherwise we will delete your account and data";

                MailMessage m = new MailMessage();
                SmtpClient sc = new SmtpClient();

                m.From = new MailAddress("sajidishaq007@gmail.com", "UDrive");
                m.To.Add(new MailAddress(email, "UDrive"));

                m.Subject = subject;
                m.IsBodyHtml = true;
                m.Body = body;


                sc.Host = "smtp.gmail.com";
                sc.Port = 587;
                sc.Credentials = new
                System.Net.NetworkCredential("sajidishaq007@gmail.com", "03445244144");
                sc.EnableSsl = true;
                sc.Send(m);
                UserManager um = new UserManager();
                um = db.UserManagers.SingleOrDefault(u => u.Email == em);
                um.LoginWarning = true;
                db.Entry(um).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
              
                return RedirectToAction("WarningUser", "Admin");
            }

            return View();

        }

        //public ActionResult ADLog()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public ActionResult Login(WebApplication5.Models.ADLog login)
        //{
        //     bool validEmail = db.ADLogs.Any(x => x.Email == login.Email);

        //    if (!validEmail)
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    string password = db.ADLogs.Where(x => x.Email == login.Email)
        //                                 .Select(x => x.Password)
        //                                 .Single();

          
        //    if (!password.Equals(login.Password))
        //    {
        //        return RedirectToAction("Login");
        //    }

        //    string authId = Guid.NewGuid().ToString();

        //    Session["AuthID"] = authId;

        //    var cookie = new HttpCookie("AuthID");
        //    cookie.Value = authId;
        //    Response.Cookies.Add(cookie);

        //    return Index();
        //}

        //public ActionResult Register()
        //{

        //    return View();
        //}
        public ActionResult compose()
        {
            return View();
        }
        public ActionResult read_mail()
        {
            return View();
        }
        public ActionResult mailbox()
        {
            return View();
        }
        public ActionResult Calendar()
        {
            return View();
        }
        public ActionResult Feedback()
        {
            return View(db.Feedbacks.ToList());

        }
        //directory size
        private static long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private static long GetDirectoryWord(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.docx", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private static long GetDirectoryExcel(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.xlsx", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private static long GetDirectoryJpg(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.jpg", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private static long GetDirectoryJpeg(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.jpeg", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
        private static long GetDirectorymp4(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*.mp4", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
    
    private static long GetDirectoryRoot(string folderPath)
    {
        DirectoryInfo di = new DirectoryInfo(folderPath);
        return di.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
    }
    public ActionResult ManageUser()
        {

            int less = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1 && m.Sent == false).Count();
            ViewData["lessStorageCount"] = less;
            //warning count
            int w = WarningCount();
            ViewData["WarningCount"] = w;
            int d = DeleteCount();
            ViewData["DeleteCount"] = d;
            int total = w + d + less;
            ViewData["TotalCount"] = total;

            return View(db.UserManagers.ToList());

        }
        public ActionResult ChartPartial()
        {
            var model = new object[0];
            return PartialView("_ChartPartial1", model);
        }
        public ActionResult LessStorageNotification()
        {
            int less = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1&& m.Sent==false).Count();

            ViewData["lessStorageCount"] = less;
            //warning count
            int w = WarningCount();
            ViewData["WarningCount"] = w;
            int d = DeleteCount();
            ViewData["DeleteCount"] = d;
            int total = w + d + less;
            ViewData["TotalCount"] = total;
            var model = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1&&m.Sent==false).ToList();

            // var user = db.UserManagers.Where(u => u.CurrentStorage > 2);
            return View(model);
          
        }
        public ActionResult DeleteUser()
        {
            int less = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1 && m.Sent == false).Count();

            ViewData["lessStorageCount"] = less;
            //warning count
            int w = WarningCount();
            ViewData["WarningCount"] = w;
            int d = DeleteCount();
            ViewData["DeleteCount"] = d;
            int total = w + d + less;
            ViewData["TotalCount"] = total;
            DateTime mydate = DateTime.Now;
           
            //UserManager um = new UserManager();
           var year = db.UserManagers.ToList();

            var modell = (from ummm in db.UserManagers
                          where (((ummm.LastLoginYear == mydate.Year)
                                  ? ((mydate.Month - ummm.LastLoginMonth) > 6 && ummm.LoginWarning == true)
                                  : ((12 - ummm.LastLoginMonth) + mydate.Month) > 6)
                                && ummm.LoginWarning == true)
                          select ummm).ToList();

            return View(modell);
          
        }
        public ContentResult DeleteUserAccount(string email)
        {
            db.Users.Remove(db.Users.Where(e => e.Email == email).Single());
            db.UserManagers.Remove(db.UserManagers.Where(e=>e.Email==email).Single());
            System.IO.Directory.Delete(Server.MapPath("~/App_Data/uploads/"+email),true);
            db.SaveChanges();
            return Content("<script language='javascript' type='text/javascript'>alert('Successfully delete!');window.location.href = 'DeleteUser';</script>");
        }
        public ActionResult WarningUser()
        {
            int less = db.UserManagers.Where(m => m.MaxStorage - m.CurrentStorage <= 0.1 && m.Sent == false).Count();

            ViewData["lessStorageCount"] = less;
            //warning count
            int w = WarningCount();
            ViewData["WarningCount"] = w;
            int d = DeleteCount();
            ViewData["DeleteCount"] = d;
            int total = w + d +less;
            ViewData["TotalCount"] = total;
            DateTime mydate = DateTime.Now;
        
            //UserManager um = new UserManager();
            var year = db.UserManagers.ToList();
            var modell = (from ummm in db.UserManagers
                          where (((ummm.LastLoginYear == mydate.Year)
                                  ? ((mydate.Month - ummm.LastLoginMonth) > 5 && ummm.LoginWarning == false)
                                  : ((12 - ummm.LastLoginMonth) + mydate.Month) > 5)
                                && ummm.LoginWarning == false)
                          select ummm).ToList();
            return View(modell);

        }
        [HttpGet]
        public ActionResult create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult create(WebApplication5.Models.Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                db.Feedbacks.Add(feedback);
                db.SaveChanges();
                RedirectToAction("Index", "Admin", new { area = "Admin" });
            }
            return View();
        }
        public ActionResult chart()
        {
            return View();
        }

        public ActionResult ChartPartial1()
        {
            var model = new object[0];
            return PartialView("_ChartPartial1", model);
        }

        public ActionResult FileManager()
        {
            return View();
        }



        [ValidateInput(false)]
        public ActionResult FileManager1Partial()
        {
            return PartialView("_FileManager1Partial", AdminControllerFileManager1Settings.Model);
        }

        public FileStreamResult FileManager1PartialDownload()
        {
            return FileManagerExtension.DownloadFiles("FileManager1", AdminControllerFileManager1Settings.Model);
        }

        [ValidateInput(false)]
        public ActionResult FileManager3Partial()
        {
            return PartialView("~/Views/Admin/_FileManager3Partial.cshtml", AdminControllerFileManager3Settings.Model);
        }

        public FileStreamResult FileManager3PartialDownload()
        {
            return FileManagerExtension.DownloadFiles("FileManager3", AdminControllerFileManager3Settings.Model);
        }

       
    }
    public class AdminControllerFileManager3Settings
    {
        public const string RootFolder = @"~\App_Data\uploads";

        public static string Model { get { return RootFolder; } }
    }

    public class AdminControllerFileManager1Settings
    {
        public const string RootFolder = @"~\App_Data\uploads";

        public static string Model { get { return RootFolder; } }
    }

}


