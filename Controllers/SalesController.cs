using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndustryConnect_Week5_WebApi.Models;
using IndustryConnect_Week5_WebApi.Dtos;

namespace IndustryConnect_Week5_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IndustryConnectWeek2Context _context;

        public SalesController(IndustryConnectWeek2Context context)
        {
            _context = context;
        }

        // GET: api/Sales 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalesDto>>> GetSales()
        {
            var sales = await _context.Sales
                                       .Include(s => s.Store)
                                       .Include(s  => s.Customer)
                                       .Include(s => s.Product)
                                      .ToListAsync();

            //Manually map each Sale entity to a SalesDto
            
            var salesDto = sales.Select(s => new SalesDto
            {
                StoreName = s.Store.Name,
                CustomerName = $"{s.Customer.FirstName} {s.Customer.LastName}",
                ProductName = s.Product.Name
            }).ToList();
        }

        // GET: api/Sales/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SalesDto>> GetSale(int id)
        {
            var sale = await _context.Sales
                                     .Include(s => s.Store)
                                     .Include(s => s.Customer)
                                     .Include(s => s.Product)
                                     .FirstOrDefaultAsync(s =>s.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            var salesDto = new SalesDto
            {
                StoreName = sale.Store.Name,
                CustomerName = $"{sale.Customer.FirstName} {sale.Customer.LastName}",
                ProductName = sale.Product.Name
            };
            return Ok(salesDto);
        }

        // PUT: api/Sales/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSale(int id, Sale sale)
        {
            if (id != sale.Id)
            {
                return BadRequest();
            }

            _context.Entry(sale).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SaleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Sales
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SalesDto>> PostSale(SalesDto salesDto)
        { 
            //Look up the customer based on the customerName in the Dto
            var store = await _context.Stores.FirstOrDefaultAsync(s => s.Name == salesDto.StoreName);
            if (store == null) return NotFound("Store not found");


            //Look up the product based on the ProductName in the Dto
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Name == salesDto.ProductName);
            if (product == null) return NotFound("Product not found");

            //Look up the customer based on the customer name in the Dto

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.FirstName == salesDto.CustomerName);
            if (customer == null) return NotFound("Customer does not exist");

            //Create the Sale entity
            var sale = new Sale
            {
                StoreId = store.Id,
                ProductId = product.Id  ,
                CustomerId = customer.Id ,
            };

            //Save the sale entity to the database
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            //Return the Dto back as the response

            var createdSalesDto = new SalesDto
            {
                StoreName = salesDto.StoreName,
                CustomerName = $"{customer.FirstName} {customer.LastName}",
                ProductName = product.Name
            };

            return CreatedAtAction("GetSale", new { id = sale.Id }, sale);
        }

        // DELETE: api/Sales/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                return NotFound();
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SaleExists(int id)
        {
            return _context.Sales.Any(e => e.Id == id);
        }
    }
}
