using System;

namespace OrphanChildrenSupport.DataContracts
{
    public class ApiResponse<T> where T : class
    {
        public ApiResponse()
        {
            IsError = false;
            Message = String.Empty;
            Data = null;
        }

        public bool IsError { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
