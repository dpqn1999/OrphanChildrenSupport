﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OrphanChildrenSupport.Infrastructure;

namespace OrphanChildrenSupport.Controllers
{
    public class ControllerBase : Controller
    {
        protected ServiceUser ServiceUser { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ControllerContext
                .HttpContext
                .Items
                .TryGetValue(
                    Constants.HttpContextServiceUserItemKey,
                    out object serviceUser);
            ServiceUser = serviceUser as ServiceUser;
            base.OnActionExecuting(context);
        }
    }
}