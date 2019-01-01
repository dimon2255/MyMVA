using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyMVA.Requirements
{
    /// <summary>
    /// Enforces a Canadian claim or Admin role to access a specific resource
    /// </summary>
    public class CanadianRequirement : AuthorizationHandler<CanadianRequirement>, IAuthorizationRequirement
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CanadianRequirement()
        {

        }

        /// <summary>
        /// Handles Requirements for policy
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanadianRequirement requirement)
        {
            //If Admin Succeed
            if (context.User.IsInRole("Admin") ||
                context.User.HasClaim(claim => claim.ValueType == ClaimTypes.Country && claim.Value == "Canada"))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
