This is an API to process a file and obfuscate PHI.

Installation instructions:
1. Use the master branch to fork/clone the repo locally
2. Open Visual Studio and Open a Porject using the PHIObfuscator.sln file
3. Install Moq and Xunit through Nuget if not included
4. Alter the value in the PHIObfuscatorController.cs line 13 to a valid file path on the local machine
5. Run the application using the https play button to run as an https API

Running the application should start up a swagger window for testing the API directly, but also exposes the API for the front end.
The API is running on port 7176 by default, but future updates can make this configurable via an environmental based config file.

In order to process the PHI files, some assumptions about the file structure were made:
1. The files would contain one piece of information on each line
2. The line would be a title followed by a value, separated by a colon ':'
3. PHI for a person's name would include the string 'name' in the title (ex. First Name, Last Name, Maiden Name, etc)
4. PHI for a person's address would include the string 'address' in the title (ex. Street Address, City Address, Zip Address)
5. PHI that include dates MAY include the string 'date' or 'dob'
6. Phone numbers would be in the format (XXX) XXX-XXXX
7. Social Security numbers would be in the format XXX-XX-XXXX
8. Dates of Birth would be in one of the following formats : YYYY-MM-DD  | DD/MM/YYYY | MM-DD-YYYY
9. MRN IDs would begin with 'MRN"
10. IP addresses would follow the conventional IPv4 standard

Future improvements:
1. Add a way to update the file path for locally saving files to be outside of the API, maybe read from a DB table or set on a personal profile
2. Expand PHI matches to include string matches on more fields
3. Add functionality to read more files than just .txt files
4. Add a call out to an LLM or other AI application that could take in more complicated file layouts and return standardized files for Obfuscation
5. Add more robust unit testing for covering edges around file permissions
