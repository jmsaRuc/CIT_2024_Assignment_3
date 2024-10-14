using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server;

public class Main
{
    private readonly int _port;

    private CategoryDb categoryDb = new CategoryDb();

    private Category[]? categories => categoryDb.GetCategories();

    public Main(int port)
    {
        _port = port;
    }

    public void Run()
    {
        var main = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1
        main.Start();

        Console.WriteLine($"Main started on port {_port}");

        /*   Load categories if none exist  */
        categoryDb.loadCategories();
        if (categories?.Length == 0)
        {
            var category = new Category { cid = 1, name = "Beverages" };
            categoryDb.AddCategory(category);
            category = new Category { cid = 2, name = "Condiments" };
            categoryDb.AddCategory(category);
            category = new Category { cid = 3, name = "Confections" };
            categoryDb.AddCategory(category);
            categoryDb.saveCategories();
            Console.WriteLine("Start Categories Loaded: " + ToJson(categories));
        }

        while (true)
        {
            var client = main.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");

            HandleClient(client);
        }
    }

    private void HandleClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            string msg = ReadFromStream(stream);

            Console.WriteLine("Message from client: " + msg);
            Console.WriteLine("Message from client: " + FromJson<Request>(msg)?.Body);

            var testRequest = new TestRequest(msg);
            var request = FromJson<Request>(msg);

            /*   Test if valid request  */

            if (!testRequest.is_request_valid_message().Equals("Request is valid"))
            {
                var respones = new Respones { Status = testRequest.is_request_valid_message() };
                Console.WriteLine("Response to client: " + ToJson(respones));
                var json = ToJson(respones);
                WriteToStream(stream, json);
            }
            /*   For handeling echo metods   */

            else if (request?.Method?.Equals("echo") == true)
            {
                var respones = new Respones { Status = "1", Body = request.Body };
                Console.WriteLine("Response to client: " + ToJson(respones));
                var json = ToJson(respones);
                WriteToStream(stream, json);
            }
            /*   For handeling create metods   */

            else if (request?.Method?.Equals("create") == true)
            {
                var category = new Category
                {
                    cid = categories?.Length + 1 ?? 0,
                    name = request.Body,
                };
                categoryDb.AddCategory(category);
                categoryDb.saveCategories();
                var respones = new Respones { Status = "1", Body = ToJson(category) };
                Console.WriteLine("Response to client: " + ToJson(respones));
                var json = ToJson(respones);
                WriteToStream(stream, json);
            }
            /*   For handeling delete metods   */

            else if (request?.Method?.Equals("delete") == true)
            {
                string[]? values = request?.Path?.Split("/");
                int cid = int.Parse(values?[3] ?? "");
                categoryDb.loadCategories();
                if (categoryDb.GetCategoryCid(cid) == null)
                {
                    var respones = new Respones
                    {
                        Status = "5 Not found, " + request?.Path + "Did not exist found",
                    };
                    Console.WriteLine("Response to client: " + ToJson(respones));
                    var json = ToJson(respones);
                    WriteToStream(stream, json);
                }
                else
                {
                    categoryDb.DeleteCategoryCid(cid);
                    categoryDb.saveCategories();
                    var respones = new Respones { Status = "1 Ok", Body = "Category deleted" };
                    Console.WriteLine("Response to client: " + ToJson(respones));
                    var json = ToJson(respones);
                    WriteToStream(stream, json);
                }
            }
            /*   For handeling read metods   */

            else if (request?.Method?.Equals("read") == true)
            {
                if (request?.Path?.StartsWith("/api/categories/") == true)
                {
                    string[]? values = request?.Path?.Split("/");
                    int cid = int.Parse(values?[3] ?? "");
                    categoryDb.loadCategories();
                    if (categoryDb.GetCategoryCid(cid) == null)
                    {
                        var respones = new Respones
                        {
                            Status = "5 Not found, " + request?.Path + "Did not exist found",
                        };
                        Console.WriteLine("Response to client: " + ToJson(respones));
                        var json = ToJson(respones);
                        WriteToStream(stream, json);
                    }
                    else
                    {
                        var respones = new Respones
                        {
                            Status = "1 Ok",
                            Body = ToJson(categoryDb.GetCategoryCid(cid)),
                        };
                        Console.WriteLine("Response to client: " + ToJson(respones));
                        var json = ToJson(respones);
                        WriteToStream(stream, json);
                    }
                }
                else if (request?.Path?.Equals("/api/categories") == true)
                {
                    categoryDb.loadCategories();
                    var respones = new Respones { Status = "1 Ok", Body = ToJson(categories) };
                    Console.WriteLine("Response to client: " + ToJson(respones));
                    var json = ToJson(respones);
                    WriteToStream(stream, json);
                }

                /*   For handeling update metods   */
            }
            else if (request?.Method?.Equals("update") == true)
            {
                string[]? values = request?.Path?.Split("/");
                int cid = int.Parse(values?[3] ?? "");
                categoryDb.loadCategories();
                if (categoryDb.GetCategoryCid(cid) == null)
                {
                    var respones = new Respones
                    {
                        Status = "5 Not found, " + request?.Path + "Did not exist found",
                    };
                    Console.WriteLine("Response to client: " + ToJson(respones));
                    var json = ToJson(respones);
                    WriteToStream(stream, json);
                }
                else
                {
                    var category = FromJson<Category>(request?.Body ?? "");
                    categoryDb.UpdateCategory(category ?? new Category());
                    categoryDb.saveCategories();
                    var respones = new Respones { Status = "3 Updated", Body = ToJson(category) };
                    Console.WriteLine("Response to client: " + ToJson(respones));
                    var json = ToJson(respones);
                    WriteToStream(stream, json);
                }
            }
        }
        catch { }
    }

    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }

    public static string ToJson<T>(T element)
    {
        return JsonSerializer.Serialize(
            element,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
    }

    public static T? FromJson<T>(string element)
    {
        return JsonSerializer.Deserialize<T>(
            element,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
    }
}
