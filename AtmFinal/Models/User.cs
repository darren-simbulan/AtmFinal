namespace AtmFinal.Models
{
    public class User
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string Pin { get; set; }
        public decimal Balance { get; set; }
        public string AccountName { get; set; }
    }
}
