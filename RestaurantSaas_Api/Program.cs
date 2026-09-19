
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Auth;
using RestaurantSaaS.Infrastructure.Auth.RestaurantSaaS.Infrastructure.Auth;
using RestaurantSaaS.Infrastructure.Data;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;  
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),

            ClockSkew = TimeSpan.Zero 
        };
    });

/*
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
        };
    });*/

builder.Services.AddAuthorization();




builder.Services.AddDbContext<AppDbContext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<IAppDbContext>(sp =>sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRestaurantTableService, RestaurantTableService>();
builder.Services.AddScoped<IOrganizationUserService, OrganizationUserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();



// Register Swagger generator and customize its behavior.
builder.Services.AddSwaggerGen(options =>
{
    // ===============================
    // 1) Define the JWT Bearer security scheme
    // ===============================
    //
    // This tells Swagger that our API uses JWT Bearer authentication
    // through the HTTP Authorization header.
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        // The name of the HTTP header where the token will be sent.
        Name = "Authorization",


        // Indicates this is an HTTP authentication scheme.
        Type = SecuritySchemeType.Http,


        // Specifies the authentication scheme name.
        // Must be exactly "Bearer" for JWT Bearer tokens.
        Scheme = "Bearer",


        // Optional metadata to describe the token format.
        BearerFormat = "JWT",


        // Specifies that the token is sent in the request header.
        In = ParameterLocation.Header,


        // Text shown in Swagger UI to guide the user.
        Description = "Enter: Bearer {your JWT token}"
    });


    // ===============================
    // 2) Require the Bearer scheme for secured endpoints
    // ===============================
    //
    // This tells Swagger that endpoints protected by [Authorize]
    // require the Bearer token defined above.
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                // Reference the previously defined "Bearer" security scheme.
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },


            // No scopes are required for JWT Bearer authentication.
            // This array is empty because JWT does not use OAuth scopes here.
            new string[] {}
        }
    });
});






builder.Services.AddCors(options =>
{
    options.AddPolicy("RestaurantApiCorsPolicy", policy =>
    {
        policy
            .WithOrigins(
                "https://localhost:7107",
                "http://localhost:5056"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseCors("RestaurantApiCorsPolicy");
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();
