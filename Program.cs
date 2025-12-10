using NLog;


using System.Linq;
using NorthwindConsole.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");
do
{
  Console.WriteLine("1) Display categories");
  Console.WriteLine("2) Add category");
  Console.WriteLine("3) Display Category and related products");
   Console.WriteLine("4) Display all Categories and their related products");
   Console.WriteLine("5) Add Product");
    Console.WriteLine("6) Display Products based off of discontinued status");
    Console.WriteLine("7) Edit a Product");
    Console.WriteLine("8) Display a specific Product by any field");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  if (choice == "1")
  {
    // display categories
    var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json");

    var config = configuration.Build();

    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryName);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "2")
  {
    // Add category
    Category category = new();
    Console.WriteLine("Enter Category Name:");
    category.CategoryName = Console.ReadLine()!;
    Console.WriteLine("Enter the Category Description:");
    category.Description = Console.ReadLine();
     ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid)
    {
      var db = new DataContext();
      // check for unique name
      if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["CategoryName"]));
      }
      else
      {
        logger.Info("Validation passed");
        db.Categories.Add(category);
        db.SaveChanges();
      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "3")
  {
    var db = new DataContext();
    var query = db.Categories.OrderBy(p => p.CategoryId);

    Console.WriteLine("Select the category whose products you want to display:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"{category.CategoryName} - {category.Description}");
    foreach (Product p in category.Products)
    {
     Console.WriteLine($"\t{p.ProductName}");
    }
  }
   else if (choice == "4")
  {
    var db = new DataContext();
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\t{p.ProductName}");
      }
    
    
    }
  }
    else if (choice == "5")
  {
    // Add Product
    Product product = new();
    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;
    Console.WriteLine("Enter the Product's Supplier id:");
    product.SupplierId = Convert.ToInt32(Console.ReadLine());
    Console.WriteLine("Enter the Product's Category id:");
    product.CategoryId = Convert.ToInt32(Console.ReadLine());
    console.WriteLine("Enter the Product's Quantity per unit:");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("Enter the Product's Unit Price:");
    product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
    Console.WriteLine("Enter the Product's Units in stock:");
    product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
    Console.WriteLine("Enter the Product's Units on order:");
    product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
    Console.WriteLine("Enter the Product's Reorder level:");
    product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
    product.Discontinued = false;
        ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult>();


   
    if (isValid)
    {
      var db = new DataContext();

      // check for unique name
      if (db.Product.Any(p => p.ProductName == product.ProductName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", ["ProductName"]));
      }
      else
      {
        logger.Info("Validation passed");
        db.Products.Add(product);
        db.SaveChanges();
      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  
  
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
