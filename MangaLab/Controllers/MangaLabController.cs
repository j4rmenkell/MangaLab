using MangaLab.Models;
using MangaLab.Models.Context;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace MangaLab.Controllers
{
    public class MangaLabController : Controller
    {

        // GET: MangaLab
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult MangaListPage()
        {   
            return View();
        }

        public ActionResult MangaPage(string id) {
            ViewBag.MangaId = id;
            return View();
        }
        public ActionResult ReadPage(string chapterId, string mangaId)
        {
            ViewBag.MangaId = mangaId; 
            ViewBag.ChapterId = chapterId;
            return View();
        }
        public ActionResult SearchResultPage(string query)
        {
            ViewBag.SearchQuery = query;
            return View();
        }
        public ActionResult SignInPage()
        {
            if (Session["userID"] != null)
            {
                return RedirectToAction("HomePage", "MangaLab");
            }
            return View();
        }
        public ActionResult RegisterPage()
        {
            if (Session["userID"] != null)
            {
                return RedirectToAction("HomePage", "MangaLab");
            }
            return View();
        }

        public ActionResult AccountPage()
        {
            if (Session["userID"] == null)
            {
                return RedirectToAction("SignInPage", "MangaLab");
            }
            return View();
        }

        public ActionResult AdministrationPage()
        {
            if (Session["role"] == null || Session["role"].ToString() != "Admin")
            {
                return RedirectToAction("HomePage", "MangaLab");
            }
            return View();
        }




        //#################################################### ACCOUNT ####################################################

        //Registration
        public JsonResult RegisterUser(UserInformation userInfo)
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    // Username Checker
                    bool usernameExists = db.tbl_registrations.Any(x => x.username == userInfo.Username);
                    if (usernameExists)
                    {
                        return Json(new { message = "usernameTaken" }, JsonRequestBehavior.AllowGet);
                    }

                    // Email Checker
                    bool emailExists = db.tbl_registrations.Any(x => x.email == userInfo.Email);
                    if (emailExists)
                    {
                        return Json(new { message = "emailTaken" }, JsonRequestBehavior.AllowGet);
                    }


                    // Hashing
                    string hashedPassword = Crypto.HashPassword(userInfo.Password);

                    var regisData = new tblRegistrationsModel()
                    {
                        username = userInfo.Username,
                        email = userInfo.Email,
                        password = hashedPassword,
                        isArchived = 0,
                        roleID = 2,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    };

                    // Save 
                    db.tbl_registrations.Add(regisData);
                    db.SaveChanges();

                    // Create Log
                    var logData = new tblLogsModel()
                    {
                        registrationID = regisData.registrationID, 
                        action = "User Registered",
                        logDate = DateTime.Now
                    };

                    db.tbl_logs.Add(logData);
                    db.SaveChanges();

                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {

                throw new ArgumentException($"Registration Error: {ex.Message}, {ex.InnerException}, {ex.StackTrace}");
            }
        }

        //Login
        public JsonResult LoginUser(LoginData loginInfo)
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    var userData = (from user in db.tbl_registrations
                                    join role in db.tbl_roles on user.roleID equals role.roleID
                                    where user.username == loginInfo.User || user.email == loginInfo.User
                                    select new
                                    {
                                        UserRecord = user,
                                        RoleName = role.roleDescription
                                    }).FirstOrDefault();

                    if (userData.UserRecord.isArchived == 1)
                    {
                        return Json(new { message = "Your account has been deactivated." }, JsonRequestBehavior.AllowGet);
                    }

                    if (userData != null && Crypto.VerifyHashedPassword(userData.UserRecord.password, loginInfo.Password))
                    {
                        var logData = new tblLogsModel()
                        {
                            registrationID = userData.UserRecord.registrationID,
                            action = "User logged In",
                            logDate = DateTime.Now
                        };
                        db.tbl_logs.Add(logData);
                        db.SaveChanges(); 

                        var profilePic = db.tbl_profilepics
                                           .FirstOrDefault(p => p.registrationID == userData.UserRecord.registrationID);
                        string picPath;
                        if (profilePic != null && !string.IsNullOrEmpty(profilePic.imageFileName))
                        {
                            picPath = profilePic.imagePath + profilePic.imageFileName;
                        }
                        else
                        {
                            picPath = "/Content/M_generated.png";
                        }

         
                        Session["userID"] = userData.UserRecord.registrationID;
                        Session["username"] = userData.UserRecord.username;
                        Session["role"] = userData.RoleName;
                        Session["profilePic"] = picPath;
                        
                        return Json(new
                        {
                            message = "success"
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { message = "Invalid username or password" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Invalid username or password" }, JsonRequestBehavior.AllowGet);
            }
        }

        //Account Age and Reads
        public JsonResult GetAccountStats()
        {
            try
            {
                if (Session["userID"] == null)
                {
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {
                    var user = db.tbl_registrations.FirstOrDefault(u => u.registrationID == registrationID);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
                    }
                    TimeSpan accountAgeSpan = DateTime.Now - user.createdAt;
                    string accountAgeString = $"{Math.Floor(accountAgeSpan.TotalDays)} days old";

                    
                    int totalMangasRead = db.tbl_history
                                            .Where(h => h.registrationID == registrationID)
                                            .Select(h => h.mangaID)
                                            .Distinct()
                                            .Count();

                    return Json(new
                    {
                        success = true,
                        accountAge = accountAgeString,
                        totalMangasRead = totalMangasRead
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Login Status for Profile Picture and Role
        //Use in JS
        public JsonResult GetLoginStatus()
        {
            try
            {
                if (Session["userID"] != null)
                {
                    return Json(new
                    {
                        isLoggedIn = true,
                        role = (string)Session["role"],
                        profilePic = (string)Session["profilePic"]
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isLoggedIn = false }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isLoggedIn = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Logout
        //Destroy Sesh
        public JsonResult Logout()
        {
            try
            {
                if (Session["userID"] == null)
                {
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {


                    var logData = new tblLogsModel()
                    {
                        registrationID = registrationID,
                        action = "User logged Out",
                        logDate = DateTime.Now
                    };
                    db.tbl_logs.Add(logData);
                    db.SaveChanges();
                }


                 Session.Clear();    
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        //GetProfile Pic
        //Will Return Default PPic if walang naupload
        private string GetProfilePicPath(int registrationID)
        {
            using (var db = new MangaLabContext())
            {
                var profilePic = db.tbl_profilepics.FirstOrDefault(p => p.registrationID == registrationID);
                if (profilePic != null && !string.IsNullOrEmpty(profilePic.imageFileName))
                {
                    return profilePic.imagePath + profilePic.imageFileName;
                }
                return "/Content/M_generated.png";
            }
        }

        //Username and Profile Pic Edit
        public JsonResult SaveEditProfile(string newUsername, HttpPostedFileBase profilePicFile)
        {
            if (Session["userID"] == null)
            {
                return Json(new { success = false, message = "Unauthorized." });
            }
            int currentUserId = (int)Session["userID"];

            try
            {
                using (var db = new MangaLabContext())
                {
                    var user = db.tbl_registrations.FirstOrDefault(x => x.registrationID == currentUserId);
                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found." });
                    }
                    bool usernameExists = db.tbl_registrations.Any(x => x.username == newUsername && x.registrationID != currentUserId);

                    if (usernameExists)
                    {
                        return Json(new { success = false, message = "Username exists." });
                    }

                    //UsernameEdit


                    user.username = newUsername;
                    user.updatedAt = DateTime.Now;



                    string finalPicPath;
                    if (profilePicFile != null && profilePicFile.ContentLength > 0)
                    {
                        //Magic ni Sir Sapit
                        string uploadPath = Server.MapPath("~/Content/Uploads");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath); //Gagawa ng Directory if wala pa

                        //Can't Explain Clearly yung components na ginamit
                        //But essentially maggenerate ng unique ID for the photo
                        string fileName = Guid.NewGuid() + Path.GetExtension(profilePicFile.FileName);
                        string filePath = Path.Combine(uploadPath, fileName);
                        profilePicFile.SaveAs(filePath);

                        finalPicPath = "/Content/Uploads/" + fileName;

                        // 1 row per Account
                        
                        var profilePic = db.tbl_profilepics.FirstOrDefault(p => p.registrationID == currentUserId);
                        if (profilePic != null)
                        {
                            // Update onli
                            profilePic.imagePath = "/Content/Uploads/";
                            profilePic.imageFileName = fileName;
                            profilePic.dateUpdated = DateTime.Now;
                        }
                        else
                        {
                            // New Entry
                            var newPic = new tblProfilepicsModel()
                            {
                                registrationID = currentUserId,
                                imagePath = "/Content/Uploads/",
                                imageFileName = fileName,
                                dateCreated = DateTime.Now,
                                dateUpdated = DateTime.Now
                            };
                            db.tbl_profilepics.Add(newPic);
                        }
                    }
                    else
                    {
                        //No new pfpic
                        var profilePic = db.tbl_profilepics.FirstOrDefault(p => p.registrationID == currentUserId);
                        if (profilePic != null && !string.IsNullOrEmpty(profilePic.imageFileName))
                        {
                            finalPicPath = profilePic.imagePath + profilePic.imageFileName;
                        }
                        else
                        {
                            finalPicPath = "/Content/M_generated.png";
                        }
                    }

                    db.SaveChanges();

                    //Front-End Updates
                    Session["username"] = newUsername;
                    Session["profilePic"] = finalPicPath;

                    return Json(new
                    {
                        success = true,
                        message = "Profile updated successfully!",
                        newProfileUrl = finalPicPath
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected server error occurred." }, JsonRequestBehavior.AllowGet);
            }
        }

        //Add Favorite
        public JsonResult AddFavorite(string mangaID)
        {
            if (Session["userID"] == null) return Json(new { message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
            int registrationID = (int)Session["userID"];

            try
            {
                using (var db = new MangaLabContext())
                {
                    
                    var existingFavorite = db.tbl_favorites.FirstOrDefault(jv =>
                        jv.registrationID == registrationID &&
                        jv.mangaID == mangaID);

                    //Favorite Checker
                    //Will not delete favorite entry
                    if (existingFavorite != null)
                    {
                        existingFavorite.isArchived = 0;
                        existingFavorite.dateSaved = DateTime.Now;
                    }
                    else
                    {
                        var newFavorite = new tblFavoritesModel()
                        {
                            registrationID = registrationID,
                            mangaID = mangaID,
                            dateSaved = DateTime.Now,
                            isArchived = 0
                        };
                        db.tbl_favorites.Add(newFavorite);
                    }

                    db.SaveChanges();
                    return Json(new { message = "success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error adding favorite." }, JsonRequestBehavior.AllowGet);
            }
        }

        //FOR ACCOUNT PAGE
        public ActionResult ArchiveFavoriteItem(int favoriteID)
        {
            try
            {
                if (Session["userID"] == null)
                {
                    return Json(new { success = false, message = "Unauthorized." });
                }
                int registrationID = (int)Session["userID"]; 

                using (var db = new MangaLabContext())
                {
                    var favoriteItem = db.tbl_favorites.FirstOrDefault(f => f.favoriteID == favoriteID);

                    //Validation by kuya Gemini
                    if (favoriteItem == null)
                    {
                        return Json(new { success = false, message = "Item not found." });
                    }

                    if (favoriteItem.registrationID != registrationID)
                    {
                        return Json(new { success = false, message = "Unauthorized." });
                    }

                    favoriteItem.isArchived = 1;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Favorite archived." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        //FOR MANGA PAGE 
        public ActionResult ArchiveFavoriteByMangaID(string mangaID)
        {
            try
            {
                if (Session["userID"] == null) return Json(new { success = false, message = "Unauthorized." });
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {
                    var favoriteItem = db.tbl_favorites.FirstOrDefault(f =>
                        f.registrationID == registrationID &&
                        f.mangaID == mangaID
                    );

                    if (favoriteItem == null)
                    {
                        return Json(new { success = true, message = "Item not found, but already un-favorited." });
                    }

                    favoriteItem.isArchived = 1;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Favorite archived." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred." });
            }
        }
        

        public JsonResult IsFavorite(string mangaID)
        {
            try
            {
                if (Session["userID"] == null) return Json(new { isFavorite = false }, JsonRequestBehavior.AllowGet);
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {
                    bool exists = db.tbl_favorites.Any(jv =>
                        jv.registrationID == registrationID &&
                        jv.mangaID == mangaID &&
                        jv.isArchived == 0 
                    );

                    return Json(new { isFavorite = exists }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { isFavorite = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ChangeEmail(string currentPassword, string newEmail)
        {
            try
            {

                if (Session["userID"] == null) return Json(new { success = false, message = "Unauthorized." });
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {
                    var userData = db.tbl_registrations.FirstOrDefault(u => u.registrationID == registrationID);

                    
                    if (userData == null || !Crypto.VerifyHashedPassword(userData.password, currentPassword))
                    {
                        return Json(new { success = false, message = "Incorrect password." });
                    }

                    bool emailExists = db.tbl_registrations.Any(u =>
                        u.email == newEmail &&
                        u.registrationID != registrationID
                    );

                    if (emailExists)
                    {
                        return Json(new { success = false, message = "This email is already in use." });
                    }

                    userData.email = newEmail;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Email updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Changing Email Error" });
            }
        }


        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {

                if (Session["userID"] == null) return Json(new { success = false, message = "Unauthorized." });
                int registrationID = (int)Session["userID"];

                if (newPassword != confirmPassword)
                {
                    return Json(new { success = false, message = "New passwords do not match." });
                }

                using (var db = new MangaLabContext())
                {
                    var userData = db.tbl_registrations.FirstOrDefault(u => u.registrationID == registrationID);

                    
                    if (userData == null || !Crypto.VerifyHashedPassword(userData.password, currentPassword))
                    {
                        return Json(new { success = false, message = "Incorrect current password." });
                    }

                    string newhashedPassword = Crypto.HashPassword(newPassword);

                    userData.password = newhashedPassword;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Password updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }

        public JsonResult GetAccountAge(int? registrationID)
        {
            try
            {
                if (registrationID == null)
                {
                    return Json(new { ageDisplay = "N/A" }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new MangaLabContext())
                {
                    var getData = db.tbl_registrations.FirstOrDefault(x => x.registrationID == registrationID.Value);

                    if (getData != null)
                    {
                        TimeSpan accountAge = DateTime.Now - getData.createdAt;

                        var result = new
                        {
                            ageDisplay = $"{Math.Floor(accountAge.TotalDays)} days old"
                        };

                        return Json(result, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { ageDisplay = "User not found" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { ageDisplay = "Error" }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetTotalReadCount(int registrationID)
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    var count = db.tbl_history
                                  .Where(x => x.registrationID == registrationID)
                                  .Select(x => x.mangaID)
                                  .Distinct()
                                  .Count();

                    return Json(new { totalCount = count }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { totalCount = 0 }, JsonRequestBehavior.AllowGet);
            }
        }


        //#################################################### ACCOUNT ####################################################




        //#################################################### CHARTS AND TABLES ####################################################

        public async Task<ActionResult> GetTopFavoriteMangaStats()
        {
            try
            {
                if (Session["role"] == null || Session["role"].ToString() != "Admin")
                {
                    return Json(new { success = false, message = "Unauthorized." }, JsonRequestBehavior.AllowGet);
                }


                using (var db = new MangaLabContext())
                {
                    var topMangaStats = db.tbl_favorites
                        .Where(f => f.isArchived == 0)
                        .GroupBy(f => f.mangaID)
                        .Select(g => new {
                            MangaID = g.Key,
                            FavoriteCount = g.Count()
                        })
                        .OrderByDescending(x => x.FavoriteCount)
                        .Take(5)
                        .ToList();

                    var topMangaLabels = new List<string>(); 
                    var topMangaData = new List<int>();    
                    var genreCounts = new Dictionary<string, int>(); 

                    if (topMangaStats.Any())
                    {
                        var baseUrl = "https://api.mangadex.org/manga";
                        var idParams = string.Join("", topMangaStats.Select(s => $"&ids[]={s.MangaID}"));
                        var finalUrl = $"{baseUrl}?{idParams.Substring(1)}";

                        var mangaApiResponse = await GetMangaDexApiAsync(finalUrl);

                        if (mangaApiResponse is ContentResult mangaContent)
                        {
                            var mangaData = JObject.Parse(mangaContent.Content);
                            var mangaList = mangaData["data"].AsEnumerable();

                            foreach (var stat in topMangaStats)
                            {
                                var mangaDetail = mangaList.FirstOrDefault(m => m["id"].ToString() == stat.MangaID);
                                if (mangaDetail != null)
                                {
                                    string title = mangaDetail["attributes"]["title"]["en"]?.ToString() ?? mangaDetail["attributes"]["title"].First.ToString();
                                    topMangaLabels.Add(title);
                                    topMangaData.Add(stat.FavoriteCount);

                                    foreach (var tag in mangaDetail["attributes"]["tags"])
                                    {
                                        if (tag["attributes"]["group"]?.ToString() == "genre")
                                        {
                                            string genreName = tag["attributes"]["name"]["en"]?.ToString() ?? "Unknown";
                                            if (genreCounts.ContainsKey(genreName))
                                            {
                                                genreCounts[genreName]++; 
                                            }
                                            else
                                            {
                                                genreCounts[genreName] = 1; 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return Json(new
                    {
                        success = true,
                        topMangaLabels = topMangaLabels,
                        topMangaData = topMangaData,
                        topGenreLabels = genreCounts.Keys.ToList(),
                        topGenreData = genreCounts.Values.ToList()
                    }, JsonRequestBehavior.AllowGet);
                } 
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetTopReadMangaStats()
        {
            try
            {
                if (Session["role"] == null || Session["role"].ToString() != "Admin")
                {
                    return Json(new { success = false, message = "Unauthorized." }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new MangaLabContext())
                {


                    var topReadStats = db.tbl_history
                        .GroupBy(h => h.mangaID)
                        .Select(g => new {
                            MangaID = g.Key,
                            ReadCount = g.Count() 
                        })
                        .OrderByDescending(x => x.ReadCount)
                        .Take(5)
                        .ToList();


                    var labels = new List<string>();
                    var data = new List<int>();

                    if (topReadStats.Any())
                    {
                        var baseUrl = "https://api.mangadex.org/manga";
                        var idParams = string.Join("", topReadStats.Select(s => $"&ids[]={s.MangaID}"));
                        var finalUrl = $"{baseUrl}?{idParams.Substring(1)}";

                        var mangaApiResponse = await GetMangaDexApiAsync(finalUrl);

                        if (mangaApiResponse is ContentResult mangaContent)
                        {
                            var mangaData = JObject.Parse(mangaContent.Content);
                            var mangaList = mangaData["data"].AsEnumerable();

                            foreach (var stat in topReadStats)
                            {
                                var mangaDetail = mangaList.FirstOrDefault(m => m["id"].ToString() == stat.MangaID);
                                if (mangaDetail != null)
                                {
                                    string title = mangaDetail["attributes"]["title"]["en"]?.ToString() ?? mangaDetail["attributes"]["title"].First.ToString();

                                    labels.Add(title);
                                    data.Add(stat.ReadCount);
                                }
                            }
                        }
                    }
                    return Json(new { success = true, labels = labels, data = data }, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public async Task<ActionResult> GetNewUsers()
        {
            try
            {
                if (Session["role"] == null || Session["role"].ToString() != "Admin")
                {
                    return Json(new { success = false, message = "Unauthorized." }, JsonRequestBehavior.AllowGet);
                }

                var thirtyDaysAgo = DateTime.Now.Date.AddDays(-30);
                List<DateTime> registrationDates;

                using (var db = new MangaLabContext())
                {
                    
                    registrationDates = await db.tbl_registrations
                        .Where(user => user.createdAt >= thirtyDaysAgo)
                        .Select(user => user.createdAt)
                        .ToListAsync();
                }

                var signupStats = registrationDates
                    .GroupBy(date => date.Date)
                    .ToDictionary(group => group.Key, group => group.Count());

               

                var days = Enumerable.Range(0, 30)
                    .Select(i => DateTime.Now.Date.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var labels = new List<string>();
                var data = new List<int>();

                foreach (var day in days)
                {
                    labels.Add(day.ToString("MMM dd"));

                    if (signupStats.ContainsKey(day))
                    {
                        data.Add(signupStats[day]);
                    }
                    else
                    {
                        data.Add(0);
                    }
                }

                return Json(new { success = true, labels = labels, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetDailyLoginStats()
        {
            try
            {
                if (Session["role"] == null || Session["role"].ToString() != "Admin")
                {
                    return Json(new { success = false, message = "Unauthorized." }, JsonRequestBehavior.AllowGet);
                }


                var thirtyDaysAgo = DateTime.Now.Date.AddDays(-30);
                List<DateTime> loginDates;

                using (var db = new MangaLabContext())
                {
                    loginDates = await db.tbl_logs
                        .Where(log => log.action == "User logged In" && log.logDate >= thirtyDaysAgo)
                        .Select(log => log.logDate)
                        .ToListAsync();
                }

                var loginStats = loginDates
                    .GroupBy(date => date.Date) 
                    .ToDictionary(group => group.Key, group => group.Count());

                var allDays = Enumerable.Range(0, 30)
                    .Select(i => DateTime.Now.Date.AddDays(-i))
                    .OrderBy(d => d) 
                    .ToList();

                var labels = new List<string>();
                var data = new List<int>();

                foreach (var day in allDays)
                {
                    labels.Add(day.ToString("MMM dd")); 

                    if (loginStats.ContainsKey(day))
                    {
                        data.Add(loginStats[day]); 
                    }
                    else
                    {
                        data.Add(0); 
                    }
                }

                return Json(new { success = true, labels = labels, data = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetAccountList()
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    var allAccountsQuery = (from r in db.tbl_registrations
                                            join ro in db.tbl_roles on r.roleID equals ro.roleID
                                            select new
                                            {
                                                r.registrationID,
                                                r.username,
                                                r.isArchived, 
                                                roleDescription = ro.roleDescription,
                                                roleID = r.roleID,
                                                r.email,      
                                                r.createdAt,   
                                                r.updatedAt,
                                            }).ToList();

                    var activeAccounts = allAccountsQuery.Where(a => a.isArchived == 0).ToList();

                    var archivedAccounts = allAccountsQuery.Where(a => a.isArchived == 1).ToList();

                    return Json(new
                    {
                        success = true,
                        activeAccounts = activeAccounts, 
                        archivedAccounts = archivedAccounts 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        // Change Role Dropdown
        public JsonResult GetRoleList()
        {
            using (var db = new MangaLabContext())
            {
                var roles = db.tbl_roles.Select(x => new { x.roleID, x.roleDescription }).ToList();
                return Json(new { success = true, data = roles }, JsonRequestBehavior.AllowGet);
            }
        }

        //Update Role
        public JsonResult UpdateUserRole(int registrationID, int newRoleID)
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    var user = db.tbl_registrations.Find(registrationID);
                    if (user != null)
                    {
                        user.roleID = newRoleID;
                        user.updatedAt = DateTime.Now; 
                        db.SaveChanges();
                        return Json(new { success = true, message = "Role Updated!" });
                    }
                    return Json(new { success = false, message = "User not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        public JsonResult GetLogsList()
        {
            try
            {
                using (var db = new MangaLabContext())
                {
                    var getData = (from x in db.tbl_registrations
                                   join y in db.tbl_logs on x.registrationID equals y.registrationID
                                   orderby y.logDate descending
                                   select new
                                   {
                                       username = x.username,
                                       y.action,
                                       y.logDate
                                   }).ToList();


                    if (getData.Any())
                    {
                        return Json(new { success = true, data = getData }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "No Logs Found" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }



        public JsonResult UpdateAccountStatus(int registrationID, int newStatus) 
        {
            try
            {
                if (Session["userID"] == null)
                {
                    return Json(new { success = false, message = "Unauthorized" }, JsonRequestBehavior.AllowGet);
                }

                using (var db = new MangaLabContext())
                {
                    var user = db.tbl_registrations.FirstOrDefault(u => u.registrationID == registrationID);

                    if (user == null)
                    {
                        return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
                    }

                    user.isArchived = newStatus;

                    db.SaveChanges();

                    string statusAction = (newStatus == 1) ? "archived" : "unarchived";

                    return Json(new
                    {
                        success = true,
                        message = $"Account {user.username} {statusAction} successfully." 
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //#################################################### CHARTS AND TABLES ####################################################








        //#################################################### API CALLS ####################################################

        // 200 Success
        // 502 MangaDex is down / bad gateway
        // 500 mali ko/ server error/ internal fail
        // return await or yung await command is to wait for the HTTP GET (kkunin from mangadex)
        // won't render my friggin app useless when waiting for the request


        //Shared Http Client
        private static readonly HttpClient _client;

        //Constructor
        static MangaLabController()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "MangaLab/1.0");

        }

        //###### POPULATE PAGES CALLS ###### 

        public async Task<ActionResult> GetRecentlyAddedList() //Recently Added 
        {
            //ANTI-BOLD
            var contentFilter = "&contentRating[]=safe";
            //URL ni mangadex
            //limit to 15
            //latest uploads
            //include covers
            var url = "https://api.mangadex.org/manga?limit=15&order[latestUploadedChapter]=desc&includes[]=cover_art" + contentFilter;
            return await GetMangaDexApiAsync(url);
        }

        public async Task<ActionResult> GetMangaList(string genre, string status, string order)
        {
            var genreTagMap = new Dictionary<string, string>
            {
                { "Action", "391b0423-d847-456f-aff0-8b0cfc03066b" },
                { "Comedy", "4d32cc48-9f00-4cca-9b5a-a839f0764984" },
                { "Romance", "423e2eae-a7a2-4a8b-ac03-a8351462d71d" },
                { "Mystery", "ee968100-4191-4968-93d3-f82d72be7e46" },
                { "Horror", "cdad7e68-1419-4192-91ab-27731c290c8a" },
                { "Sci-Fi", "256c8bd9-4904-4360-91f4-27ae9d05d0e4" },
                { "Fantasy", "cdc58593-87dd-415e-bbc0-2ec27bf404cc" }
            };

            var contentFilter = "&contentRating[]=safe";
            var sortOrder = !string.IsNullOrEmpty(order) ? order : "order[followedCount]=desc";

            var baseUrl = $"https://api.mangadex.org/manga?limit=40&includes[]=cover_art{contentFilter}";

            baseUrl += $"&{sortOrder}";

            if (!string.IsNullOrEmpty(status))
            {
                
                if (status == "One-Shot")
                {
                    baseUrl += "&includedTags[]=0234a31e-a729-4e28-9d6a-3f87c4966b9e";
                }
                else
                {
                    baseUrl += $"&status[]={status.ToLower()}";
                }
            }

            if (!string.IsNullOrEmpty(genre) && genreTagMap.TryGetValue(genre, out string tagId))
            {
                baseUrl += $"&includedTags[]={tagId}";
            }

            return await GetMangaDexApiAsync(baseUrl);
        }

        public async Task<ActionResult> SearchMangaApi(string query) 
        {
            var contentFilter = "&contentRating[]=safe";
            var encodedQuery = Uri.EscapeDataString(query); 

            var sortByMostFollowed = "&order[followedCount]=desc";
            var url = $"https://api.mangadex.org/manga?limit=20&title={encodedQuery}&includes[]=cover_art{contentFilter}{sortByMostFollowed}";

            return await GetMangaDexApiAsync(url);
        }


        //###### POPULATE MANGA DETAILS ######

        public async Task<ActionResult> GetMangaDetails(string mangaId) 
        {
            if (string.IsNullOrEmpty(mangaId))
            {
                return new HttpStatusCodeResult(400, "Manga ID cannot be empty.");
            }

            try
            {
                var detailsUrl = $"https://api.mangadex.org/manga/{mangaId}?includes[]=cover_art&includes[]=author";
                var statsUrl = $"https://api.mangadex.org/statistics/manga/{mangaId}";

                var detailsTask = _client.GetAsync(detailsUrl);
                var statsTask = _client.GetAsync(statsUrl);
                await Task.WhenAll(detailsTask, statsTask);

                var detailsResponse = await detailsTask;
                if (!detailsResponse.IsSuccessStatusCode)
                {
                    return new HttpStatusCodeResult(502, "Failed to Get core manga details from MangaDex API.");
                }

                var detailsObject = JObject.Parse(await detailsResponse.Content.ReadAsStringAsync());

                var mangaData = detailsObject["data"];

                var statsResponse = await statsTask;
                if (statsResponse.IsSuccessStatusCode)
                {
                    var statsObject = JObject.Parse(await statsResponse.Content.ReadAsStringAsync());
                    if (mangaData != null && statsObject["statistics"]?[mangaId] != null)
                    {
                        mangaData["statistics"] = statsObject["statistics"][mangaId];
                    }
                }

                var allChapters = new JArray();
                int limit = 500; 

                var baseFeedUrl = $"https://api.mangadex.org/manga/{mangaId}/feed?limit={limit}&order[chapter]=desc";

              
                var firstFeedUrl = $"{baseFeedUrl}&offset=0";
                var firstFeedResponse = await _client.GetAsync(firstFeedUrl);

                if (firstFeedResponse.IsSuccessStatusCode)
                {
                    var feedObject = JObject.Parse(await firstFeedResponse.Content.ReadAsStringAsync());
                    var firstPageChapters = (JArray)feedObject["data"];
                    if (firstPageChapters != null)
                    {
                        allChapters.Merge(firstPageChapters);
                    }

                    int totalChapters = feedObject["total"]?.Value<int>() ?? 0;

                    if (totalChapters > limit)
                    {
                        var chapterTasks = new List<Task<HttpResponseMessage>>();
                        for (int offset = limit; offset < totalChapters; offset += limit)
                        {
                            var nextFeedUrl = $"{baseFeedUrl}&offset={offset}";
                            chapterTasks.Add(_client.GetAsync(nextFeedUrl));
                        }

                        await Task.WhenAll(chapterTasks);

                        foreach (var task in chapterTasks)
                        {
                            var response = await task;
                            if (response.IsSuccessStatusCode)
                            {
                                var pageObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                                var chaptersFromPage = (JArray)pageObject["data"];
                                if (chaptersFromPage != null)
                                {
                                    allChapters.Merge(chaptersFromPage);
                                }
                            }
                        }
                    }
                }

                if (mangaData != null)
                {
                    mangaData["chapters"] = allChapters;
                }

                return Content(mangaData.ToString(), "application/json");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"An internal server error occurred: {ex.Message}, {ex.InnerException}, {ex.StackTrace} ");
            }
        }

        public async Task<ActionResult> GetSimilarManga(string currentMangaId, List<string> genreIds) 
        {
            
            if (genreIds == null || !genreIds.Any())
            {
                return Content("[]", "application/json"); 
            }

            var contentFilter = "&contentRating[]=safe";
            var baseUrl = $"https://api.mangadex.org/manga?limit=6&includes[]=cover_art&order[followedCount]=desc{contentFilter}";

            var genreParams = string.Join("", genreIds.Select(id => $"&includedTags[]={id}"));
            var finalUrl = baseUrl + genreParams;

            try
            {
                var response = await _client.GetAsync(finalUrl);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var resultObject = JObject.Parse(jsonString);

                    var mangaList = resultObject["data"].AsEnumerable()
                        .Where(m => m["id"].ToString() != currentMangaId)
                        .ToList();

                    return Content(new JArray(mangaList).ToString(), "application/json");
                }

                return Content("[]", "application/json");
            }
            catch (Exception)
            {
                return Content("[]", "application/json");
            }
        }


        //###### POPULATE READING PAGE ######

        public async Task<ActionResult> GetReadingPages(string mangaId, string chapterId) 
        {
            if (string.IsNullOrEmpty(chapterId) || string.IsNullOrEmpty(mangaId))
            {
                return new HttpStatusCodeResult(400, "Manga ID and Chapter ID cannot be empty.");
            }

            try
            {
                var pagesUrl = $"https://api.mangadex.org/at-home/server/{chapterId}";
                var pagesResponseTask = _client.GetAsync(pagesUrl);

                var feedUrl = $"https://api.mangadex.org/manga/{mangaId}/feed?limit=500&order[chapter]=desc";
                var feedResponseTask = _client.GetAsync(feedUrl);

                var detailsUrl = $"https://api.mangadex.org/manga/{mangaId}";
                var detailsResponseTask = _client.GetAsync(detailsUrl);

                await Task.WhenAll(pagesResponseTask, feedResponseTask, detailsResponseTask);

                var pagesResponse = await pagesResponseTask;
                var feedResponse = await feedResponseTask;
                var detailsResponse = await detailsResponseTask;

                if (!pagesResponse.IsSuccessStatusCode)
                {
                    return new HttpStatusCodeResult(502, "Failed to Get chapter pages.");
                }

                var result = new JObject();
                result["pageData"] = JObject.Parse(await pagesResponse.Content.ReadAsStringAsync());

                if (feedResponse.IsSuccessStatusCode)
                {
                    result["allChapters"] = JObject.Parse(await feedResponse.Content.ReadAsStringAsync())["data"];
                }

                if (detailsResponse.IsSuccessStatusCode)
                {
                    result["mangaData"] = JObject.Parse(await detailsResponse.Content.ReadAsStringAsync())["data"];
                }

                return Content(result.ToString(), "application/json");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        public async Task<ActionResult> GetFavoriteManga()
        {
            try
            {
                if (Session["userID"] == null)
                {
                    return new HttpStatusCodeResult(401, "Unauthorized");
                }
                int registrationID = (int)Session["userID"]; 

                List<tblFavoritesModel> favorites;
                using (var db = new MangaLabContext())
                {
                    favorites = db.tbl_favorites
                        .Where(f => f.registrationID == registrationID && f.isArchived == 0)
                        .ToList();
                }



                if (!favorites.Any())
                {
                    return Content("[]", "application/json");
                }

                var mangaIds = favorites.Select(f => f.mangaID).ToList();
                var baseUrl = "https://api.mangadex.org/manga?includes[]=cover_art";
                var idParams = string.Join("", mangaIds.Select(id => $"&ids[]={id}"));
                var finalUrl = baseUrl + idParams;

                var mangaApiResponse = await GetMangaDexApiAsync(finalUrl);

                if (mangaApiResponse is ContentResult mangaContent)
                {
                    var jsonString = mangaContent.Content;
                    var mangaData = JObject.Parse(jsonString);
                    var mangaList = mangaData["data"].AsEnumerable();

                    var combinedResult = new List<object>();
                    foreach (var favItem in favorites)
                    {
                        var mangaDetail = mangaList.FirstOrDefault(m => m["id"].ToString() == favItem.mangaID);
                        if (mangaDetail != null)
                        {
                            combinedResult.Add(new
                            {
                                favoriteID = favItem.favoriteID, 
                                manga = mangaDetail 
                            });
                        }
                    }


                    return Content(JsonConvert.SerializeObject(combinedResult), "application/json");
                }
                else
                {
                    return Content(new JObject(new JProperty("data", new JArray())).ToString(), "application/json");
                }
            }
            catch (Exception ex)
            {
                return Content(new JObject(new JProperty("data", new JArray())).ToString(), "application/json");
            }
        }

        public async Task<ActionResult> GetRecommendedMangaByGenre(string genreId)
        {
            if (Session["userID"] == null) return Content("[]", "application/json");
            int registrationID = (int)Session["userID"];

            if (string.IsNullOrEmpty(genreId) || registrationID == 0)
            {
                return Content(new JObject(new JProperty("data", new JArray())).ToString(), "application/json");
            }

            try
            {
                List<string> excludedMangaIds;
                using (var db = new MangaLabContext())
                {
                    var favoriteMangaIds = db.tbl_favorites
                        .Where(f => f.registrationID == registrationID)
                        .Select(f => f.mangaID);

                    var historyMangaIds = db.tbl_history
                        .Where(h => h.registrationID == registrationID)
                        .Select(h => h.mangaID);

                    excludedMangaIds = favoriteMangaIds.Concat(historyMangaIds).Distinct().ToList();
                }

                var baseUrl = $"https://api.mangadex.org/manga?limit=30&includes[]=cover_art&order[followedCount]=desc&contentRating[]=safe";
                var finalUrl = baseUrl + $"&includedTags[]={genreId}";

                var mangaApiResponse = await GetMangaDexApiAsync(finalUrl);

                if (mangaApiResponse is ContentResult mangaContent)
                {
                    var jsonString = mangaContent.Content;
                    var mangaData = JObject.Parse(jsonString);
                    var allManga = mangaData["data"].AsEnumerable();

                    var filteredManga = allManga
                        .Where(m => !excludedMangaIds.Contains(m["id"].ToString()))
                        .Take(10) 
                        .ToList();

                    var result = new JObject(
                        new JProperty("data", new JArray(filteredManga))
                    );

                    return Content(result.ToString(), "application/json");
                }
                else
                {
                    return Content(new JObject(new JProperty("data", new JArray())).ToString(), "application/json");
                }
            }
            catch (Exception ex)
            {
                return Content(new JObject(new JProperty("data", new JArray())).ToString(), "application/json");
            }
        }

        public ActionResult UpdateReadingHistory(string mangaID, string chapterID)
        {

            if (Session["userID"] == null) return new HttpStatusCodeResult(401, "Unauthorized");
            int registrationID = (int)Session["userID"];

            if (registrationID == 0 || string.IsNullOrEmpty(mangaID) || string.IsNullOrEmpty(chapterID))
            {
                return new HttpStatusCodeResult(400, "Invalid data");
            }

            try
            {
                using (var db = new MangaLabContext())
                {
                    var existingEntry = db.tbl_history
                        .FirstOrDefault(h => h.registrationID == registrationID && h.mangaID == mangaID);

                    if (existingEntry != null)
                    {
                        
                        existingEntry.isArchived = 0;
                        existingEntry.chapterID = chapterID;
                        existingEntry.lastReadAt = DateTime.Now;
                    }
                    else
                    {
                        var newEntry = new tblHistoryModel
                        {
                            registrationID = registrationID,
                            mangaID = mangaID,
                            chapterID = chapterID,
                            isArchived = 0,
                            lastReadAt = DateTime.Now
                        };

                        db.tbl_history.Add(newEntry);
                    }

                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        public async Task<ActionResult> GetContinueReading()
        {
            try
            {
                if (Session["userID"] == null) return Content("[]", "application/json");
                int registrationID = (int)Session["userID"];

                List<tblHistoryModel> history;
                using (var db = new MangaLabContext())
                {
                    history = db.tbl_history
                    .Where(h => h.registrationID == registrationID &&
                                h.isArchived == 0)
                    .OrderByDescending(h => h.lastReadAt)
                    .Take(10)
                    .ToList();
                }

                if (!history.Any())
                {
                    return Content("[]", "application/json");
                }

                var mangaIds = history.Select(h => h.mangaID).ToList();
                var baseUrl = "https://api.mangadex.org/manga?includes[]=cover_art";
                var idParams = string.Join("", mangaIds.Select(id => $"&ids[]={id}"));
                var finalUrl = baseUrl + idParams;

                var mangaApiResponse = await GetMangaDexApiAsync(finalUrl);

                if (mangaApiResponse is ContentResult mangaContent)
                {
                    var jsonString = mangaContent.Content;
                    var mangaData = JObject.Parse(jsonString);
                    var mangaList = mangaData["data"].AsEnumerable();

                    var combinedResult = new List<object>();
                    foreach (var historyItem in history)
                    {
                        var mangaDetail = mangaList.FirstOrDefault(m => m["id"].ToString() == historyItem.mangaID);
                        if (mangaDetail != null)
                        {
                            combinedResult.Add(new
                            {
                                historyID = historyItem.historyID,
                                manga = mangaDetail,
                                chapterId = historyItem.chapterID,
                                lastReadAt = historyItem.lastReadAt
                            });
                        }
                    }

                    return Content(JsonConvert.SerializeObject(combinedResult), "application/json");
                }
                else
                {
                    return Content("[]", "application/json");
                }
            }   
            catch (Exception ex)
            {
                return Content("[]", "application/json");
            }
        }

        public ActionResult ArchiveHistoryItem(int historyID)
        {
            try
            {
                if (Session["userID"] == null) return Json(new { success = false, message = "Unauthorized." });
                int registrationID = (int)Session["userID"];

                using (var db = new MangaLabContext())
                {
                    var historyItem = db.tbl_history.FirstOrDefault(h => h.historyID == historyID);

                    if (historyItem == null)
                    {
                        return Json(new { success = false, message = "Item not found." });
                    }

                    if (historyItem.registrationID != registrationID)
                    {
                        return Json(new { success = false, message = "Unauthorized." });
                    }

                    historyItem.isArchived = 1;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Item archived." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error" });
            }
        }



        //###### REUSABLE LOGIC ######
        private async Task<ActionResult> GetMangaDexApiAsync(string url)
        {
            try
            {
                // Since reuseable, hinihintay niya yung url builders
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    //returns json
                    return Content(jsonString, "application/json");
                }

                

                return new HttpStatusCodeResult(502, $"The external MangaDex API failed with status code: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, $"An internal server error occurred: {ex.Message}");
            }
        }

    }
}