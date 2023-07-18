using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoorWeb.Data;
using BooksOnDoorWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db):base(db)
        {
                _db = db;
        }
        public void save()
        {
            _db.SaveChanges();
        }

        public void update(Category category)
        {
            _db.Update(category);
        }
    }
}
