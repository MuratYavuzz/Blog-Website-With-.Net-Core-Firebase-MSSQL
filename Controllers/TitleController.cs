using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YENISOZLUK.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.Config;

namespace YENISOZLUK.Controllers
{
    public class TitleController : Controller
    {
        public int title_id;
        public int last_int;

         IFirebaseConfig config = new FirebaseConfig{
            AuthSecret="khMRMHn8RT2JATmDyMLubxAjirblqRvUpF7gAbzl",
            BasePath="https://yenisozlukdb.firebaseio.com/"
        };
        IFirebaseClient client;
        public int user_userid;
        List<Title> titlelist = new List<Title>();

        private readonly EntriesContext _context;
        public TitleController(EntriesContext context)
        {
            _context = context;
        }
       public async Task<IActionResult> Index()
        {
            client = new FireSharp.FirebaseClient(config);
            string key = HttpContext.Session.GetString("Key");
            ViewBag.UserState = HttpContext.Session.GetString("Key");
            if(key=="1")
            {
            ViewBag.UserSession = HttpContext.Session.GetString("Ses_Name");
            }
            SqlConnection connect = new SqlConnection();
            connect.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SozlukDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
             connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TitleID FROM Titles" , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    int id=reader.GetInt32 (0);
                    FirebaseResponse response =client.Get("Basliklar/" + id );
                    Title title = response.ResultAs<Title>();
                    titlelist.Add(title);
                    ViewBag.List = titlelist;   
                }
                }
                connect.Close();
                 connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TOP 1 * FROM Titles order by TitleID desc " , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    last_int=reader.GetInt32(0);
                }
                }
                connect.Close();
                int counter =0;
                while(counter< 1){
                counter++;
                last_int++;
                FirebaseResponse response =client.Get("Basliklar/" + last_int );
                Title title = response.ResultAs<Title>();
                if(title != null)
                {
                    titlelist.Add(title);
                    connect.Open();
                    using (SqlCommand command = new SqlCommand("Insert into Titles(Title_Text,Title_UserID,Title_Date) VALUES(@title,@userid,@date)", connect))
                {
                    command.Parameters.AddWithValue("title",title.Title_Text);
                    command.Parameters.AddWithValue("userid",title.Title_UserID);
                    command.Parameters.AddWithValue("date", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                    command.ExecuteNonQuery();
                }
                connect.Close();
                }
                }

            return View(await _context.Titles.ToListAsync());
        }
        public IActionResult Create()
        {
            ViewBag.UserSession = HttpContext.Session.GetString("Ses_Name");
            ViewBag.UserState = HttpContext.Session.GetString ("Key");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Title title)
        {
            ViewBag.UserSession = HttpContext.Session.GetString("Ses_Name");
            string user_username = HttpContext.Session.GetString("Ses_Name");
            int user_id = Convert.ToInt32(HttpContext.Session.GetInt32("Ses_Id"));
            SqlConnection connect = new SqlConnection();
            connect.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SozlukDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            if (ModelState.IsValid)
            {
                connect.Open();
                using (SqlCommand command = new SqlCommand("Insert into Titles(Title_Text,Title_UserID,Title_Date) VALUES(@title,@userid,@date)", connect))
                {
                    command.Parameters.AddWithValue("title",title.Title_Text);
                    command.Parameters.AddWithValue("userid",user_id);
                    command.Parameters.AddWithValue("date", DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                    command.ExecuteNonQuery();
                }
                connect.Close();
                _context.Add(title);
                 connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TOP 1 TitleID FROM Titles ORDER BY TitleID DESC" , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    title_id=reader.GetInt32 (0);
                }
                }
                connect.Close();

            var created_title = new Title
            {
                TitleID=title_id,
                Entry_Number=0,
                Title_Text=title.Title_Text,
                Title_UserID=user_id,
                Title_Date=(DateTime.Now.ToString ("dddd, dd MMMM yyyy"))
            };
            client = new FireSharp.FirebaseClient(config);
            SetResponse response =await client.SetAsync("Basliklar/"+title_id,created_title);
            Title result = response.ResultAs<Title>();
            ViewBag.Collection=response;
            }
            Response.Redirect("Index");
            return View(title);
        }
        public IActionResult AddEntry()
        {
            return View();
        }

    }
}