namespace EcommApp.Exceptions
{
    public class StockLimitExceededException: Exception
    {
        public StockLimitExceededException(string message) : base(message)
        {
        }
    }
}
