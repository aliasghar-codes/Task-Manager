using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Skill_Task_Manager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using TaskModel = Skill_Task_Manager.Models.Task;

namespace Skill_Task_Manager.Pages
{
    public class FormModel : PageModel
    {
        private readonly ILogger<FormModel> _logger;

        public FormModel(ILogger<FormModel> logger)
        {
            _logger = logger;
        }

        [BindProperty]
        public TaskModel Task { get; set; }  // Changed alias to just Task for clarity per your model

        public List<TaskModel> Tasks { get; set; }

        // OnGet fetches tasks for current user
        public void OnGet()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            Tasks = new List<TaskModel>();

            using (SqlConnection connection = new SqlConnection("Server=.\\SQLEXPRESS; Database=stm; Trusted_Connection=True;"))
            {
                // Changed SELECT to match columns in FormData table, no Description, DueDate, IsCompleted
                string query = "SELECT FormDataID, TaskName FROM FormData WHERE UserID = @UserID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tasks.Add(new TaskModel
                            {
                                FormDataID = reader.GetInt32(0),
                                TaskName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (SqlConnection connection = new SqlConnection("Server=.\\SQLEXPRESS; Database=stm; Trusted_Connection=True; MultipleActiveResultSets=True"))
            {
                string query = "INSERT INTO FormData (UserID, TaskName) VALUES (@UserID, @TaskName)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int? userId = HttpContext.Session.GetInt32("UserID");

                    command.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaskName", Task.TaskName);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            return RedirectToPage("Index");
        }

        public IActionResult OnPostDelete(int formDataID)
        {
            using (SqlConnection connection = new SqlConnection("Server=.\\SQLEXPRESS; Database=stm; Trusted_Connection=True;"))
            {
                string query = "DELETE FROM FormData WHERE FormDataID = @FormDataID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FormDataID", formDataID);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToPage("Index");
        }
    }
}

namespace Skill_Task_Manager.Models
{
    public class Task
    {
        public int FormDataID { get; set; }  // Changed from Id

        [Required]
        [StringLength(255)]
        public string TaskName { get; set; }  // Changed from Title to match DB
    }
}
