using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.DTO;
using RedMango_API.Utilities;
using System.Net;
using System.Text.Json;

namespace RedMango_API.Controllers
{
    [Route("api/Order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private API_Response _response;

        public OrderController(DatabaseContext db)
        {
            _db = db;
            _response = new API_Response();
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<API_Response>> GetOrders(string? userId, string searchString,
            string status, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders = _db.OrderHeaders.Include(u => u.OrderDetails)
                    .ThenInclude(u => u.MenuItem)
                    .OrderByDescending(u => u.OrderHeaderId);

                if (!string.IsNullOrEmpty(userId))
                {
                    orderHeaders = orderHeaders.Where(u => u.ApplicationUserId == userId);
                }
                
                if (!string.IsNullOrEmpty(searchString))
                {
                    orderHeaders = orderHeaders.Where(
                        u => u.PickupPhoneNumber.ToLower().Contains(searchString.ToLower()) 
                        || u.PickupEmail.ToLower().Contains(searchString.ToLower()) 
                        || u.PickupEmail.ToLower().Contains(searchString.ToLower()));
                }
                if (!string.IsNullOrEmpty(status))
                {
                    orderHeaders = orderHeaders.Where(u => u.Status.ToLower() == status.ToLower());
                }

                Pagination pagination = new Pagination()
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = orderHeaders.Count()
                };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = orderHeaders.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<API_Response>> GetOrder(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeaders = _db.OrderHeaders.Include(u => u.OrderDetails).ThenInclude(u => u.MenuItem)
                    .Where(u => u.OrderHeaderId == id);

                if (orderHeaders == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                else
                {
                    _response.Result = orderHeaders;
                    _response.StatusCode = HttpStatusCode.OK;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<API_Response>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                OrderHeader order = new()
                {
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                    PickupEmail = orderHeaderDTO.PickupEmail,
                    PickupName = orderHeaderDTO.PickupName,
                    PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID,
                    TotalItems = orderHeaderDTO.TotalItems,
                    Status = String.IsNullOrEmpty(orderHeaderDTO.Status) ? StaticDetail.status_pending :
                        orderHeaderDTO.Status
                };

                if (ModelState.IsValid)
                {
                    _db.OrderHeaders.Add(order);
                    _db.SaveChanges();

                    foreach (var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _db.OrderDetails.Add(orderDetails);
                    }
                    _db.SaveChanges();
                    _response.Result = order;
                    order.OrderDetails = null;
                    _response.StatusCode = HttpStatusCode.Created;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<API_Response>> UpdateOrderHeader(int id,
            [FromBody] OrderHeaderUpdateDTO orderHeaderUpdateDTO)
        {
            try
            {
                if (orderHeaderUpdateDTO == null || id != orderHeaderUpdateDTO.OrderHeaderId) 
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }
                OrderHeader orderFromDb = _db.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == id);

                if (orderFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest();
                }

                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupName))
                {
                    orderFromDb.PickupName = orderHeaderUpdateDTO.PickupName;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupEmail))
                {
                    orderFromDb.PickupEmail = orderHeaderUpdateDTO.PickupEmail;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.PickupPhoneNumber))
                {
                    orderFromDb.PickupPhoneNumber = orderHeaderUpdateDTO.PickupPhoneNumber;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.Status))
                {
                    orderFromDb.Status = orderHeaderUpdateDTO.Status;
                }
                if (!string.IsNullOrEmpty(orderHeaderUpdateDTO.StripePaymentIntentID))
                {
                    orderFromDb.StripePaymentIntentID = orderHeaderUpdateDTO.StripePaymentIntentID;
                }

                _db.SaveChanges();
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
