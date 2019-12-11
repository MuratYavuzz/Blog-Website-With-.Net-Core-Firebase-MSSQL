using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace YENISOZLUK.Models
{
    public class Entry
    {
        [Key]
        public int EntryID { get; set; }

        [Column(TypeName ="int")]
        public int TitleID { get; set; }

        [Column(TypeName = "int")]
        public int UserID { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Text { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string TitleName { get; set; }

        public string UserName { get; set; }

        public string DateTime { get; set; }
    }
}
