using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace requestsApp
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Console App!");

            while (true)
            {
                Console.Write("Enter a command (type 'help' for available commands): ");
                string command = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(command))
                {
                    Console.WriteLine("Please enter a valid command.");
                    continue;
                }

                string[] commandParts = command.Split(' ');

                switch (commandParts[0].ToLower())
                {
                    case "fetch":
                        if (commandParts.Length > 1 && commandParts[1].ToLower() == "users")
                        {
                            FetchUsers();
                        }
                        else
                        {
                            Console.WriteLine("Invalid fetch command. Type 'help' for available commands.");
                        }
                        break;
                    case "view":
                        if (commandParts.Length > 2 && commandParts[1].ToLower() == "user")
                        {
                            if (int.TryParse(commandParts[2], out int userId))
                            {
                                ViewUser(userId);
                            }
                            else
                            {
                                Console.WriteLine("Invalid user ID. Please enter a valid integer ID.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid view command. Type 'help' for available commands.");
                        }
                        break;
                    case "create":
                        int userIndex = Array.IndexOf(commandParts, "user");
                        int nameIndex = Array.IndexOf(commandParts, "name", userIndex + 1);
                        int jobIndex = Array.IndexOf(commandParts, "job", nameIndex + 1);

                        if (userIndex != -1 && nameIndex != -1 && jobIndex != -1 && jobIndex < commandParts.Length - 1)
                        {
                            string fullName = GetFullName(commandParts, nameIndex + 1, jobIndex - nameIndex - 1);
                            string jobPosition = GetJobPosition(commandParts, jobIndex + 1);
                            CreateUser(fullName, jobPosition);
                        }
                        else
                        {
                            Console.WriteLine("Invalid create command. Type 'help' for available commands.");
                        }
                        break;
                    case "update":
                        string flag = (commandParts.Length > 2 && commandParts[2].StartsWith("-")) ? commandParts[2].ToLower() : "";

                        int updateUserIndex = Array.IndexOf(commandParts, "user");
                        int updateNameIndex = Array.IndexOf(commandParts, "name", updateUserIndex + 1);
                        int updateJobIndex = Array.IndexOf(commandParts, "job", updateNameIndex + 1);

                        if (updateUserIndex != -1 && updateNameIndex != -1 && updateJobIndex != -1 && updateJobIndex < commandParts.Length - 1)
                        {
                            int userId;

                            if (int.TryParse(commandParts[updateUserIndex + 1], out userId))
                            {
                                string fullName = GetFullName(commandParts, updateNameIndex + 1, updateJobIndex - updateNameIndex - 1);
                                string jobPosition = GetJobPosition(commandParts, updateJobIndex + 1);

                                UpdateUser(userId, fullName, jobPosition, flag);
                            }
                            else
                            {
                                Console.WriteLine("Invalid user ID. Please enter a valid integer ID.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid update command. Type 'help' for available commands.");
                        }
                        break;
                    case "delete":
                        if (commandParts.Length == 3 && commandParts[1].ToLower() == "user")
                        {
                            int userId;

                            if (int.TryParse(commandParts[2], out userId))
                            {
                                bool success = DeleteUser(userId);

                                if (success)
                                {
                                    Console.WriteLine($"User with ID {userId} deleted successfully!");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to delete user with ID {userId}. Please try again.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Invalid user ID. Please enter a valid integer ID.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid delete command. Type 'help' for available commands.");
                        }
                        break;
                    case "help":
                        DisplayHelp();
                        break;
                    case "exit":
                        Console.WriteLine("Exiting the program. Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid command. Type 'help' for available commands.");
                        break;
                }
            }


        }




        static void FetchUsers()
        {
            // Make the API request
            string apiUrl = "https://reqres.in/api/users?page=2&per_page=6";
            string jsonResponse = MakeApiRequest(apiUrl);

            // Parse the JSON using JObject
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Access the list of people
            List<Person> people = jsonObject["data"].ToObject<List<Person>>();

            // Display information
            foreach (Person person in people)
            {
                Console.WriteLine($"ID: {person.Id}");
                Console.WriteLine($"Email: {person.Email}");
                Console.WriteLine($"Name: {person.FirstName} {person.LastName}");
                Console.WriteLine($"Avatar: {person.Avatar}");
                Console.WriteLine();
            }
        }


        static void ViewUser(int userId)
        {
            // Make the API request
            string apiUrl = $"https://reqres.in/api/users/{userId}";

            try
            {
                string jsonResponse = MakeApiRequest(apiUrl);

                // Check if the response is null or empty
                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    // Parse the JSON using JObject
                    JObject jsonObject = JObject.Parse(jsonResponse);

                    // Check if the user exists
                    if (jsonObject["data"] != null)
                    {
                        // Access the user information
                        Person user = jsonObject["data"].ToObject<Person>();

                        // Display user information
                        Console.WriteLine($"ID: {user.Id}");
                        Console.WriteLine($"Email: {user.Email}");
                        Console.WriteLine($"Name: {user.FirstName} {user.LastName}");
                        Console.WriteLine($"Avatar: {user.Avatar}");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"User with ID {userId} not found.");
                    }
                }
                else
                {
                    Console.WriteLine("Empty or null response received. Please try again.");
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"User with ID {userId} not found.");
                }
                else
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }



        static void CreateUser(string fullName, string jobPosition)
        {
            // Prepare the JSON data for the API request
            //string jsonData = $"{{\"name\": \"{fullName}\", \"job\": \"{jobPosition}\"}}";

            User user = new User() { Name = fullName, Job =  jobPosition };
            string jsonData = JsonConvert.SerializeObject(user);

            // Make the API request to create a new user
            string apiUrl = "https://reqres.in/api/users";
            string jsonResponse = MakeApiRequest(apiUrl, "POST", jsonData);

            // Parse the JSON using JObject
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Check if the user was created successfully
            if (jsonObject["id"] != null)
            {
                // Access the created user information
                int userId = jsonObject["id"].ToObject<int>();
                string createdAt = jsonObject["createdAt"].ToObject<string>();

                // Display user creation information
                Console.WriteLine($"User created successfully!");
                Console.WriteLine($"ID: {userId}");
                Console.WriteLine($"Full Name: {fullName}");
                Console.WriteLine($"Job Position: {jobPosition}");
                Console.WriteLine($"Created At: {createdAt}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Failed to create user. Please try again.");
            }
        }

        static string GetFullName(string[] commandParts, int startIndex, int length)
        {
            return string.Join(" ", commandParts.Skip(startIndex).Take(length));
        }

        static string GetJobPosition(string[] commandParts, int startIndex)
        {
            return string.Join(" ", commandParts.Skip(startIndex));
        }


        static void UpdateUser(int userId, string fullName, string jobPosition, string flag)
        {
            // Prepare the JSON data for the API request
            string jsonData = $"{{\"name\": \"{fullName}\", \"job\": \"{jobPosition}\"}}";

            // Determine the HTTP method based on the flag
            string httpMethod = (flag == "-put") ? "PUT" : "PATCH";

            // Make the API request to update the user
            string apiUrl = $"https://reqres.in/api/users/{userId}";
            string jsonResponse = MakeApiRequest(apiUrl, httpMethod, jsonData);

            // Parse the JSON using JObject
            JObject jsonObject = JObject.Parse(jsonResponse);

            // Check if the user was updated successfully
            if (jsonObject["updatedAt"] != null)
            {
                string updatedAt = jsonObject["updatedAt"].ToObject<string>();

                // Display user update information
                Console.WriteLine($"User with ID {userId} updated successfully!");
                Console.WriteLine($"Updated At: {updatedAt}");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine($"Failed to update user with ID {userId}. Please try again.");
            }
        }


        static bool DeleteUser(int userId)
        {
            // Make the DELETE request to delete the user
            string apiUrl = $"https://reqres.in/api/users/{userId}";
            string response = MakeApiRequest(apiUrl, "DELETE");

            // Check if the delete operation was successful (status code 204)
            return response == "204 No Content";
        }



        static void DisplayHelp()
        {
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  - fetch users           : Fetches and displays user data.");
            Console.WriteLine("  - view user <id>        : Displays information about a specific user.");
            Console.WriteLine("  - create user name <full name> job <job position> : ");
            Console.WriteLine("                           Creates a new user with the specified <full name> and <job position>.");
            Console.WriteLine("  - update user <flag> <id> name <full name> job <job position> : Updates a user's information.");
            Console.WriteLine("                           Flags: -put (for PUT request), no flag (for PATCH request).");
            Console.WriteLine("  - delete user <id>      : Deletes a user with the specified ID.");
            Console.WriteLine("  - help                  : Displays available commands.");
            Console.WriteLine("  - exit                  : Exits the program.");
        }


        static string MakeApiRequest(string apiUrl, string method = "GET", string jsonData = null)
        {
            try
            {
                WebRequest request = WebRequest.Create(apiUrl);
                request.Method = method;

                if ((method.ToUpper() == "POST" || method.ToUpper() == "PUT" || method.ToUpper() == "PATCH") && !string.IsNullOrWhiteSpace(jsonData))
                {
                    // Set content type for POST, PUT, and PATCH requests
                    request.ContentType = "application/json";

                    // Write JSON data to the request stream
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(jsonData);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }

                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string jsonResponse = reader.ReadToEnd();
                reader.Close();
                response.Close();

                return jsonResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
    }
}
