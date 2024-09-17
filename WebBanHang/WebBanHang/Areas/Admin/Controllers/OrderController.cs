using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Repository;

namespace WebBanHang.Areas.Admin.Controllers
{
	[Area("Admin")]
    [Authorize(Roles = "Admin, Master")]
    public class OrderController : Controller
    {
		private readonly DataContext _dataContext;
		public OrderController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
		}
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
			return View(DetailsOrder);
		}
		[HttpPost]
		[Route("UpdateOrder")]
		public async Task<IActionResult> UpdateOrder(string ordercode, int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

			if (order == null)
			{
				return NotFound();
			}

			order.Status = status;

			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new
				{
					success = true,
					message = "Order status updated successfully"
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, "Error updating");
			}
		}

    }
}
