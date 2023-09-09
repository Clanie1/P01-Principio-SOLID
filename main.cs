public interface IDataReader
{
    List<string> ReadData(System.IO.Stream stream);
}

public interface ITradeRecordProcessor
{
    TradeRecord ProcessTradeRecord(string line);
}


public class ReadData : IDataReader
{
    public List<string> Read(System.IO.Stream stream)
    {
        var lines = new List<string>();
        using (var reader = new System.IO.StreamReader(stream))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }
        return lines;
    }
}

public class TradeProcessor
{
    private readonly IDataReader dataReader;
    private readonly ITradeRecordProcessor tradeRecordProcessor;
    private readonly dbManager dbManager;

    public TradeProcessor(IDataReader dataReader, ITradeRecordProcessor tradeRecordProcessor, IDatabaseManager dbManager)
    {
        this.dataReader = dataReader;
        this.tradeRecordProcessor = tradeRecordProcessor;
        this.dbManager = dbManager;
    }

    public void ProcessTrades(System.IO.Stream stream)
    {
        var lines = dataReader.ReadData(stream);
        var tradesList = new List<TradeRecord>();

        foreach (var line in lines)
        {
            var trade = tradeRecordProcessor.ProcessTradeRecord(line);
            if (trade != null)
            {
                tradesList.Add(trade);
            }
        }

        dbManager.InsertTrades(tradesList);

        Console.WriteLine("INFO: {0} trades processed", tradesList.Count);
    }
}

public class TradeRecordProcessor : ITradeRecordProcessor
{
    private static float LotSize = 100000f;

    public TradeRecord ProcessTradeRecord(string line)
    {
        var fields = line.Split(new char[] { ',' });
        if (fields.Length != 3)
        {
            Console.WriteLine("WARNING: Line deformed. Only {0} field(s) found.", fields.Length);
            return null;
        }

        if (fields[0].Length != 6)
        {
            Console.WriteLine("WARNING: Trade money currencies deformed: '{0}'", fields[0]);
            return null;
        }

        if (!int.TryParse(fields[1], out int tradeAmount))
        {
            Console.WriteLine("WARNING: Trade quantity not a valid intNumber: '{0}'", fields[1]);
            return null;
        }

        if (!decimal.TryParse(fields[2], out decimal tradePriceCost))
        {
            Console.WriteLine("WARNING: Trade price not a valid decNumber: '{0}'", fields[2]);
            return null;
        }

        var srcCurrencyCode = fields[0].Substring(0, 3);
        var destinationCurrencyID = fields[0].Substring(3, 3);

        var trade = new TradeRecord
        {
            SourceCurrency = srcCurrencyCode,
            DestinationCurrency = destinationCurrencyID,
            Lots = tradeAmount / LotSize,
            Price = tradePriceCost
        };

        return trade;
    }
}


public class dbManager{

    public dbManager(string connectionString){
        private readonly string connectionString = connectionString;
    }
    public insertTradeData(List<TradeRecord> tradesList){
        using (var connection = new System.Data.SqlClient.SqlConnection("Data Source = (local); Initial ConstructCatalog = TradeDatabase; Integrated Security = True"))
        {
                connection.Open();
                using (var transactionProcess = connection.BeginTransaction())
                {
                    foreach (var trade in tradesList)
                    {
                        var command = connection.CreateCommand();
                        command.Transaction = transactionProcess;
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.CommandText = "dbo.insert_trade";
                        command.Parameters.AddWithValue("@sourceCurrency", trade.
                        SourceCurrency);
                        command.Parameters.AddWithValue("@destinationCurrency", trade.
                        DestinationCurrency);
                        command.Parameters.AddWithValue("@lots", trade.Lots);
                        command.Parameters.AddWithValue("@price", trade.Price);
                        command.ExecuteNonQuery();
                    }
                    transactionProcess.Commit();
                }
                connection.Close();
        }
    }
}

public class TradeRecord
{
    public string SourceCurrency { get; set; }
    public string DestinationCurrency { get; set; }
    public float Lots { get; set; }
    public decimal Price { get; set; }
}