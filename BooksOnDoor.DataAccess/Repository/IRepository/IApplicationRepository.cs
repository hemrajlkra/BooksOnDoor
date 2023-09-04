using BooksOnDoor.Models.Models;
using BooksOnDoorWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.DataAccess.Repository.IRepository
{
    public interface IApplicationRepository : IRepository<ApplicationUser>
    {
    }
}
