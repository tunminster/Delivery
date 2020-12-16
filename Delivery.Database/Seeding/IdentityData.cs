using System;
using System.Linq;
using System.Threading.Tasks;
using Delivery.Database.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Delivery.Database.Seeding
{
    public class IdentityData
    {
        private readonly ApplicationDbContext applicationDbContext;
        public IdentityData(ApplicationDbContext applicationDbContext)
        {
            this.applicationDbContext = applicationDbContext;
        }
        public void Initialize()
        {
            string[] roles = new string[] { "Administrator", "User", "Customer" };
            
            foreach (string role in roles)
            {
                var roleStore = new RoleStore<IdentityRole>(applicationDbContext);

                if (!applicationDbContext.Roles.Any(r => r.Name == role))
                {
                    // ReSharper disable once VSTHRD110
                    applicationDbContext.Roles.Add(new IdentityRole
                        {Name = role, NormalizedName = role, ConcurrencyStamp = Guid.NewGuid().ToString()});
                    applicationDbContext.SaveChanges();
                }
            }
        }
    }
}