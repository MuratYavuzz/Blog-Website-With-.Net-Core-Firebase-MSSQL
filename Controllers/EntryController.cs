using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YENISOZLUK.Models;
using FireSharp.Interfaces;
using FireSharp.Response;
using FireSharp.Config;

namespace YENISOZLUK.Controllers {
    public class EntryController : Controller {
        public int user_id;
        public int last_int;
        public string title_name;
        public int entry_number;
        public int entry_id;
        private readonly EntriesContext _context;
        List<Entry> entrylist = new List<Entry>();
        IFirebaseClient client;

         IFirebaseConfig config = new FirebaseConfig{
            AuthSecret="khMRMHn8RT2JATmDyMLubxAjirblqRvUpF7gAbzl",
            BasePath="https://yenisozlukdb.firebaseio.com/"
        };
        public EntryController (EntriesContext context) {
            _context = context;
        }
        string constring = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SozlukDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public async Task<IActionResult> Index (int? id) {

            string key = HttpContext.Session.GetString ("Key");
            ViewBag.UserState = HttpContext.Session.GetString ("Key");
            if (key == "1") {
                ViewBag.UserSession = HttpContext.Session.GetString ("Ses_Name");
            }
            ViewBag.TitleID = id;
            client = new FireSharp.FirebaseClient(config);
            SqlConnection connect = new SqlConnection ();
            connect.ConnectionString = constring;
            connect.Open ();
            using (SqlCommand cmd_name = new SqlCommand ("Select Title_Text,Title_Date From Titles Where TitleID=" + id, connect)) {
                SqlDataReader reader = cmd_name.ExecuteReader ();
                while (reader.Read ()) {
                    ViewBag.TitleName = reader.GetString (0);
                    ViewBag.TitleDate = reader.GetString(1);
                }
            }
            connect.Close();
            connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT EntryID FROM Entries Where TitleID=" + id , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    int temp_id=reader.GetInt32 (0);
                    FirebaseResponse response =client.Get("Girdiler/" + temp_id );
                    Entry entry_ = response.ResultAs<Entry>();
                    entrylist.Add(entry_);
                    ViewBag.EntryList = entrylist;   
                }
                }
            connect.Close ();

             connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TOP 1 * FROM Entries order by EntryID desc " , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    last_int=reader.GetInt32(0);
                }
                }
            connect.Close();

            int counter =0;
                while(counter< 3){
                counter++;
                last_int++;
                FirebaseResponse response2 =client.Get("Girdiler/" + last_int );
                Entry entry_ = response2.ResultAs<Entry>();
                if(entry_ != null)
                {

                connect.Open();
                using (SqlCommand command = new SqlCommand ("Insert into Entries(TitleID,UserID,Text,UserName,DateTime) VALUES(@titleid,@userid,@txt,@username,@datetime)", connect)) {
                command.Parameters.AddWithValue ("titleid", entry_.TitleID);
                command.Parameters.AddWithValue ("userid", entry_.UserID);
                command.Parameters.AddWithValue ("txt", entry_.Text);
                command.Parameters.AddWithValue ("username", entry_.UserName);
                command.Parameters.AddWithValue ("datetime", DateTime.Now.ToString ("dddd, dd MMMM yyyy"));
                command.ExecuteNonQuery ();
            }
                connect.Close(); 
                }
                }

            return View (await _context.Entries.ToListAsync ());
        }
        public IActionResult Create (int? id) {
            ViewBag.UserSession = HttpContext.Session.GetString ("Ses_Name");
            ViewBag.UserState = HttpContext.Session.GetString ("Key");
            ViewBag.EditEntry = null;
            SqlConnection connect = new SqlConnection ();
            connect.ConnectionString = constring;
            connect.Open ();
            using (SqlCommand cmd_name = new SqlCommand ("Select Title_Text From Titles Where TitleID=" + id, connect)) {
                SqlDataReader reader = cmd_name.ExecuteReader ();
                while (reader.Read()) {
                    ViewBag.TitleName = reader.GetString (0);
                }
            }
            connect.Close ();
            int user = Convert.ToInt32 (HttpContext.Session.GetInt32 ("Ses_Id"));
            connect.Open ();
            using (SqlCommand cmd_edit = new SqlCommand ("Select Text from Entries Where UserID=" + user + "and TitleID=" + id, connect)) {
                SqlDataReader _reader = cmd_edit.ExecuteReader (); {
                    while (_reader.Read ()) {
                        ViewBag.EditEntry = _reader.GetString (0);
                    }
                }
            }
            connect.Close ();

            return View ();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Title title, Entry entry, int? id) {
            ViewBag.EditEntry=null;
            int url_id = Convert.ToInt32 (id);
            int user_id = Convert.ToInt32 (HttpContext.Session.GetInt32 ("Ses_Id"));
            string user_name = HttpContext.Session.GetString ("Ses_Name");
            HttpContext.Session.SetInt32 ("Title_Id", Convert.ToInt32 (id));
            int user = Convert.ToInt32 (HttpContext.Session.GetInt32 ("Ses_Id"));

            SqlConnection connect = new SqlConnection ();
            connect.ConnectionString = constring;
            connect.Open();
            using (SqlCommand cmd_edit = new SqlCommand ("Select Text from Entries Where UserID=" + user + "and TitleID=" + id, connect)) {
                SqlDataReader _reader = cmd_edit.ExecuteReader (); {
                    while (_reader.Read ()) {
                        ViewBag.EditEntry ="true";
                    }
                }
            }
            connect.Close();
            connect.Open ();
            using (SqlCommand cmd_name = new SqlCommand ("Select Title_Text,Entry_Number From Titles Where TitleID=" + id, connect)) {
                SqlDataReader reader = cmd_name.ExecuteReader ();
                while (reader.Read ()) {
                    entry.TitleName = reader.GetString (0);
                    entry_number = reader.GetInt32(1);
                }
            }
            connect.Close ();
            if(ViewBag.EditEntry == null)
            {
            connect.Open ();
            using (SqlCommand command = new SqlCommand ("Insert into Entries(TitleID,UserID,Text,UserName,DateTime) VALUES(@titleid,@userid,@txt,@username,@datetime)", connect)) {
                command.Parameters.AddWithValue ("titleid", id);
                command.Parameters.AddWithValue ("userid", user_id);
                command.Parameters.AddWithValue ("txt", entry.Text);
                command.Parameters.AddWithValue ("username", user_name);
                command.Parameters.AddWithValue ("datetime", DateTime.Now.ToString ("dddd, dd MMMM yyyy"));
                command.ExecuteNonQuery ();
            }
            connect.Close ();
            connect.Open();
                using (SqlCommand command = new SqlCommand ("SELECT TOP 1 EntryID FROM Entries ORDER BY EntryID DESC" , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    entry_id=reader.GetInt32 (0);
                }
                }
            connect.Close();

            var created_entry = new Entry
            {
                EntryID=entry_id,
                TitleID=Convert.ToInt32(id),
                UserID=user_id,
                Text=entry.Text,
                UserName=user_name,
                DateTime=DateTime.Now.ToString ("dddd, dd MMMM yyyy")
            };
            client = new FireSharp.FirebaseClient(config);

            SetResponse response =await client.SetAsync("Girdiler/"+entry_id,created_entry);
            Entry result = response.ResultAs<Entry>();

            connect.Open();
            using (SqlCommand cmd_number = new SqlCommand("Update Titles SET [Entry_Number] = @num Where TitleID=" + id,connect))
            {
                entry_number++;
                cmd_number.Parameters.AddWithValue("num",entry_number);
                cmd_number.ExecuteNonQuery();
            }
            connect.Close();
            connect.Open();
            using (SqlCommand command = new SqlCommand ("SELECT Title_Text from Titles Where TitleID="+ id , connect)) 
                {
                SqlDataReader reader = command.ExecuteReader ();
                while (reader.Read ()) {
                    title_name=reader.GetString(0);
                }
            }
            connect.Close();
            var update= new Title
            {
                TitleID=Convert.ToInt32(id),
                Entry_Number=entry_number,
                Title_Text=title_name,
                Title_UserID=user_id,
                Title_Date=(DateTime.Now.ToString ("dddd, dd MMMM yyyy"))
            };
            client = new FireSharp.FirebaseClient(config);

            SetResponse res_update =await client.SetAsync("Basliklar/"+id,update);
            Title title_update = response.ResultAs<Title>();


            }
            else{
            connect.Open();
            using (SqlCommand cmd = new SqlCommand ("Update Entries SET [Text] = @txt Where UserID="+ user +"and TitleID="+ id,connect))
            {
                cmd.Parameters.AddWithValue("txt",entry.Text);
                cmd.ExecuteNonQuery();
            }
            connect.Close();
            connect.Open();
            using (SqlCommand command = new SqlCommand ("SELECT EntryID from Entries Where UserID="+ user +"and TitleID="+ id , connect)) 
                {
                    SqlDataReader reader = command.ExecuteReader ();
                    while (reader.Read ()) {
                    entry_id=reader.GetInt32(0);
                }
            }   
            connect.Close();

            var created_entry = new Entry
            {
                EntryID=entry_id,
                TitleID=Convert.ToInt32(id),
                UserID=user_id,
                Text=entry.Text,
                UserName=user_name,
                DateTime=DateTime.Now.ToString ("dddd, dd MMMM yyyy")
            };
            client = new FireSharp.FirebaseClient(config);

            SetResponse response =await client.SetAsync("Girdiler/"+entry_id,created_entry);
            Entry result = response.ResultAs<Entry>();

            }
            return RedirectToAction ("Index", new { id = url_id });
        
    }
    }
}
