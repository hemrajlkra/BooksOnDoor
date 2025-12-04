using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoorWeb.Data;
using BooksOnDoorWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.DataAccess.Repository
{
    public class UserCommentRepository : Repository<Comment>, IUserCommentRepository
    {
        private ApplicationDbContext _db;
        public UserCommentRepository(ApplicationDbContext db) :base(db)
        {
                _db = db;
        }
        
    }
}
