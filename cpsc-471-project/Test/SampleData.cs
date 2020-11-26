﻿#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using cpsc_471_project.Models;
using Microsoft.EntityFrameworkCore.Internal;


/*
To add sample data, simply add the following line at the top of the document
using cpsc_471_project.Test;

And then copy this code to the UserController.cs file (allongside the other APIs)

#if DEBUG
        // POST: api/Users/SampleData
        [HttpPost("PopulateDB")]
        public async Task<ActionResult<User>> PopulateDB()
        {
            SampleData.AddSampleData(_context);
            return Ok("Sample data has been added if there was no existing data");
        }
#endif

Then, to add the sample data, you simply have to make a POST request to https://localhost:5001/api/Users/PopulateDB using Postman
*/

namespace cpsc_471_project.Test
{
    public class SampleData
    {
        public static void AddSampleData(JobHunterDBContext _context)
        {
            if( !_context.Users.Any() )
            {
                _context.Users.AddRange(SampleUserData());
            }
            if ( !_context.Companies.Any() )
            {
                _context.Companies.AddRange(SampleCompanyData());
            }
            if (!_context.Resumes.Any())
            {
                _context.Resumes.AddRange(SampleResumeData());
            }
            _context.SaveChanges();
        }

        public static List<User> SampleUserData()
        {
            List<User> returnedUsers = new List<User>();

            User testUser1 = new User()
            {
                UserId = 1,
                Role = UserRole.Admin,
                FirstName = "Bob",
                LastName = "Smith",
                Email = "bobsmith12345@jgd098suyfvk23jbfjsdv.com",
                Phone = "555-555-5555",
            };
            returnedUsers.Add(testUser1);

            User testUser2 = new User()
            {
                UserId = 2,
                Role = UserRole.Recruiter,
                FirstName = "Caitlyn",
                LastName = "Brown",
                Email = "caitlynbrown1@jgd098suyfvk23jbfjsdv.com",
                Phone = "444-444-4444",
            };
            returnedUsers.Add(testUser2);

            User testUser3 = new User()
            {
                UserId = 3,
                Role = UserRole.Candidate,
                FirstName = "Evan",
                LastName = "Johnson",
                Email = "evanjohnson@jgd098suyfvk23jbfjsdv.com",
                Phone = "333-333-3333",
            };
            returnedUsers.Add(testUser3);

            return returnedUsers;
        }
        public static List<Company> SampleCompanyData()
        {
            List<Company> returnedCompanies = new List<Company>();

            Company testCompany1 = new Company()
            {
                CompanyId = 1,
                Size = CompanySize.OneToTen,
                Name = "Test Company 1",
                Description = "Test Description 1",
                Industry = "Technology",
                AdminId = 1
            };
            returnedCompanies.Add(testCompany1);

            Company testCompany2 = new Company()
            {
                CompanyId = 2,
                Size = CompanySize.ElevenToFifty,
                Name = "Test Company 2",
                Description = "Test Description 2",
                Industry = "Retail",
                AdminId = 1
            };
            returnedCompanies.Add(testCompany2);

            Company testCompany3 = new Company()
            {
                CompanyId = 3,
                Size = CompanySize.FiftyOneToTwoFifty,
                Name = "Test Company 3",
                Description = "Test Description 3",
                Industry = "Manufacturing",
                AdminId = 1
            };
            returnedCompanies.Add(testCompany3);

            return returnedCompanies;
        }

        public static List<Resume> SampleResumeData()
        {
            List<Resume> resumes = new List<Resume>();

            resumes.Add(new Resume()
            {
                ResumeId = 1,
                Name = "Resume #1",
                CandidateId = 1
            });

            resumes.Add(new Resume()
            {
                ResumeId = 2,
                Name = "Resume #2",
                CandidateId = 1
            });

            resumes.Add(new Resume()
            {
                ResumeId = 3,
                Name = "Gr8 resume",
                CandidateId = 3
            });

            return resumes;
        }
    }
}
#endif