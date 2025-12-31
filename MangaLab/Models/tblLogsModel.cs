using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblLogsModel
    {
        public int logID { get; set; }
        public int registrationID { get; set; }
        public string action { get; set; }
        public DateTime logDate { get; set; }
    }
}