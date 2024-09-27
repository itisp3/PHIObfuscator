using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace PHiObfuscator.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("[controller]")]
    public class PhiObfuscatorController : ControllerBase
    {
        //Update this string to the file path on the local machine.  Could be updated to pull value from another source (DB table, config file, etc)
        private const string fileLocation = "C:/ObfuscatedFiles/";
        //Created an array of file types in case of expansion into different formats in future
        private readonly string[] fileTypes = { "txt" };

        [HttpPost(Name = "PhiObfuscator")]
        public async Task<IActionResult> PhiObfuscator(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                //Return all results as Ok with a descriptive message to display on the UI.
                //Could convert to error codes that match situation if security is a concern
                return Ok("No file uploaded");
            }

            var fileName = file.FileName.ToLowerInvariant().Split('.');
            var extension = fileName[^1];
            if (!fileTypes.Contains(extension))
            {
                return Ok("Please upload .TXT files only");
            }

            using var stream = new StreamReader(file.OpenReadStream());
            var lines = new List<string>();
            string line;

            while ((line = await stream.ReadLineAsync()) != null)
            {
                    lines.Add(LineObfuscator(line));
            }

            var obfuscatedFileName = $"{fileName[0]}_sanitized.{extension}";
            var obfuscatedFilePath = Path.Combine(fileLocation, obfuscatedFileName);
            try
            {
                await System.IO.File.WriteAllLinesAsync(obfuscatedFilePath, lines);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Ok($"Unable to save file due to not having access to the folder:{ex}");
            }
            catch (DirectoryNotFoundException ex)
            {
                return Ok($"Cannot write file.  Directory Not found:{ex}");
            }
            catch (Exception ex)
            {
                return Ok($"Cannot write file.{ex.Message}");
            }

            return Ok($"{obfuscatedFileName}obfuscated successfully");
        }

        internal string LineObfuscator(string line)
        {
            //Assume colon is standard in all files as seperator of title and data
            var parts = line.Split(':');
            if (parts.Length < 2)
            {
                return line;
            }

            string firstElement = parts[0].Trim();
            string secondElement = parts[1].Trim();
            string obfuscated = $"{firstElement} : [REDACTED]";

            //Check titles for PHI that does not have a standard format.  Can be expanded to include more checks as necessary
            if (firstElement.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0 ||
                firstElement.IndexOf("address", StringComparison.OrdinalIgnoreCase) >= 0 ||
                firstElement.IndexOf("birth", StringComparison.OrdinalIgnoreCase) >= 0 ||
                firstElement.IndexOf("dob", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return obfuscated;
            }

            //Regex patterns for commonly used PHI formats
            var patterns = new List<Regex>
            {
                new Regex(@"^\d{3}-\d{2}-\d{4}$"), // SSN format: XXX-XX-XXXX
                new Regex(@"^\(\d{3}\) \d{3}-\d{4}$"), // Phone number format: (XXX) XXX-XXXX
                new Regex(@"^\d{4}-\d{2}-\d{2}"), // YYYY-MM-DD date format
                new Regex(@"^\d{2}/\d{2}/\d{4}}"), // DD/MM/YYYY date format
                new Regex(@"^\d{2}-\d{2}-\d{4}"), // MM-DD-YYYY date format
                new Regex(@"^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$"), // Email format
                new Regex(@"^MRN"), // Starts with 'MRN'
                new Regex(@"^((25[0-5]|(2[0-4]|1\d|[1-9]|)\d)\.?\b){4}$") // IP address
            };

            foreach (var pattern in patterns)
            {
                if (pattern.IsMatch(secondElement))
                {
                    return obfuscated;
                }
            }

            return line;
        }
    }
}
