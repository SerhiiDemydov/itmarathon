using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Queries;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;
using RoomAggregate = Epam.ItMarathon.ApiService.Domain.Aggregate.Room.Room;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{

    public class DeleteUserHandler(IRoomRepository roomRepository)
        : IRequestHandler<DeleteUserRequest, Result<RoomAggregate, ValidationResult>>
    {
        ///<inheritdoc/>
        public async Task<Result<RoomAggregate, ValidationResult>> Handle(DeleteUserRequest request,
            CancellationToken cancellationToken)
        {
            // 1. If UserCode not found, return BadRequestError
            if (string.IsNullOrEmpty(request.UserCode))
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure("userCode", "userCode is required.")
                ]));
            }


            // 3. Get room by user code
            var roomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);


            // 4. If room not found, return NotFoundError
            if (roomResult.IsFailure)
            {
                return roomResult;
            }

            // 2. If userCode and UserID is the same user, return BadRequestError
            var user = roomResult.Value;
            if (user.Id == request.UserId.Value)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(string.Empty, "User cannot delete himself.")
                ]));
            }

            // 5. If room found, delete user by id from room
            var room = roomResult.Value;
            var deleteResult = room.DeleteUser(request.UserId);
            if (deleteResult.IsFailure)
            {
                return deleteResult;
            }

            // 6. Update room in repository
            var updateResult = await roomRepository.UpdateAsync(room, cancellationToken);
            if (updateResult.IsFailure)
            {
                return Result.Failure<RoomAggregate, ValidationResult>(new BadRequestError([
                    new ValidationFailure(string.Empty, updateResult.Error)
                ]));
            }

            // 7. Get updated room
            var updatedRoomResult = await roomRepository.GetByUserCodeAsync(request.UserCode, cancellationToken);

            return updatedRoomResult;
        }
    }
}