using System.Security.Claims;

Dictionary<string, List<string>> gamesMap = new()
{
    {"player1",new List<string>(){"Street Fighter","Street Fighter II","Minecraft"}},
    {"player2",new List<string>(){"Mario","Contra","Pubg"}}
};

Dictionary<string, List<string>> subscriptionMap = new()
{
    {"silver",new List<string>(){"Street Fighter","Street Fighter II","Minecraft"}},
    {"gold",new List<string>(){"Street Fighter","Street Fighter II","Minecraft","Mario","Contra","Pubg"}}
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/playergames", () => gamesMap)
    .RequireAuthorization(policy =>
    {
        policy.RequireRole("admin");
    });

app.MapGet("/mygames", (ClaimsPrincipal user) =>
{
    var hasClaim = user.HasClaim(claim => claim.Type == "subscription");

    if (hasClaim)
    {
        var subs = user.FindFirstValue("subscription") ?? throw new Exception("Claim has no value");
        return Results.Ok(subscriptionMap[subs]);
    }

    ArgumentException.ThrowIfNullOrEmpty(user.Identity?.Name);
    var username = user.Identity.Name;

    if (!gamesMap.ContainsKey(username))
    {
        return Results.Empty;
    }
    return Results.Ok(gamesMap[username]);
}).RequireAuthorization(policy =>
{
    policy.RequireRole("player");
});
app.Run();
