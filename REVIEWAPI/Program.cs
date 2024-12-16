using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});
builder.Services.AddDbContext<EmailDb>(opt => opt.UseInMemoryDatabase("EmailList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "Email API";
    config.Title = "EmailAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "EmailAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// create api map group
var emailItems = app.MapGroup("/emailitems");

emailItems.MapGet("/", GetEmailItems);
emailItems.MapGet("/sent", GetSentEmails);
emailItems.MapGet("/{id}", GetEmailsById);
emailItems.MapPost("/", CreateEmails);
emailItems.MapPut("/{id}", UpdateSentStatus);
emailItems.MapDelete("/{id}", DeleteEmailById);


app.Run();

// Gets all emails present in db
static async Task<IResult> GetEmailItems(EmailDb emailDb)
{   
    var filteredResults = await emailDb.Emails.Select(email => new EmailDTO(email)).ToArrayAsync();
    return TypedResults.Ok(filteredResults);
}

// Returns all sent emails in the DB
static async Task<IResult> GetSentEmails(EmailDb emailDb)
{
    var sentEmails = await emailDb.Emails.Where(email => email.IsSent).ToArrayAsync();
    var filteredResults = sentEmails.Select(email => new EmailDTO(email));
    return TypedResults.Ok(filteredResults);
}

// Get Emails
static async Task<IResult> GetEmailsById(int id, EmailDb emailDb)
{   
    var isEmailRegistered =  await emailDb.Emails.FindAsync(id); 
    if (isEmailRegistered is null) return TypedResults.NotFound();
    return TypedResults.Ok( new EmailDTO(isEmailRegistered));
}

// Register new email
static async Task<IResult> CreateEmails(EmailDTO emailItem, EmailDb emailDb)
{
    var userEmail = new Email
    {
        Name = emailItem.Name,
        IsSent = emailItem.IsSent,
        UserEmail = emailItem.UserEmail,
    };
    
    emailDb.Add(userEmail);
    await emailDb.SaveChangesAsync();
    
    emailItem = new EmailDTO(userEmail);
    return TypedResults.Created($"/emailitems/{emailItem.Id}", emailItem);
}

// Update email object
static async Task<IResult> UpdateSentStatus(int id, EmailDTO emailDto, EmailDb db)
{
    var email = await db.Emails.FindAsync(id);
    if (email is null) return TypedResults.NotFound();
    
    email.Name = emailDto.Name;
    email.UserEmail = emailDto.UserEmail;
    email.IsSent = emailDto.IsSent;
    
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
}


// delete email by 
static async Task<IResult> DeleteEmailById(int id, EmailDb db)
{
    var isEmailValid = await db.Emails.FindAsync(id);
    if (isEmailValid is null) return TypedResults.NotFound();
    
    db.Emails.Remove(isEmailValid);
    await db.SaveChangesAsync();
    return TypedResults.NoContent();
    
}

