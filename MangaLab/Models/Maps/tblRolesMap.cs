using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblRolesMap : EntityTypeConfiguration<tblRolesModel>
    {

        public tblRolesMap()
        {
            HasKey(i => i.roleID);
            ToTable("tbl_roles");
        }

    }
}