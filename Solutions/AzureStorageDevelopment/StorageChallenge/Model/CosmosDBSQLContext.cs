using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CSSTDEvaluation;
using CSSTDModels;
using System.IO;
using System.Configuration;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
#region "Data structures"
/*       
 namespace CSSTDModels       
  {           
     [Description("Sample product descriptive document used for Document DB APIs and search")]            
     public class ProductDocument            {               
         [Description("Arbitrary key - set to a GUID")]               
          public string ID { get; set; }                  
          [Description("Industry designation, currently 'Training' or 'Swimming'")]               
           public string Industry { get; set; }                  
           [Description("Name of the product")]               
            public string Name { get; set; }                  
            [Description("Pricing tier, currently 'Basic' or 'Premium'")]               
             public string Tier { get; set; }                
               [Description("Product description, primarily in Latin with a little English embedded for searching.")]               
                public string Description { get; set; }              }        } * */
#endregion  namespace CSSTDSolution.Models{    
public class CosmosDBSQLContext : ICosmosDBSQLContext
{
    private const string databaseName = "productDB"; private const string collectionName = "products";
    private DocumentClient client; public CosmosDBSQLContext(string uri, string key)
    {
        client = new DocumentClient(new Uri(uri), key);
    }
    public string ConnectionString { get; set; }
    public async Task CreateCollection(string collectionName)
    {
        var database = await client.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseName });
        DocumentCollection collectionSpec = new DocumentCollection { Id = collectionName };
        DocumentCollection collection = await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(databaseName), collectionSpec,
         new RequestOptions { OfferThroughput = 400 });
    }
    public List<ProductDocument> GetDocuments(string collectionName)
    {
        IQueryable<ProductDocument> query = client.CreateDocumentQuery<ProductDocument>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));
        return query.ToList();
    }
    public List<ProductDocument> GetDocuments(string industry, string collectionName)
    {
        IQueryable<ProductDocument> query = client.CreateDocumentQuery<ProductDocument>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName)).Where(p => p.Industry == industry); return query.ToList();
    }
    public Task UploadDocuments(List<ProductDocument> documents, string collectionName)
    {
        List<Task> tasks = new List<Task>();
        foreach (var document in documents)
        {
            tasks.Add(Task.Run(async () =>
            {
                var uri = UriFactory.CreateDocumentCollectionUri(databaseName, collectionName);
                var result = await client.UpsertDocumentAsync(uri, document);
            }));
        }
        Task.WaitAll(tasks.ToArray());
        return Task.CompletedTask;
    }
}}

