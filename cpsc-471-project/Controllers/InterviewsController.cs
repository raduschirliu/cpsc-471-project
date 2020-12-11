﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpsc_471_project.Authentication;
using cpsc_471_project.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cpsc_471_project.Controllers
{
    [Route("api/interviews")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly JobHunterDBContext _context;

        public InterviewsController(JobHunterDBContext context, UserManager<User> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        // GET: api/interviews
        // Returns all interviews for a user
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InterviewDTO>>> GetInterviews()
        {
            User user = await userManager.FindByIdAsync(User.Identity.Name);
            IList<string> roles = await userManager.GetRolesAsync(user);
            List<Interview> interviews = new List<Interview>();

            if (roles.Contains(UserRoles.Admin))
            {
                // For admins, return all interviews on the platform
                interviews = await _context.Interviews.ToListAsync();
            }
            else if (roles.Contains(UserRoles.Recruiter))
            {
                // For recruiters, return all interviews for their company
                var query = from interview in _context.Interviews
                            join app in _context.Applications on interview.ApplicationId equals app.ApplicationId
                            join recruiter in _context.Recruiters on user.Id equals recruiter.UserId
                            join job in _context.JobPosts on recruiter.CompanyId equals job.CompanyId
                            where app.JobId == job.JobPostId
                            select interview;

                interviews = await query.ToListAsync();
            }
            else
            {
                // For users, return all interviews that are for one of their applications
                var query = from interview in _context.Interviews
                            join application in _context.Applications on interview.ApplicationId equals application.ApplicationId
                            join resume in _context.Resumes on application.ResumeId equals resume.ResumeId
                            where resume.CandidateId == user.Id
                            select interview;

                interviews = await query.ToListAsync();
            }

            return interviews.Select(interview => InterviewToDTO(interview)).ToList();
        }

        // GET: api/interviews/{applicationId}
        // Returns single interview for a user
        [Authorize]
        [HttpGet("{applicationId}")]
        public async Task<ActionResult<InterviewDTO>> GetInterview(long applicationId)
        {
            User user = await userManager.FindByIdAsync(User.Identity.Name);
            IList<string> roles = await userManager.GetRolesAsync(user);
            Interview result = null;

            if (roles.Contains(UserRoles.Admin))
            {
                // For admins, return any interview on the platform
                result = await _context.Interviews.FindAsync(applicationId);
            }
            else if (roles.Contains(UserRoles.Recruiter))
            {
                // For recruiters, return interviews for their company
                var query = from interview in _context.Interviews
                            join app in _context.Applications on interview.ApplicationId equals app.ApplicationId
                            join recruiter in _context.Recruiters on user.Id equals recruiter.UserId
                            join job in _context.JobPosts on recruiter.CompanyId equals job.CompanyId
                            where interview.ApplicationId == applicationId && app.JobId == job.JobPostId
                            select interview;

                result = await query.FirstOrDefaultAsync();
            }
            else
            {
                // For users, return interviews that are for one of their applications
                var query = from interview in _context.Interviews
                            join application in _context.Applications on interview.ApplicationId equals application.ApplicationId
                            join resume in _context.Resumes on application.ResumeId equals resume.ResumeId
                            where resume.CandidateId == user.Id && interview.ApplicationId == applicationId
                            select interview;

                result = await query.FirstOrDefaultAsync();
            }

            if (result == null)
            {
                return NotFound();
            }

            return InterviewToDTO(result);
        }

        // DELETE: api/interviews/{applicationId}
        // Deletes an interview for an application
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Recruiter)]
        [HttpDelete("{applicationId}")]
        public async Task<ActionResult> DeleteInterview(long applicationId)
        {
            User user = await userManager.FindByIdAsync(User.Identity.Name);
            Interview interview = null;

            if (await userManager.IsInRoleAsync(user, UserRoles.Admin))
            {
                // Admin can delete any interview
                interview = await _context.Interviews.FindAsync(applicationId);
            }
            else
            {
                // Recruiter can only delete their own interviews
                var query = from qInterview in _context.Interviews
                            join app in _context.Applications on qInterview.ApplicationId equals app.ApplicationId
                            join recruiter in _context.Recruiters on user.Id equals recruiter.UserId
                            join job in _context.JobPosts on recruiter.CompanyId equals job.CompanyId
                            where qInterview.ApplicationId == applicationId
                            select qInterview;

                interview = await query.FirstOrDefaultAsync();
            }

            if (interview == null)
            {
                return NotFound();
            }

            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();

            return Ok(InterviewToDTO(interview));
        }

        // POST: api/interviews
        // Creates a new interview for the current recruiter
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Recruiter)]
        [HttpPost]
        public async Task<ActionResult<InterviewDTO>> PostInterview(InterviewDTO interviewDTO)
        {
            User user = await userManager.FindByIdAsync(User.Identity.Name);
            Interview interview = null;

            if (await userManager.IsInRoleAsync(user, UserRoles.Admin))
            {
                // Admin can create interview or any recruiter on any job
                interview = DTOToInterview(interviewDTO);
            }
            else
            {
                // Recruiter can only create interviews for themselves and for their own jobs
                var query = from job in _context.JobPosts
                            join application in _context.Applications on job.JobPostId equals application.JobId
                            join recruiter in _context.Recruiters on user.Id equals recruiter.UserId
                            where application.ApplicationId == interviewDTO.ApplicationId && job.CompanyId == recruiter.CompanyId
                            select job;

                JobPost jobPost = await query.FirstOrDefaultAsync();

                if (jobPost == null)
                {
                    return Unauthorized("Cannot create an interview for a job that you either do not own, or does not exist");
                }

                interview = DTOToInterview(interviewDTO);
                interview.RecruiterId = user.Id;
            }

            _context.Interviews.Add(interview);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostInterview", new { interview.ApplicationId }, InterviewToDTO(interview));
        }

        // PATCH: api/interviews/{applicationId}
        // Updates an existing interview
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Recruiter)]
        [HttpPatch("{applicationId}")]
        public async Task<ActionResult<InterviewDTO>> PatchInterview(long applicationId, InterviewDTO interviewDTO)
        {
            if (applicationId != interviewDTO.ApplicationId)
            {
                return BadRequest("applicationId in query params does not match applicationId in body");
            }
            User user = await userManager.FindByIdAsync(User.Identity.Name);
            Interview interview = null;

            if (await userManager.IsInRoleAsync(user, UserRoles.Admin))
            {
                // Admin can update interview or any recruiter on any job
                interview = DTOToInterview(interviewDTO);
            }
            else
            {
                // Recruiter can only update interviews for themselves and for their own jobs
                var query = from job in _context.JobPosts
                            join application in _context.Applications on job.JobPostId equals application.JobId
                            join recruiter in _context.Recruiters on user.Id equals recruiter.UserId
                            where application.ApplicationId == interviewDTO.ApplicationId && job.CompanyId == recruiter.CompanyId
                            select job;

                JobPost jobPost = await query.FirstOrDefaultAsync();

                if (jobPost == null)
                {
                    return Unauthorized("Cannot update an interview for a job that you either do not own, or does not exist");
                }

                interview = DTOToInterview(interviewDTO);
                interview.RecruiterId = user.Id;
            }

            if (interview == null)
            {
                return NotFound();
            }

            _context.Interviews.Update(interview);
            await _context.SaveChangesAsync();

            return Ok(InterviewToDTO(interview));
        }

        public static InterviewDTO InterviewToDTO(Interview interview) =>
            new InterviewDTO()
            {
                ApplicationId = interview.ApplicationId,
                RecruiterId = interview.RecruiterId,
                Date = interview.Date
            };

        public static Interview DTOToInterview(InterviewDTO interviewDTO) =>
            new Interview()
            {
                ApplicationId = interviewDTO.ApplicationId,
                RecruiterId = interviewDTO.RecruiterId,
                Date = interviewDTO.Date
            };
    }
}
