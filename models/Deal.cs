namespace MainApp.models
{
    public class Deal
    {
        public Deal(int dealId, string company, string peril, string location)
        {
            DealId = dealId;
            Company = company;
            Peril = peril;
            Location = location;
        }
        public Deal()
        {

        }

        public int DealId { get; set; }
        public string Company { get; set; }
        public string Peril { get; set; }
        public string Location { get; set; }
        public decimal Amount { get; set; }
    }
}
