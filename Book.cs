
namespace ISBNRetriever;
public class Book
{
    public string? Isbn { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public int? NumberOfPages { get; set; }
    public string? PublishDate { get; set; }
    public List<string>? Authors { get; set; }
    public int RowNumber { get; set; }
    public int DataRetrievalType { get; set; }

}
