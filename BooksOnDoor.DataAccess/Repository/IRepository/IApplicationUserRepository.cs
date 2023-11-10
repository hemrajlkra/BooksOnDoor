using BooksOnDoor.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository: IRepository<ApplicationUser>
    {
        public void Update(ApplicationUser applicationUser);
    }
}
