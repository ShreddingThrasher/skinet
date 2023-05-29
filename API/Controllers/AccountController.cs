using System.Security.Claims;
using API.Dtos;
using API.Errors;
using API.Extensions;
using AutoMapper;
using Core.Entities.Identity;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class AccountController : BaseApiController
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ITokenService _tokenService;
		private readonly IMapper _mapper;

		public AccountController(
			UserManager<AppUser> userManager, 
			SignInManager<AppUser> signInManager,
			ITokenService tokenService,
			IMapper mapper)
		{
			this._userManager = userManager;
			this._signInManager = signInManager;
			this._tokenService = tokenService;
			this._mapper = mapper;
		}
		
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<UserDto>> GetCurrentUser()
		{
			var user = await this._userManager.FindUserByClaimsPrincipleAddress(this.User);
			
			return new UserDto()
			{
				Email = user.Email,
				Token = this._tokenService.CreateToken(user),
				DisplayName = user.DisplayName
			};
		}

		[HttpPost("login")]
		public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
		{
			var user = await this._userManager.FindByEmailAsync(loginDto.Email);

			if (user == null) return Unauthorized(new ApiResponse(401));

			var result = await this._signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

			if (!result.Succeeded) return Unauthorized(new ApiResponse(401));
			
			return new UserDto()
			{
				Email = user.Email,
				Token = this._tokenService.CreateToken(user),
				DisplayName = user.DisplayName
			};
		}
		
		[HttpPost("register")]
		public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
		{
			if(this.CheckEmailExistsAsync(registerDto.Email).Result.Value)
			{
				return new BadRequestObjectResult(new ApiValidationErrorResponse()
					{
						Errors = new [] 
						{
							"Email address is in use"
						}
					});
			}
			
			var user = new AppUser()
			{
				DisplayName = registerDto.DisplayName,
				Email = registerDto.Email,
				UserName = registerDto.Email
			};
			
			var result = await this._userManager.CreateAsync(user, registerDto.Password);

			if (!result.Succeeded) return BadRequest(new ApiResponse(400));
			
			return new UserDto()
			{
				DisplayName = user.DisplayName,
				Token = this._tokenService.CreateToken(user),
				Email = user.Email
			};
		}
		
		[HttpGet("emailexists")]
		public async Task<ActionResult<bool>> CheckEmailExistsAsync([FromQuery]string email)
		{
			return await this._userManager.FindByEmailAsync(email) != null;
		}
		
		[Authorize]
		[HttpGet("address")]
		public async Task<ActionResult<AddressDto>> GetUserAddress()
		{		
			var user = await this._userManager.FindUserByClaimsPrincipleAddress(this.User);
			
			return this._mapper.Map<Address, AddressDto>(user.Address);
		}
		
		[HttpPut("address")]
		[Authorize]
		public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto address)
		{
			var user = await this._userManager.FindUserByClaimsPrincipleAddress(this.HttpContext.User);
			
			user.Address = this._mapper.Map<AddressDto, Address>(address);
			
			var result = await this._userManager.UpdateAsync(user);
			
			if(result.Succeeded) return Ok(this._mapper.Map<Address, AddressDto>(user.Address));
			
			return BadRequest("Problem updating the user");
		}
	}
}