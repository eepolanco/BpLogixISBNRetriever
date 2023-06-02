using CsvHelper;
using CsvHelper.Configuration;
using ISBNRetriever;
using System.Globalization;

namespace CsvMapper;
public sealed class BookMap : ClassMap<Book>
{
    public BookMap()
    {
        Map(b => b.RowNumber).Name("Row Number");
        Map(b => b.DataRetrievalType).Name("Data Retrieval Type").Convert(x => x.Value.DataRetrievalType == 1 ? "Server" : "Cache");
        Map(b => b.Isbn).Name("ISBN");
        Map(b => b.Title).Name("Title").Convert(x => x.Value.Title == null ? "N/A" : x.Value.Title); ;
        Map(b => b.Subtitle).Name("Subtitle").Convert(x => x.Value.Subtitle == null ? "N/A" : x.Value.Subtitle);
        Map(b => b.Authors).Name("Author Name(s)").Convert(x => string.Join("; ", x.Value.Authors));
        Map(b => b.NumberOfPages).Name("Number of Pages").Convert(x => x.Value.NumberOfPages == null ? "N/A" : x.Value.NumberOfPages.ToString());
        Map(b => b.PublishDate).Name("Publish Date");
    }
}

public static class CsvMapping
{
    public static void WriteBooksToCsv(IEnumerable<Book> books, string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        };

        var bookMap = new BookMap();

        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.Context.RegisterClassMap(bookMap);
            csv.WriteRecords(books);
        }
    }

}
