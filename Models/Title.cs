using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace YENISOZLUK.Models
{
    public class Title
    {
        [Key]
        public int TitleID { get; set; }

        [Display(Name = "Ne hakkında konuşulsun istersin?")]
        [Column(TypeName = "nvarchar(MAX)")]
        public string Title_Text { get; set; }

        [Column(TypeName = "int")]
        public int Title_UserID { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Title_Date { get; set; }

        [Column(TypeName = "int")]
        public int Entry_Number { get; set; }


    }
}
