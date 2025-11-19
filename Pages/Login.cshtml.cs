using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;

public class LoginModel : PageModel
{
    [BindProperty]
    [Required]
    public string Username { get; set; }

    [BindProperty]
    [Required]
    public string Password { get; set; }

    public string ErrorMessage { get; set; }

    public void OnGet()
    {
        // Just show login form
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Validate user against DB
        using var connection = new SqlConnection("Server=.\\SQLEXPRESS; Database=stm; Trusted_Connection=True;");
        var query = "SELECT PasswordHash, UserID FROM Users WHERE UserName = @UserName";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserName", Username);

        connection.Open();
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            var dbPasswordHash = reader.GetString(0);
            var userId = reader.GetInt32(1);

            // NOTE: Here you should verify the password hash properly.
            // For demo assume plain text comparison (replace with secure hash check!)
            if (Password == dbPasswordHash)
            {
                // login success - set session
                HttpContext.Session.SetInt32("UserID", userId);
                HttpContext.Session.SetString("UserName", Username);

                return RedirectToPage("/Index"); // or wherever landing page is
            }
        }

        ErrorMessage = "Invalid username or password";
        return Page();
    }
}
