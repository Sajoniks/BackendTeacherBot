using System.Text.Json.Serialization;

namespace LearnBotServer.Model;

public class Response<T>
{
    [JsonPropertyName("is_ok")] public bool IsOK { get; set; }
    [JsonPropertyName("data")] public T Data { get; set; }
    [JsonPropertyName("response_code")] public int ResponseCode { get; set; }

    public static Response<T> OkResponse(T data)
    {
        return new Response<T>()
        {
            IsOK = true,
            Data = data
        };
    }

    public static Response<T> FailResponse()
    {
        return new Response<T>()
        {
            IsOK = false
        };
    }

}