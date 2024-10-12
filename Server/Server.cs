using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;
    }

    public void Run()
    {
        var server = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1
        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
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
            Console.WriteLine("Message from client: " + FromJson(msg)?.Body);

            var testRequest = new TestRequest(msg);
            var request = FromJson(msg);
            if (!testRequest.is_request_valid_message().Equals("Request is valid"))
            {
                var respones = new Respones { Status = testRequest.is_request_valid_message() };
                Console.WriteLine("Response to client: " + ToJson(respones));
                var json = ToJson(respones);
                WriteToStream(stream, json);
            }
            else if (request?.Method?.Equals("echo") == true) {
                var respones = new Respones { Status = "1", Body = request.Body};
                Console.WriteLine("Response to client: " + ToJson(respones));
                var json = ToJson(respones);
                WriteToStream(stream, json);
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

    public static string ToJson(Respones? respones)
    {
        return JsonSerializer.Serialize(
            respones,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(
            element,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
    }
}
