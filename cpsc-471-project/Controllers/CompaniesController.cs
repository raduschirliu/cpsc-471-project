﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cpsc_471_project.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.AspNetCore.Authorization;

namespace cpsc_471_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly JobHunterDBContext _context;

        public CompaniesController(JobHunterDBContext context)
        {
            _context = context;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetCompany()
        {
            var companies = await _context.Companies.ToListAsync();

            // NOTE: the select function here is not querying anything
            // it is simply converting the values to another format
            // i.e. the functional programming map function is named Select in C#
            return companies.Select(x => CompanyToDTO(x)).ToList();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDTO>> GetCompany(long id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
            {
                return NotFound();
            }

            return CompanyToDTO(company);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(long id, CompanyDTO companyDTO)
        {
            Company sanitizedCompany = DTOToCompany(companyDTO);
            if (id != sanitizedCompany.CompanyId)
            {
                return BadRequest();
            }

            bool newCompany;
            if (!CompanyExists(id))
            {
                newCompany = true;
                _context.Companies.Add(sanitizedCompany);
            }
            else
            {
                newCompany = false;
                _context.Entry(sanitizedCompany).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (newCompany)
            {
                return CreatedAtAction("PutCompany", new { id = sanitizedCompany.CompanyId }, companyDTO);
            }
            else
            {
                return AcceptedAtAction("PutCompany", new { id = sanitizedCompany.CompanyId }, companyDTO);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CompanyDTO>> PostCompany(CompanyDTO companyDTO)
        {
            Company sanitizedCompany = DTOToCompany(companyDTO);
            _context.Companies.Add(sanitizedCompany);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostCompany", new { id = sanitizedCompany.CompanyId }, sanitizedCompany);
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<CompanyDTO>> DeleteCompany(long id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return CompanyToDTO(company);
        }

        private bool CompanyExists(long id)
        {
            return _context.Companies.Any(e => e.CompanyId == id);
        }

        private static CompanyDTO CompanyToDTO(Company company) =>
            new CompanyDTO
            {
                CompanyId = company.CompanyId,
                Size = company.Size,
                Name = company.Name,
                Description = company.Description,
                Industry = company.Industry,
                AdminId = company.AdminId
            };

        private static Company DTOToCompany(CompanyDTO companyDTO) =>
            new Company
            {
                CompanyId = companyDTO.CompanyId,
                Size = companyDTO.Size,
                Name = companyDTO.Name,
                Description = companyDTO.Description,
                Industry = companyDTO.Industry,
                AdminId = companyDTO.AdminId,
                Admin = null
            };
    }
}
