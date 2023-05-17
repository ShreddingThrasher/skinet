using System.Net;
using System.Text.RegularExpressions;
using API.Errors;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly StoreContext _context;
        public BuggyController(StoreContext context)
        {
            this._context = context;
        }

        [HttpGet("notfound")]
        public ActionResult GetNotFoundRequest()
        {
            var thing = this._context.Products.Find(100);

            if (thing == null)
            {
                return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));
            }

            return Ok();
        }

        [HttpGet("servererror")]
        public ActionResult GetServerError()
        {
            var thing = this._context.Products.Find(56);

            //Generate exception
            var thingToReturn = thing.ToString();

            return Ok();
        }

        [HttpGet("badrequest")]
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse((int)HttpStatusCode.BadRequest));
        }

        [HttpGet("badrequest/{id}")]
        public ActionResult GetBadRequest(int id)
        {
            return Ok();
        }
    }
}