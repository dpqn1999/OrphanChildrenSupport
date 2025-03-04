﻿using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using OrphanChildrenSupport.Tools.HttpContextExtensions;

namespace OrphanChildrenSupport.Tools
{
    public class HttpContextHelper : IHttpContextHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentUser()
        {
            try
            {
                return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
