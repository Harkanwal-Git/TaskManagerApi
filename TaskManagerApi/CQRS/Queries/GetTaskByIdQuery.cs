using MediatR;
using TaskManagerApi.DTO;

namespace TaskManagerApi.CQRS.Queries;


public record GetTaskByIdQuery(Guid TaskId, Guid UserId, bool IsAdmin) : IRequest<ResponseTaskDto?>;