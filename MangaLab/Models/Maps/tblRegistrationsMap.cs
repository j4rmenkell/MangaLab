using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblRegistrationsMap : EntityTypeConfiguration<tblRegistrationsModel>
    {

        public tblRegistrationsMap() 
        {
            HasKey(i => i.registrationID);
            ToTable("tbl_registrations");
        }


    }
}