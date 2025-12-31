using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblRolesModel
    {

        public int roleID { get; set; }
        public string roleDescription { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated{ get; set; }

    }
}