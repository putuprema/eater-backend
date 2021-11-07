namespace Application.Queries.GetAllTables
{
    public class GetAllTablesQueryHandler : IRequestHandler<GetAllTablesQuery, IEnumerable<TableFullDto>>
    {
        private readonly ITableRepository _tableRepository;

        public GetAllTablesQueryHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<IEnumerable<TableFullDto>> Handle(GetAllTablesQuery request, CancellationToken cancellationToken)
        {
            var tables = await _tableRepository.GetAllAsync(cancellationToken);
            return tables.Adapt<List<TableFullDto>>();
        }
    }
}
