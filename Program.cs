using System.Text.RegularExpressions;

namespace AspRestApiApp
{
    public class Program
    {
        static List<User> users = new()
        {
            new(){ Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 23 },
            new(){ Id = Guid.NewGuid().ToString(), Name = "Joe", Age = 39 },
            new(){ Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 19 },
            new(){ Id = Guid.NewGuid().ToString(), Name = "Tim", Age = 26 },
        };
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.Run(async (context) =>
            {
                var response = context.Response;
                var request = context.Request;
                var path = request.Path;

                // string exprIntegerId = "^api/users/([0-9]+)$";
                string exprGuId = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

                if(path == "/api/users" && request.Method == "GET")
                {
                    await GetAllUsers(response);
                }
                else if(Regex.IsMatch(path, exprGuId) && request.Method == "GET")
                {
                    string? id = path.Value?.Split("/")[3];
                    await GetUser(response, id);
                }
                else if(path == "/api/users" && request.Method == "PUT")
                {
                    await UpdateUser(response, request);
                }
                else if(path == "/api/users" && request.Method == "POST")
                {
                    await CreateUser(response, request);
                }
                else if(Regex.IsMatch(path, exprGuId) && request.Method == "DELETE")
                {
                    string? id = path.Value?.Split("/")[3];
                    await DeleteUser(response, id);
                }
                else
                {
                    response.ContentType = "text/html; charset=utf-8";
                    await response.SendFileAsync("html/index.html");
                }
            });

            app.Run();
        }

        async static Task GetAllUsers(HttpResponse response)
        {
            await response.WriteAsJsonAsync(users);
        }

        async static Task GetUser(HttpResponse response, string id)
        {
            User user = users.FirstOrDefault((u) => u.Id == id);
            if (user is not null)
                await response.WriteAsJsonAsync(user);
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "User not found"});
            }
        }

        async static Task UpdateUser(HttpResponse response, HttpRequest request)
        {
            try
            {
                User userClient = await request.ReadFromJsonAsync<User>();
                if(userClient is not null)
                {
                    User user = users.FirstOrDefault((u) => u.Id == userClient.Id);
                    if(user is not null)
                    {
                        user.Name = userClient.Name;
                        user.Age = userClient.Age;
                        await response.WriteAsJsonAsync(user);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        await response.WriteAsJsonAsync(new { message = "User not found" });
                    }
                }
                else
                {
                    throw new Exception("incorrect data");
                }
            }
            catch(Exception e)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = e.Message });
            }
        }

        async static Task CreateUser(HttpResponse response, HttpRequest request)
        {
            try
            {
                User userClient = await request.ReadFromJsonAsync<User>();
                if(userClient is not null)
                {
                    userClient.Id = Guid.NewGuid().ToString();
                    users.Add(userClient);
                    await response.WriteAsJsonAsync(userClient);
                }
                else
                {
                    throw new Exception("incorrect data");
                }
            }
            catch(Exception e)
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = e.Message });
            }

        }

        async static Task DeleteUser(HttpResponse response, string id)
        {
            User user = users.FirstOrDefault((u) => u.Id == id);
            if(user is not null)
            {
                users.Remove(user);
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "User not found" });
            }
        }
    }
}