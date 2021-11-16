namespace Domain.Entities
{
    public class ActiveOrder : Order
    {
        public string ObjectType { get; } = nameof(ActiveOrder);
        public int Ttl { get; set; } = -1;

        public void MarkForDeletion()
        {
            Ttl = 60;
        }
    }
}
