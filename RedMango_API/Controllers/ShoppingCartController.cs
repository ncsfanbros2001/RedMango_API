using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/ShoppingCart")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        protected API_Response _response;
        private readonly DatabaseContext _db;

        public ShoppingCartController(DatabaseContext db)
        {
            _db = db;
            _response = new();
        }

        [HttpGet]
        public async Task<ActionResult<API_Response>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if (string.IsNullOrEmpty(userId))
                {
                    shoppingCart = new();
                }
                else
                {
                    shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems).ThenInclude(u => u.MenuItem)
                        .FirstOrDefault(u => u.UserId == userId);
                }

                if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count() > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
                }

                _response.Result = shoppingCart;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }

            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<API_Response>> AddOrUpdateItemInCart(string userId, int menuItemId,
            int updateQuantityBy)
        {
            // Shopping cart will have one entry per user id, even if a user has many items in cart.
            // Cart items will have all the items in shopping cart for a user
            // updatequantityby will have count by with an items quantity needs to be updated
            // if it is -1 that means we have lower a count if it is 5 it means we have to add 5 count to existing count.
            // if updatequantityby by is 0, item will be removed


            // when a user adds a new item to a new shopping cart for the first time
            // when a user adds a new item to an existing shopping cart (basically user has other items in cart)
            // when a user updates an existing item count
            // when a user removes an existing item



            ShoppingCart shoppingCart = _db.ShoppingCarts.Include(u => u.CartItems)
                .FirstOrDefault(u => u.UserId == userId);
            MenuItem menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == menuItemId);

            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (shoppingCart == null && updateQuantityBy > 0) // Create shopping cart & add cart item
            {
                ShoppingCart newCart = new()
                {
                    UserId = userId
                };

                _db.ShoppingCarts.Add(newCart);
                _db.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };

                _db.CartItems.Add(newCartItem);
                _db.SaveChanges();
            }
            else // Shopping cart already exist
            {
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(u => u.MenuItemId == menuItemId);

                if (cartItemInCart == null) // Item doesn't exist in current cart
                {
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };

                    _db.CartItems.Add(newCartItem);
                    _db.SaveChanges();
                }
                else // Item already exist in current cart
                {
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;

                    if (updateQuantityBy == 0 || newQuantity <= 0) 
                    {
                        // remove item from cart (remove the cart if this is the only item in the cart)
                        _db.CartItems.Remove(cartItemInCart);

                        if (shoppingCart.CartItems.Count() == 1)
                        {
                            _db.ShoppingCarts.Remove(shoppingCart);
                        }

                        _db.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = cartItemInCart.Quantity + updateQuantityBy;
                        _db.SaveChanges();
                    }
                }
            }

            return _response;
        }
    }
}
