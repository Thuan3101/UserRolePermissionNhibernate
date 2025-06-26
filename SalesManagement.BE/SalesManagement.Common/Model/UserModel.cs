﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Model
{
    public class UserModel
    {
        public  int UserId { get; set; }
        public  string UserCode { get; set; }
        public  string UserName { get; set; }
        public  string FirstName { get; set; }
        public  string LastName { get; set; }
        public  string Gender { get; set; }
        public  string Email { get; set; }
        public  DateTime DateOfBirth { get; set; }
        public  string Phone { get; set; }
        public  string Password { get; set; }
        public  string Status { get; set; }
    }
}
