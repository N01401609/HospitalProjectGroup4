﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//Install  entity framework 6 on Tools > Manage Nuget Packages > Microsoft Entity Framework (ver 6.4)
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using HospitalProjectTeam4.Data;

namespace HospitalProjectTeam4.Models
{
    public class ForumReply
    {
        [Key]
        public int ReplyID { get; set; }

        //Representing the "One" in (One User to Many Replies)
        //Link it to the users table rather than doctors or patients
        public string PatientID { get; set; }
        [ForeignKey("PatientID")]
        public virtual Patient Patient { get; set; }

        //Representing the "One" in (One Post to Many Replies)
        public int PostID { get; set; }
        [ForeignKey("PostID")]

        public virtual ForumPost ForumPost { get; set; }


        public DateTime ReplyDate { get; set; }
        public string ReplyContent { get; set; }

    }
}