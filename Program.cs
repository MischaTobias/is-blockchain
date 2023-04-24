// Replace with the name of your confidential ledger

using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.ConfidentialLedger;

const string ledgerName = "confidential-ledger-is";
var ledgerUri = $"https://{ledgerName}.confidential-ledger.azure.com";

// Create a confidential ledger client using the ledger URI and DefaultAzureCredential

var ledgerClient = new ConfidentialLedgerClient(new Uri(ledgerUri), new DefaultAzureCredential());

// Write to the ledger

Operation postOperation = ledgerClient.PostLedgerEntry(
waitUntil: WaitUntil.Completed,
RequestContent.Create(
new { contents = "Hello world!" }));

string content = postOperation.GetRawResponse().Content.ToString();
var transactionId = postOperation.Id;
string collectionId = "subledger:0";

// Try fetching the ledger entry until it is "loaded".
Response getByCollectionResponse = default;
JsonElement rootElement = default;
bool loaded = false;

while (!loaded)
{
    // Provide both the transactionId and collectionId.
    getByCollectionResponse = ledgerClient.GetLedgerEntry(transactionId, collectionId);
    rootElement = JsonDocument.Parse(getByCollectionResponse.Content).RootElement;
    loaded = rootElement.GetProperty("state").GetString() != "Loading";
}

string contents = rootElement
.GetProperty("entry")
.GetProperty("contents")
.GetString();

Console.WriteLine(contents); // "Hello world!"

// Now just provide the transactionId.
getByCollectionResponse = ledgerClient.GetLedgerEntry(transactionId);

string collectionId2 = JsonDocument.Parse(getByCollectionResponse.Content)
.RootElement
.GetProperty("entry")
.GetProperty("collectionId")
.GetString();

Console.WriteLine($"{collectionId} == {collectionId2}");
Console.ReadLine();