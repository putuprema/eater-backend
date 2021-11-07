using Application.Queries.GetTableById;

namespace Application.Commands.SetActive
{
    public class SetActiveCommand : IRequest<TableDto>
    {
        public string TableId { get; set; }
        public bool Active { get; set; } = true;
    }
}
