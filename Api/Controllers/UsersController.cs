using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Data;
using Api.DTOs;
using Api.Entities;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Authorize]
    public class UsersController : ApiBaseController
    {
        private readonly DataContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(DataContext context, IUserRepository userRepository,
        
        IMapper mapper, IPhotoService photoService)
        {
            _photoService = photoService;
            _mapper = mapper;
            _userRepository = userRepository;
            _context = context;

        }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
        userParams.CurrentUserName = user.UserName;
        if(userParams.Gender == null )
            userParams.Gender = (user.Gender == "male") ? "female" : "male";
        var users = await _userRepository.GetMembersAsync(userParams);
        Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotoalPages);
        return Ok(users);
    }


    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _userRepository.GetMemberByUserName(username);
        var userToReturn = _mapper.Map<MemberDto>(user);
        return userToReturn;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var username = User.GetUserName();
        var user = await _userRepository.GetUserByUserNameAsync(username);

        _mapper.Map(memberUpdateDto, user);

        _userRepository.Update(user);
        if (await _userRepository.SaveAllAsync()) return NoContent();
        return BadRequest("Failed to update of this user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

        var results = await _photoService.AddPhotoAsync(file);

        if(results.Error != null) return BadRequest(results.Error.Message);    

        var photo = new Photo
        {
            Url = results.SecureUrl.AbsoluteUri,
            PublicId = results.PublicId 
        };

        if (user.Photos.Count ==  0)
        {
            photo.IsMain = true;
        }

        user.Photos.Add(photo);

        if(await _userRepository.SaveAllAsync()) return _mapper.Map<PhotoDto>(photo);    

        return BadRequest("An error ocres");
    }

    [HttpPut("set-main-photo/{photoid}")]
    public async Task<ActionResult> SetMainPhoto(int photoid)
    {
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoid);

        if ( photo == null ) return Forbid();

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if(currentMain != null) currentMain.IsMain = false;

        photo.IsMain = true;

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("There us some error");

    }

    [HttpDelete("delete-photo/{photoid}")]
    public async Task<ActionResult> DeletePhoto(int photoid)
    {
        var user = await _userRepository.GetUserByUserNameAsync(User.GetUserName());

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoid);
        
        if (photo == null )
            return Forbid();

        if( photo.IsMain ) return BadRequest("You can't delete your main photo");
        
        if( photo.PublicId != null )
        {
            var result = await _photoService.DeletePhoto(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        
        user.Photos.Remove(photo);

        if ( await _userRepository.SaveAllAsync()) return Ok();
        
        return BadRequest("Something went wrong"); 
    }

}
}