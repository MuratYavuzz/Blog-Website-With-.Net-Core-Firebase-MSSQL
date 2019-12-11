using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using YENISOZLUK.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.Config;

namespace YENISOZLUK.Controllers {
    public class UsersController : Controller {
        private readonly EntriesContext _context;
        List<String> datetime = new List<String>();
        public int user_id;

         IFirebaseConfig config = new FirebaseConfig{
            AuthSecret="khMRMHn8RT2JATmDyMLubxAjirblqRvUpF7gAbzl",
            BasePath="https://yenisozlukdb.firebaseio.com/"
        };
        IFirebaseClient client;

        public UsersController (EntriesContext context) {
            _context = context;
        }

        public async Task<IActionResult> Index () {
            return View (await _context.Users.ToListAsync ());
        }
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }
            var user = await _context.Users
                .FirstOrDefaultAsync (m => m.UserID == id);
            if (user == null) {
                return NotFound ();
            }

            return View (user);
        }

        public IActionResult Create () {
            return View ();
        }
        public IActionResult Login () {
            return View ();
        }

        [HttpGet]
        public IActionResult Profile (User user) {
            int user_id = Convert.ToInt32 (HttpContext.Session.GetInt32 ("Ses_Id"));
            SqlConnection connect = new SqlConnection ();
            connect.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SozlukDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";;
            connect.Open ();
            using (SqlCommand command = new SqlCommand ("Select Text,TitleID,UserID,DateTime from Entries Where UserID=" + user_id, connect)) {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    user.Entry_Text.Add (reader.GetString (0));
                    user.Title_ID.Add (reader.GetInt32 (1));
                    datetime.Add(reader.GetString(3));
                }
            }
            ViewBag.Name = HttpContext.Session.GetString("Ses_Name");
            ViewBag.DateTime = datetime;
            connect.Close ();

            for (int i = 0; i < user.Title_ID.Count; i++) {
                connect.Open ();
                using (SqlCommand command_ = new SqlCommand ("Select Title_Text from Titles Where TitleID=" + user.Title_ID[i], connect)) {
                    SqlDataReader reader_ = command_.ExecuteReader ();
                    while (reader_.Read ()) {
                        user.Title_Text.Add (reader_.GetString (0));
                    }
                    connect.Close();
                }
            }
            connect.Close ();

            return View (user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login (User user) {
            ViewBag.LoginError=null;
            if (user.Username != "" && user.Password != "") {
                client = new FireSharp.FirebaseClient(config);
                if(client != null)
                {
                FirebaseResponse response =client.Get("Kullanicilar/" + user.Username );
                User usr = response.ResultAs<User>();
                if(user.Password == usr.Password && user.Username == usr.Username)
                {
                    HttpContext.Session.SetString ("Ses_Name", user.Username);
                    HttpContext.Session.SetString ("Key", "1");
                    HttpContext.Session.SetInt32 ("Ses_Id", usr.UserID);
                    Response.Redirect ("/Title/Index");
                } 
                
                else 
                {
                    ViewBag.LoginError="Kullanıcı Adı Sistemde Kayıtlı Değil!";
                    HttpContext.Session.SetInt32 ("key", -1);
                    Response.Redirect ("/users/login");
                }
                return View();
                }
                
                return View();
            }
            return View();
        }
        public void SignOut (User user) {
            HttpContext.Session.SetString ("Ses_Name", "null");
            HttpContext.Session.SetString ("Key", "null");
            HttpContext.Session.SetInt32 ("Ses_Id", -1);
            Response.Redirect ("../Title/Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind ("UserID,Username,Password,DateTime,Password2,Email")] User user) {

            SqlConnection connect = new SqlConnection ();
            connect.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SozlukDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            if (ModelState.IsValid) {
                connect.Open ();
                using (SqlCommand command = new SqlCommand ("Insert into Users(Username,Password,DateTime,Password2,Email) VALUES(@us_name,@pass,@d_time,@pass2,@e_mail)", connect)) {
                    command.Parameters.AddWithValue ("us_name", user.Username);
                    command.Parameters.AddWithValue ("d_time", DateTime.Now.ToString ());
                    command.Parameters.AddWithValue ("pass", user.Password);
                    command.Parameters.AddWithValue ("pass2", user.Password2);
                    command.Parameters.AddWithValue ("e_mail", user.Email);
                    command.ExecuteNonQuery ();
                }
                connect.Close ();

                connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TOP 1 UserID FROM Users ORDER BY UserID DESC" , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    user_id=reader.GetInt32 (0);
                }
                }
                connect.Close();

            var created_user = new User
            {
                UserID=user_id,
                Username=user.Username,
                Password=user.Password,
                Password2=user.Password2,
                Email=user.Email,
                DateTime=(DateTime.Now.ToString ("dddd, dd MMMM yyyy"))
            };
            client = new FireSharp.FirebaseClient(config);

            SetResponse response =await client.SetAsync("Kullanicilar/"+user.Username,created_user);
            User result = response.ResultAs<User>();

            }
                _context.Add (user);
                HttpContext.Session.SetString ("register", "true");
                ViewBag.UserRegister = HttpContext.Session.GetString ("register");
            return View (user);
        }

    }
}
