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
            var prodFromDb = _db.Products.FirstOrDefault(u => u.Id == product.Id);
            //_db.Update(product);
            if (prodFromDb != null)
            {
                prodFromDb.ISBN = product.ISBN;
                prodFromDb.Author = product.Author;
                prodFromDb.Description = product.Description;
                prodFromDb.Price50 = product.Price50;
                prodFromDb.CategoryId = product.CategoryId;
                prodFromDb.ListPrice = product.ListPrice;
                prodFromDb.Price = product.Price;
                prodFromDb.Title = product.Title;
                prodFromDb.Price100 = product.Price100;
                if(prodFromDb.ImageUrl != null)
                {
                    prodFromDb.ImageUrl = product.ImageUrl;
                }



            }
        }
    }
}
