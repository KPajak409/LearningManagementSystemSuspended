﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using LMS.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Activities
{
    public class CreateActivityModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        
        public CreateActivityModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Activity Activity { get; set; }
        [EnumDataType(typeof(ActivityType))]
        public ActivityType SelectedActivityType { get; set; }
        [BindProperty]
        public IList<IFormFile> Files { get; set; } = new List<IFormFile>();
        [BindProperty]
        public string FileName { get; set; } = string.Empty;
        [BindProperty]
        public string Error { get; set; }

        public IActionResult OnGet()
        {
            
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(int courseId, int sectionId)
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            if(Activity.StartTime > Activity.EndTime)
            {

                Error = "Start time must be before end time.";
                return Page();
            }
            if (Activity.EndTime < DateTime.Now)
            {

                Error = "End time should be in the future.";
                return Page();
            }
            if (Files.Count > 0)
            {
                var size = Files.Sum(f => f.Length);
                foreach (var formFile in Files)
                {
                    if (formFile.Length > 0)
                    {
                        if(Files.Last() != formFile)
                        {
                            if (Activity.FileNames == null)
                            {
                                Activity.FileNames = string.Empty;
                                Activity.FileNames += formFile.FileName + ' ';
                            }
                            else
                            {
                                Activity.FileNames += formFile.FileName + ' ';
                            }
                        }
                        else
                        {
                            if (Activity.FileNames == null)
                            {
                                Activity.FileNames = string.Empty;
                                Activity.FileNames += formFile.FileName;
                            }
                            else
                            {
                                Activity.FileNames += formFile.FileName;
                            }
                        }
                                               
                    } else if(formFile.Length == 0)
                    {
                        Activity.FileNames = string.Empty;
                    }
                }
            }
             
            var section = _context.Sections
               .Include(s => s.Activities)
               .Include(s => s.Course)
               .FirstOrDefault(s => s.Id == sectionId);
            Activity.Course = section.Course;
            section.Activities.Add(Activity);
            _context.SaveChanges();

            if(Files.Count > 0)
            {
                var filePaths = new List<string>();
                var folderPath = Path.Combine(_environment.ContentRootPath, "Files\\Activities\\" + $"{Activity.Id}\\");
                Directory.CreateDirectory(folderPath);
                foreach (var formFile in Files)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Files\\Activities\\" + $"{Activity.Id}\\", formFile.FileName);
                    filePaths.Add(filePath);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }
                }
            }
            

            return RedirectToPage("../Courses/CourseEditMode", new  { id = courseId});
        }
    }
}
