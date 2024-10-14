using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Server;

public class TestRequest(string msg)
{
    private readonly Request? request = Main.FromJson<Request>(msg);
    private string _message = "";

    public string is_request_valid_message()
    {
        //////////////////////////////////////////////////////////
        ///
        /// Error handling for ERROR CODE 4
        ///
        //////////////////////////////////////////////////////////

        var sb = new System.Text.StringBuilder();

        /*   For Method  */

        if (string.IsNullOrEmpty(request?.Method))
        {
            if (sb.Length > 0)
            {
                sb.Append(", missing method");
            }
            else
            {
                sb.Append("4 Bad Request, missing method");
            }
        }

        if (
            request?.Method?.Equals("create") == false
            && request?.Method?.Equals("read") == false
            && request?.Method?.Equals("update") == false
            && request?.Method?.Equals("delete") == false
            && request?.Method?.Equals("echo") == false
        )
        {
            if (sb.Length > 0)
            {
                sb.Append(", illegal method");
            }
            else
            {
                sb.Append("4 Bad Request, illegal method");
            }
        }
        else if (
            request?.Method?.Equals("echo") == false
            && string.IsNullOrEmpty(request?.Body) == true
            && string.IsNullOrEmpty(request?.Path) == true
        )
        {
            if (sb.Length > 0)
            {
                sb.Append(", missing resource");
            }
            else
            {
                sb.Append("4 Bad Request,  missing resource");
            }
        }

        /*    For Date     */

        if (string.IsNullOrEmpty(request?.Date))
        {
            if (sb.Length > 0)
            {
                sb.Append(", missing date");
            }
            else
            {
                sb.Append("4 Bad Request, missing date");
            }
        }

        try
        {
            _ = DateTimeOffset.FromUnixTimeSeconds(long.Parse(request?.Date ?? "0")).ToString();
        }
        catch
        {
            if (sb.Length > 0)
            {
                sb.Append(", illegal date");
            }
            else
            {
                sb.Append("4 Bad Request, illegal date");
            }
        }

        /*    For Body     */

        if (
            request?.Method?.Equals("create") == true
            || request?.Method?.Equals("update") == true
            || request?.Method?.Equals("echo") == true
        )
        {
            if (string.IsNullOrEmpty(request?.Body))
            {
                if (sb.Length > 0)
                {
                    sb.Append(", missing body");
                }
                else
                {
                    sb.Append("4 Bad Request, missing body");
                }
            }
        }

        if (request?.Method?.Equals("update") == true)
        {
            try
            {
                _ = System.Text.Json.JsonDocument.Parse(request?.Body ?? "");
            }
            catch
            {
                if (sb.Length > 0)
                {
                    sb.Append(", illegal body");
                }
                else
                {
                    sb.Append("4 Bad Request, illegal body");
                }
            }
        }

        /*   For Path     */

        if (request?.Path?.StartsWith("/api/categories") == false)
        {
            if (sb.Length > 0)
            {
                sb.Append(", illegal path");
            }
            else
            {
                sb.Append("4 Bad Request, illegal path");
            }
        }
        else if (request?.Method?.Equals("update") == true && request?.Path?.StartsWith("/api/categories/") == false)
        {
            if (sb.Length > 0)
            {
                sb.Append(", illegal path with Method update");
            }
            else
            {
                sb.Append("4 Bad Request, illegal path with Method update");
            }
        }
        else if (request?.Method?.Equals("delete") == true && request?.Path?.StartsWith("/api/categories/") == false)
        {
            if (sb.Length > 0)
            {
                sb.Append(", illegal path with Method delete");
            }
            else
            {
                sb.Append("4 Bad Request, illegal path with Method delete");
            }
        }
        else if (request?.Path?.StartsWith("/api/categories/") == true)
        {
            string[]? values = request?.Path?.Split('/');
            try
            {
                _ = int.Parse(values?[3] ?? "");

                if (request?.Method?.Equals("create") == true)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(", illegal path with Method create");
                    }
                    else
                    {
                        sb.Append("4 Bad Request, illegal path with Method create");
                    }
                }
            }
            catch
            {
                if (sb.Length > 0)
                {
                    sb.Append(", illegal path ID");
                }
                else
                {
                    sb.Append("4 Bad Request, illegal path ID");
                }
            }
        }

        _message = sb.ToString();
        if (string.IsNullOrEmpty(_message))
        {
            _message = "Request is valid";
        }
        return _message;
    }
}
