using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MangaLab.Models
{
    public class tblFavoritesModel
    {
        public int favoriteID { get; set; }
        public int registrationID { get; set; }
        public string mangaID { get; set; }

        public int isArchived { get; set; }

        public DateTime dateSaved { get; set; }

    }
}