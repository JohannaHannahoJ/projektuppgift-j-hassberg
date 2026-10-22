namespace Digibok.Models;

/*
* Representerar en rad i bas-kontoplanens csv-fil.
* Innehåller kontoklassens kod och namn samt kontonummer och kontonamn.
*/

public class AccountCsvRow
{
    public string ClassCode { get; set; } = "";
    public string ClassName { get; set; } = "";
    public int AccountNumber { get; set; }
    public string AccountName { get; set; } = "";
}
