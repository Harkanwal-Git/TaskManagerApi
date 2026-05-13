using MediatR;
using TaskManagerApi.DTO;

namespace TaskManagerApi.CQRS.Commands;

public record CreateTaskCommand(CreateTaskDto CreateTaskDto, Guid userId, bool IsAdmin) : IRequest<ResponseTaskDto>;