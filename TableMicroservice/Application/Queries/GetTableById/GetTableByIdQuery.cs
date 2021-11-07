namespace Application.Queries.GetTableById
{
    public class GetTableByIdQuery : IRequest<TableDto>
    {
        public string TableId { get; set; }
    }
}
