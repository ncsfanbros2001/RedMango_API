using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using Stripe;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/Payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private readonly IConfiguration _configuration;
        private API_Response _response;

        public PaymentController(IConfiguration configuration, DatabaseContext db)
        {
            _db = db;
            _configuration = configuration;
            _response = new API_Response();
        }

        [HttpPost]
        public async Task<ActionResult<API_Response>> MakePayment(string userId)
        {
            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).ThenInclude(u => u.MenuItem)
                .FirstOrDefault(u => u.UserId == userId);

            if (shoppingCart == null || shoppingCart.CartItems == null || shoppingCart.CartItems.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            #region Create Payment Intent

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);

            PaymentIntentCreateOptions options = new() 
            {
                Amount = (int)(shoppingCart.CartTotal * 100),
                Currency = "usd",
                PaymentMethodTypes = new List<string>
                {
                    "card"
                }
            };
            PaymentIntentService service = new();
            PaymentIntent response = service.Create(options);
            shoppingCart.StripePaymentIntentId = response.Id;
            shoppingCart.ClientSecret = response.ClientSecret;

            #endregion

            _response.StatusCode = HttpStatusCode.OK;
            _response.Result = shoppingCart;
            return Ok(_response);
        }
    }
}
