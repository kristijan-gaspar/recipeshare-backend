using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using RecipeShare.Application.Exceptions;

using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepo;
    private readonly IUserRepository _userRepo;
    private readonly IUnitOfWork _unitOfWork;



    public FollowService(IFollowRepository followRepo, IUserRepository userRepo, IUnitOfWork unitOfWork)
    {
        _followRepo = followRepo;
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ToggleFollowAsync(int targetUserId, int currentUserId)
    {
        if (targetUserId == currentUserId)
            throw new BadRequestException("You can't follow yourself");

        var targetUser = await _userRepo.GetByIdAsync(targetUserId);
        if (targetUser == null)
            throw new NotFoundException("User not found");

        var existingFollow = await _followRepo.GetByUsersAsync(currentUserId, targetUserId);

        if(existingFollow != null)
        {
            _followRepo.Delete(existingFollow);
            await _unitOfWork.SaveChangesAsync();
            return false;
        }

        var follow = new Follow
        {
            FollowerId = currentUserId,
            FollowedId = targetUserId,
            CreatedAt = DateTime.UtcNow,
        };

        await _followRepo.AddAsync(follow);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
