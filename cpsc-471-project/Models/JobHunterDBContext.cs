﻿using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpsc_471_project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using cpsc_471_project.Authentication;

namespace cpsc_471_project.Models
{
    public class JobHunterDBContext : IdentityDbContext<User>
    {
        public JobHunterDBContext(DbContextOptions<JobHunterDBContext> options) : base(options) {}

        public DbSet<Company> Companies { get; set; }

        public DbSet<JobPost> JobPosts { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Resume> Resumes { get; set; }
    }
}
