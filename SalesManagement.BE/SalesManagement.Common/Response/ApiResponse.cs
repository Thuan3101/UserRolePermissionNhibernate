using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Common.Response
{
    using System.Text.Json.Serialization;

    public class ApiResponseSuccess<T>
    {
        [JsonPropertyOrder(0)]
        public int StatusCode { get; set; }

        [JsonPropertyOrder(1)]
        public bool Success { get; set; }

        [JsonPropertyOrder(2)]
        public string? Message { get; set; }

        [JsonPropertyOrder(3)]
        public T? Data { get; set; }
    }

    public class ApiResponseSuccessPaginated<T> : ApiResponseSuccess<T>
    {
        [JsonPropertyOrder(4)]
        public int TotalCount { get; set; }

        [JsonPropertyOrder(5)]
        public int PageSize { get; set; }

        [JsonPropertyOrder(6)]
        public int CurrentPage { get; set; }
       
    }

    public class ApiResponseError
    {
        public int StatusCode { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public object? Error { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    //public class ApiResponseBooking
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public object Data { get; set; }
    //    public object Error { get; set; }
    //    public Dictionary<string, string[]> Errors { get; set; }
    //    public int StatusCode { get; set; }
    //}
    //public class PaginatedResponse<T>
    //{

    //    public int TotalCount { get; set; }
    //    public int PageSize { get; set; }
    //    public int CurrentPage { get; set; }
    //    public int TotalPages { get; set; }
    //    public List<T> Items { get; set; }

    //    public PaginatedResponse(List<T> items, int totalCount, int pageSize, int currentPage)
    //    {

    //        TotalCount = totalCount;
    //        PageSize = pageSize;
    //        CurrentPage = currentPage;
    //        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    //        Items = items;
    //    }
    //}
}
