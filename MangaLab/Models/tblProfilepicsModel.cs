using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblProfilepicsModel
    {
        public int imageID { get; set; }
        public int registrationID { get; set; }
        public String imagePath { get; set; }

        public String imageFileName { get; set; }

        public DateTime dateCreated { get; set; }

        public DateTime dateUpdated { get; set; }


    }
}