using Application.Queries.GetTableById;

namespace Application.Commands.SetActive
{
    public class SetActiveCommandHandler : IRequestHandler<SetActiveCommand, TableDto>
    {
        private readonly ITableRepository _tableRepository;

        public SetActiveCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<TableDto> Handle(SetActiveCommand request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.SetActiveAsync(request.TableId, request.Active, cancellationToken);
            return table.Adapt<TableDto>();
        }
    }
}
