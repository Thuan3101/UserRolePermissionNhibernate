//namespace SalesManagement.Common.Responses
//{
//    public class PaginatedResponse<T>
//    {
       
//        public int TotalCount { get; set; }
//        public int PageSize { get; set; }
//        public int CurrentPage { get; set; }
//        public int TotalPages { get; set; }
//        public List<T> Items { get; set; }

//        public PaginatedResponse(List<T> items, int totalCount, int pageSize, int currentPage)
//        {

//            TotalCount = totalCount;
//            PageSize = pageSize;
//            CurrentPage = currentPage;
//            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
//            Items = items;
//        }
//    }
//}