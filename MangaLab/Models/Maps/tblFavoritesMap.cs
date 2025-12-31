using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblFavoritesMap : EntityTypeConfiguration<tblFavoritesModel>
    {
        public tblFavoritesMap() {
        
            HasKey(i => i.favoriteID);
            ToTable("tbl_favorites");
        
    }
    }
}