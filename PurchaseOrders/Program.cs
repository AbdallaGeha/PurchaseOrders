using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchaseOrders.API.Middleware;
using PurchaseOrders.Application.Services.Financial;
using PurchaseOrders.Application.Services.Payments;
using PurchaseOrders.Application.Services.PurchaseOrders;
using PurchaseOrders.Application.Services.PurchaseOrderStatements;
using PurchaseOrders.Application.Services.Setup;
using PurchaseOrders.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(x => x.AddDefaultPolicy(x => x.WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()));

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IPurchaseOrderStatementService, PurchaseOrderStatementService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILookupService, LookupService>();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    //Add validation errors as one concatinated string to 400 response
    options.InvalidModelStateResponseFactory = context =>
    {
        var validationErrors = new List<string>();

        foreach (var state in context.ModelState)
        {
            foreach (var error in state.Value.Errors)
            {
                validationErrors.Add($"{state.Key}: {error.ErrorMessage}");
            }
        }

        return new BadRequestObjectResult(string.Join(System.Environment.NewLine, validationErrors));
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Run database seeding
await SeedDataRunner.RunAsync(app.Services);

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
