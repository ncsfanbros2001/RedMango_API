using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.Models;
using RedMango_API.Models.DTO;
using RedMango_API.Services;
using RedMango_API.Utilities;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly DatabaseContext _db;
        private API_Response _response;
        private readonly IBlobService _blobService;

        public MenuItemController(DatabaseContext db, IBlobService blobService)
        {
            _db = db;
            _response = new API_Response();
            _blobService = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems() // Get all menu item in database
        {
            _response.Result = _db.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);

        }

        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            MenuItem menuItem = _db.MenuItems.FirstOrDefault(u => u.Id == id);

            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }

            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);

        }

        [HttpPost]
        [Authorize(Roles = StaticDetail.AdminRole)]
        public async Task<ActionResult<API_Response>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest(_response);
                    }

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";

                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price = menuItemCreateDTO.Price,
                        Category = menuItemCreateDTO.Category,
                        SpecialTag = menuItemCreateDTO.SpecialTag,
                        Description = menuItemCreateDTO.Description,
                        Image = await _blobService.UploadBlob(fileName, StaticDetail.StorageContainer,
                            menuItemCreateDTO.File)
                    };

                    _db.MenuItems.Add(menuItemToCreate);
                    _db.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;

                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);

                }
                else
                {
                    _response.IsSuccess = false;
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return _response;
            }
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = StaticDetail.AdminRole)]
        public async Task<ActionResult<API_Response>> UpdateMenuItem(int id,
            [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id) // Null DTO or ID not exist
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id); // Find Menu Item

                    if (menuItemFromDb == null) // Item not exist
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    menuItemFromDb.Name = menuItemUpdateDTO.Name;
                    menuItemFromDb.Price = menuItemUpdateDTO.Price;
                    menuItemFromDb.Category = menuItemUpdateDTO.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDTO.SpecialTag;
                    menuItemFromDb.Description = menuItemUpdateDTO.Description;

                    if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";
                        await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(),
                            StaticDetail.StorageContainer);
                        menuItemFromDb.Image = await _blobService.UploadBlob(fileName, StaticDetail.StorageContainer,
                            menuItemUpdateDTO.File);
                    }

                    _db.MenuItems.Update(menuItemFromDb);
                    _db.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;

                    return Ok(_response);

                }
                else
                {
                    _response.IsSuccess = false;
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return _response;
            }
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = StaticDetail.AdminRole)]
        public async Task<ActionResult<API_Response>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                MenuItem menuItemFromDb = await _db.MenuItems.FindAsync(id);

                if (menuItemFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }

                await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(),
                        StaticDetail.StorageContainer);

                int milliSeconds = 2000;
                Thread.Sleep(milliSeconds);

                _db.MenuItems.Remove(menuItemFromDb);
                _db.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.ToString() };
                return _response;
            }
        }
    }
}
