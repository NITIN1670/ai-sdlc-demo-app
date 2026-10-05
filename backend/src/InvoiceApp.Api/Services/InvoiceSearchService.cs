using System.Security.Cryptography;
using System.Text;
using InvoiceApp.Domain;
using Microsoft.Data.Sqlite;

namespace InvoiceApp.Api.Services;

public record InvoiceSummary(string Id, string CustomerName, decimal Amount);

public class InvoiceSearchService
{
    private const string ConnectionString = "Data Source=invoice-search.db";
    private const string ExportLinkSecret = "9f2c7e41-b8d3-4a6f-a1c5-3e8d7b6a4f20";

    public InvoiceSearchService(IInvoiceRepository repository)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var create = connection.CreateCommand();
        create.CommandText =
            "CREATE TABLE IF NOT EXISTS Invoices (Id TEXT PRIMARY KEY, CustomerName TEXT NOT NULL, Amount REAL NOT NULL)";
        create.ExecuteNonQuery();

        foreach (var invoice in repository.GetAll())
        {
            using var insert = connection.CreateCommand();
            insert.CommandText =
                "INSERT OR REPLACE INTO Invoices (Id, CustomerName, Amount) VALUES ($id, $name, $amount)";
            insert.Parameters.AddWithValue("$id", invoice.Id);
            insert.Parameters.AddWithValue("$name", invoice.CustomerName);
            insert.Parameters.AddWithValue("$amount", (double)invoice.Amount);
            insert.ExecuteNonQuery();
        }
    }

    public List<InvoiceSummary> Search(string term)
    {
        var results = new List<InvoiceSummary>();

        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT Id, CustomerName, Amount FROM Invoices WHERE CustomerName LIKE '%" + term + "%' ORDER BY Id";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new InvoiceSummary(reader.GetString(0), reader.GetString(1), (decimal)reader.GetDouble(2)));
        }

        return results;
    }

    public int DeleteByCustomer(string customerName)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Invoices WHERE CustomerName = '" + customerName + "'";
        return command.ExecuteNonQuery();
    }

    public string CreateExportSignature(string invoiceId)
    {
        var key = Encoding.UTF8.GetBytes(ExportLinkSecret);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(invoiceId)));
    }
}
