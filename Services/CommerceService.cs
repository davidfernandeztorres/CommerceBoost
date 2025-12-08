using System;
using CommerceBoost.Data;
using CommerceBoost.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CommerceBoost.Services
{
    public class CommerceService
    {
        private readonly CommerceDbContext _db;

        public CommerceService(CommerceDbContext db)
        {
            _db = db;
        }

        public List<Sale> GetSales()
        {
            return _db.Sales
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Include(s => s.Customer)
                .ToList();
        }

        public List<Product> GetProducts()
        {
            return _db.Products.ToList();
        }

        public List<Customer> GetCustomers()
        {
            return _db.Customers.ToList();
        }

        public void AddSale(Sale sale)
        {
            _db.Sales.Add(sale);
            _db.SaveChanges();
        }

        public void UpdateSales(List<Sale> sales)
        {
            _db.Sales.UpdateRange(sales);
            _db.SaveChanges();
        }

        public void AddProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void AddCustomer(Customer customer)
        {
            _db.Customers.Add(customer);
            _db.SaveChanges();
        }

        public Product? GetProductByCode(string code)
        {
            return _db.Products.FirstOrDefault(p => p.Id.ToString() == code || p.Nombre == code);
        }

        public void UpdateSaleItem(SaleItem item)
        {
            _db.SaleItems.Update(item);
            _db.SaveChanges();
        }

        public void DeleteSaleItem(int saleItemId)
        {
            var item = _db.SaleItems.Find(saleItemId);
            if (item != null)
            {
                _db.SaleItems.Remove(item);
                _db.SaveChanges();
            }
        }
        
        public void DeleteProduct(int productId)
        {
            var product = _db.Products.Find(productId);
            if (product != null)
            {
                _db.Products.Remove(product);
                _db.SaveChanges();
            }
        }
        
        public decimal GetDailyTotal(DateTime date)
        {
            return _db.Sales
                .Where(s => s.Fecha.Date == date.Date)
                .Sum(s => s.Total);
        }
        
        public void UpdateStock(int productId, int quantitySold)
        {
            var product = _db.Products.Find(productId);
            if (product != null)
            {
                product.Stock -= quantitySold;
                _db.SaveChanges();
            }
        }
        
        public void EnsureSchema()
        {
            try
            {
                // Fix missing Discount column in SaleItems
                _db.Database.ExecuteSqlRaw("ALTER TABLE \"SaleItems\" ADD COLUMN IF NOT EXISTS \"Discount\" numeric NOT NULL DEFAULT 0;");
                
                // Add MetodoPago and ZClosed columns to Sales
                _db.Database.ExecuteSqlRaw("ALTER TABLE \"Sales\" ADD COLUMN IF NOT EXISTS \"MetodoPago\" text NOT NULL DEFAULT 'Efectivo';");
                _db.Database.ExecuteSqlRaw("ALTER TABLE \"Sales\" ADD COLUMN IF NOT EXISTS \"ZClosed\" boolean NOT NULL DEFAULT false;");
            }
            catch (Exception ex)
            {
                // Log or handle schema update errors
                System.Diagnostics.Debug.WriteLine($"Schema update error: {ex.Message}");
            }
        }
    }
}
