using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblRegistrationsModel
    {
        public int registrationID { get; set; }

        public int roleID { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int isArchived { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

    }
}