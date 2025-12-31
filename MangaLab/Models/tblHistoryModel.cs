using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblHistoryModel
    {
        public int historyID { get; set; }
        public int registrationID { get; set; }
        public String mangaID { get; set; }
        public String chapterID { get; set; }

        public int isArchived { get; set; }

        public DateTime lastReadAt { get; set; }


    }
}