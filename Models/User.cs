using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace YENISOZLUK.Models {
    public class User {
        [Key]
        public int UserID { get; set; }

        [Required]
        [Column (TypeName = "nvarchar(20)")]
        public string Username { get; set; }

        [Required]
        [Column (TypeName = "nvarchar(20)")]
        public string Password { get; set; }

        [Column (TypeName = "nvarchar(MAX)")]
        public string DateTime { get; set; }

        public string Email { get; set; }

         public string Password2 { get; set; }

        public List <string> Entry_Text = new List<string>();
        public List <string> Title_Text = new List<string>();
        public List <int> Title_ID = new List<int>();

    }
}