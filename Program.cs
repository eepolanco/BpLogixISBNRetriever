using RestSharp;
using Newtonsoft.Json;
using CsvMapper;
using ISBNRetriever;

namespace OpenLibraryDemo;
class Program
{
    static async Task Main(string[] args)
    {
        {
            List<string> listIsbn = GetInputFileList();
            List<Book> books = new List<Book>();
            var rowNumber = 1;
            foreach (var isbn in listIsbn)
            {
                var book = await GetBookInfoAsync(isbn);
                book.RowNumber = rowNumber++;
                books.Add(book);
            }

            CsvMapping.WriteBooksToCsv(books, $"C:/Users/dell/Desktop/books_{DateTime.Now.ToString("dd-MM-yyyy HHmmss")}.csv");
        }
    }

    public static Dictionary<string, Book> Cache = new Dictionary<string, Book>();
    public static List<string> GetInputFileList()
    {
        string filePath = @"C:/Users/dell/Desktop/ISBN_Input_File.txt";

        List<string> isbnList = new List<string>();

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Replace any commas in the line with spaces
                    line = line.Replace(",", " ");

                    // Trim any leading or trailing white space from the line
                    line = line.Trim();

                    // Split the line into separate ISBNs using the space character as the delimiter
                    string[] isbns = line.Split(' ');

                    // Add each ISBN to the list
                    foreach (var isbn in isbns)
                    {
                        isbnList.Add(isbn);
                    }
                }
            }
        }

        return isbnList;
    }

    public static async Task<Book> GetBookInfoAsync(string isbn)
    {
        // Try to retrieve the book from the cache
        if (Cache.TryGetValue(isbn, out Book? book))
        {
            Console.WriteLine($"Retrieved book info for ISBN {isbn} from cache.");
            return new Book
            {
                Isbn = book.Isbn,
                Title = book.Title,
                Subtitle = book.Subtitle,
                NumberOfPages = book.NumberOfPages,
                PublishDate = book.PublishDate,
                Authors = book.Authors,
                DataRetrievalType = 2
            };
        }

        // Initialize RestClient with the OpenLibrary API endpoint
        var client = new RestClient("https://openlibrary.org/");

        // Set the request parameters
        var request = new RestRequest("isbn/{isbn}.json", Method.Get);
        request.AddParameter("isbn", isbn, ParameterType.UrlSegment);

        try
        {
            // Execute the request and retrieve the response
            var response = await client.ExecuteAsync(request);

            // I used dynamic to keep simple, but could create a DTO for this response and use automapper for map from DTO --> Book. 
            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);

            List<dynamic> authorList = JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse?.authors.ToString());
            List<string> authorNames = await GetAuthorNamesAsync(authorList);

            // Create a new Book object with the retrieved data
            book = new Book
            {
                Isbn = isbn,
                Title = jsonResponse!.title,
                Subtitle = jsonResponse!.subtitle,
                NumberOfPages = jsonResponse!.number_of_pages,
                PublishDate = jsonResponse!.publish_date,
                Authors = authorNames,
                DataRetrievalType = 1
            };

            // Add the book to the cache
            Cache[isbn] = book;

            Console.WriteLine($"Retrieved book info for ISBN {isbn} from API.");

            return book;
        }
        catch (Exception ex)
        {
            string errorMessage = String.Format("Error retrieving isbn: {isbn}", isbn);
            Console.WriteLine(errorMessage);
            throw new Exception(errorMessage, ex);
        }
    }

    public static async Task<List<string>> GetAuthorNamesAsync(List<dynamic> authors)
    {
        List<string> authorNames = new List<string>();
        foreach (var author in authors)
        {
            string authorKey = author["key"];
            string url = $"https://openlibrary.org";
            dynamic jsonResponse = await GetJsonResponseAsync(url, authorKey.Replace("/authors/", ""));
            string personalName = jsonResponse.personal_name != null ? jsonResponse.personal_name : "Unknown Author";
            authorNames.Add(personalName);
        }
        return authorNames;
    }

    public static async Task<dynamic> GetJsonResponseAsync(string baseUrl, string authorKey)
    {
        var client = new RestClient(baseUrl);
        var request = new RestRequest($"authors/{authorKey}.json", Method.Get);
        try
        {
            var response = await client.ExecuteAsync(request);

            dynamic jsonResponse = JsonConvert.DeserializeObject(response.Content);
            return jsonResponse;
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error retrieving author: {authorKey}";
            Console.WriteLine(errorMessage);
            throw new Exception(errorMessage, ex);
        }
    }
}
