using ABC_Retail.Services;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Retrieve the connection string from the configuration
string connectionString = builder.Configuration.GetConnectionString("AzureStorage");

// Register the TableServiceClient with dependency injection
builder.Services.AddSingleton(new TableServiceClient(connectionString));
builder.Services.AddSingleton(new AzureStorageService(connectionString));
builder.Services.AddSingleton(new BlobServiceClient(connectionString));
builder.Services.AddSingleton(new ShareServiceClient(connectionString));
builder.Services.AddSingleton(new QueueServiceClient(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();

