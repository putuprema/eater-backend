namespace Application.Queries.GetTableById
{
    public class GetTableByIdQueryHandler : IRequestHandler<GetTableByIdQuery, TableDto>
    {
        private readonly ITableRepository _tableRepository;

        public GetTableByIdQueryHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<TableDto> Handle(GetTableByIdQuery request, CancellationToken cancellationToken)
        {
            var table = await _tableRepository.GetByIdAsync(request.TableId, cancellationToken);
            return table.Adapt<TableDto>();
        }
    }
}
