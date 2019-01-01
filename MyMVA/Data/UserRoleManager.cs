using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyMVA.Data
{
    public class UserRoleManager
    {
        #region Private Members

        /// <summary>
        /// Factory for creating loggers
        /// </summary>
        private ILoggerFactory _loggerFactory;

        /// <summary>
        /// Logger for logging
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// Provider of Services, holds a collection of DI services 
        /// that are registered with the application
        /// </summary>
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Holds information of our hosting environment
        /// such as Development or Production etc...
        /// </summary>
        private IHostingEnvironment _hostingEnvironment;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="serviceProvider">Provides access to all services </param>
        /// <param name="loggerFactory">Logger Factory creation of loggers</param>
        /// <param name="hostingEnvironment">Provides information on the environment</param>
        public UserRoleManager(IServiceProvider serviceProvider,
                               ILoggerFactory loggerFactory,
                               IHostingEnvironment hostingEnvironment)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _hostingEnvironment = hostingEnvironment;

            //Create a logger with UserRoleManager category name
            _logger = _loggerFactory.CreateLogger<UserRoleManager>();
        }

        /// <summary>
        /// Creates a role as specified by the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<IdentityResult> CreateRole(string name)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                //Create a Role
                var role = new IdentityRole()
                {
                    Name = name
                };

                //Log information
                _logger?.LogInformation($"Successfully added role => {name}.");

                //Create Role
                return await roleManager.CreateAsync(role);
            }
        }

        /// <summary>
        /// Adds user to role
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddUserToRole(string usernameOrEmail, string password, string rolename)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                //Create IdentityUser
                var userToCreate = new IdentityUser()
                {
                    UserName = usernameOrEmail,
                    Email = usernameOrEmail

                };

                var IsEmail = usernameOrEmail.Contains('@');

                //Try finding the user in the database
                var userIdentity = IsEmail ? await userManager.FindByEmailAsync(usernameOrEmail)
                                            :
                                             await userManager.FindByNameAsync(usernameOrEmail);

                if (userIdentity == null)
                {

                    var result = await userManager.CreateAsync(userToCreate, password);

                    if (!result.Succeeded)
                        return result;
                }

                //Create a role
                await CreateRole(rolename);

                _logger.LogInformation($"Successfully added user: {usernameOrEmail} to role: {rolename}");

                //here we tie the new user to the role
                return await userManager.AddToRoleAsync(userToCreate, rolename);
            }
        }

        /// <summary>
        /// Adds user to role
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddClaimToUserAsync(string usernameOrEmail, Claim claimToAdd)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                //Get a ServiceProvider for accessing services
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                //Check if the value passed is email or username
                var IsEmail = usernameOrEmail.Contains('@');

                //Try finding the user in the database
                var userIdentity = IsEmail ? await userManager.FindByEmailAsync(usernameOrEmail)
                                           :
                                             await userManager.FindByNameAsync(usernameOrEmail);

                _logger.LogInformation($"Successfully added claim to user: {usernameOrEmail}");

                //here we tie the new user to the role
                return await userManager.AddClaimAsync(userIdentity, claimToAdd);
            }
        }
    }
}
