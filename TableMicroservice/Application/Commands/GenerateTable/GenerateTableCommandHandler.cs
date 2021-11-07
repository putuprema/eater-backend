namespace Application.Commands.GenerateTable
{
    public class GenerateTableCommandHandler : IRequestHandler<GenerateTableCommand>
    {
        private readonly ITableRepository _tableRepository;

        public GenerateTableCommandHandler(ITableRepository tableRepository)
        {
            _tableRepository = tableRepository;
        }

        public async Task<Unit> Handle(GenerateTableCommand request, CancellationToken cancellationToken)
        {
            await _tableRepository.GenerateTablesAsync(request.Quantity, cancellationToken);
            return Unit.Value;
        }
    }
}
