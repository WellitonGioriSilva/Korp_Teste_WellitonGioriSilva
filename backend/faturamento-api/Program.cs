using faturamento_api.DataContext;
using faturamento_api.Profiles;
using faturamento_api.Services;
using Microsoft.EntityFrameworkCore;
using faturamento_api.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<BaixaEstoquePublisher>();
builder.Services.AddHostedService<BaixaEstoqueResultadoConsumer>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<NotaFiscalSseService>();
builder.Services.AddScoped<NotaFiscalService>();
builder.Services.AddHttpClient<EstoqueApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EstoqueApi:BaseUrl"]!);
});
builder.Services.AddAutoMapper(config => config.AddProfile<NotaFiscalProfile>());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
