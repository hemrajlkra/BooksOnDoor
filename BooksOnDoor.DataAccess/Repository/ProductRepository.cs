using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoorWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksOnDoor.DataAccess.Repository
{
    public class ProductRepository:Repository<Product>,IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void update(Product product)
        {
            _db.Update(product);
        }
    }
}
