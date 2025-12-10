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
    Console.WriteLine("8) Display a specific Product");
    Console.WriteLine("9) Edit a Category");
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
        if (!p.Discontinued)
        {
            Console.WriteLine($"\t{p.ProductName}");
        }
     
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
        if (!p.Discontinued){
             Console.WriteLine($"\t{p.ProductName}");
        }
       
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
    var db = new DataContext();
    db.Suppliers.ToList().ForEach(s => Console.WriteLine($"{s.SupplierId}) {s.CompanyName}"));
    product.SupplierId = int.Parse(Console.ReadLine());
    Console.WriteLine("Enter the Product's Category id:");
    db.Categories.ToList().ForEach(c => Console.WriteLine($"{c.CategoryId}) {c.CategoryName}"));
    product.CategoryId = int.Parse(Console.ReadLine());
    Console.WriteLine("Enter the Product's Quantity per unit:");
    product.QuantityPerUnit = Console.ReadLine();
    Console.WriteLine("Enter the Product's Unit Price:");
    product.UnitPrice = decimal.Parse(Console.ReadLine());
    Console.WriteLine("Enter the Product's Units in stock:");
    product.UnitsInStock = short.Parse(Console.ReadLine());
    Console.WriteLine("Enter the Product's Units on order:");
    product.UnitsOnOrder = short.Parse(Console.ReadLine());
    Console.WriteLine("Enter the Product's Reorder level:");
    product.ReorderLevel = short.Parse(Console.ReadLine());
    product.Discontinued = false;
        ValidationContext context = new ValidationContext(product, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(product, context, results, true);
   
    if (isValid)
    {
      

      // check for unique name
      if (db.Products.Any(p => p.ProductName == product.ProductName))
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
  else if (choice == "6")
  {
    // Display Products based off of discontinued status
    Console.WriteLine("What do you want to see? \n1) Discontinued Products \n2) Active Products \n3) All Products");
    int statusChoice = int.Parse(Console.ReadLine()!);
    logger.Info($"Status choice {statusChoice} selected");
    var db = new DataContext();
    if(statusChoice == 1)
    {
      var query = db.Products.Where(p => p.Discontinued == true).OrderBy(p => p.ProductId);
      Console.WriteLine("Discontinued Products:");
      foreach (var item in query)
      {
        Console.WriteLine($"{item.ProductId}) {item.ProductName}");
      }
    }
    else if (statusChoice == 2)
    {
      var query = db.Products.Where(p => p.Discontinued == false).OrderBy(p => p.ProductId);
      Console.WriteLine("Active Products:");
      foreach (var item in query)
      {
        Console.WriteLine($"{item.ProductId}) {item.ProductName}");
      }
    }
    else if (statusChoice == 3)
    {
      var query = db.Products.OrderBy(p => p.ProductId);
      Console.WriteLine("All Products:");
      foreach (var item in query)
      {
        Console.WriteLine($"{item.ProductId}) {item.ProductName} - Discontinued: {item.Discontinued}");
      }
    }
 

  }
  else if (choice == "7")
  {
    // Edit a Product
     var db = new DataContext();
    var query = db.Products.OrderBy(p => p.ProductId);

    Console.WriteLine("Select the product you want to edit:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.ProductId}) {item.ProductName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"Product {id} selected");
    Product product = db.Products.FirstOrDefault(p => p.ProductId == id);
    Console.WriteLine($"What field would you like to edit? \n1) Product Name \n2) Supplier Id \n3) Category Id \n4) Quantity per unit \n5) Unit Price \n6) Units in stock \n7) Units on order \n8) Reorder level \n9) Discontinued Status ");
    int fieldChoice = int.Parse(Console.ReadLine()!);
    logger.Info($"Field {fieldChoice} selected");
    if (fieldChoice == 1)
    {
      Console.WriteLine("Enter new Product Name:");
      product.ProductName = Console.ReadLine()!;
    }
    else if (fieldChoice == 2)
    {
      Console.WriteLine("Enter new Supplier Id:");
      product.SupplierId = Convert.ToInt32(Console.ReadLine());
    }
    else if (fieldChoice == 3)
    {
      Console.WriteLine("Enter new Category Id:");
      product.CategoryId = Convert.ToInt32(Console.ReadLine());
    }
    else if (fieldChoice == 4)
    {
      Console.WriteLine("Enter new Quantity per unit:");
      product.QuantityPerUnit = Console.ReadLine();
    }
    else if (fieldChoice == 5)
    {
      Console.WriteLine("Enter new Unit Price:");
      product.UnitPrice = Convert.ToDecimal(Console.ReadLine());
    }
    else if (fieldChoice == 6)
    {
      Console.WriteLine("Enter new Units in stock:");
      product.UnitsInStock = Convert.ToInt16(Console.ReadLine());
    }
    else if (fieldChoice == 7)
    {
      Console.WriteLine("Enter new Units on order:");
      product.UnitsOnOrder = Convert.ToInt16(Console.ReadLine());
    }
    else if (fieldChoice == 8)
    {
      Console.WriteLine("Enter new Reorder level:");
      product.ReorderLevel = Convert.ToInt16(Console.ReadLine());
    }
    else if (fieldChoice == 9)
    {
      Console.WriteLine("Is the product discontinued? (y/n):");
      string discontinuedInput = Console.ReadLine()!;
      product.Discontinued = discontinuedInput.ToLower() == "y" ? true : false;
    }
    db.SaveChanges();
    logger.Info("Product updated successfully");
  }
  else if (choice == "8")
  {
    // Display a specific Product 
    Console.WriteLine("What is the Product ID you want to display?");
    var db = new DataContext();
    db.Products.OrderBy(p => p.ProductId).ToList().ForEach(p => Console.WriteLine($"{p.ProductId}) {p.ProductName}"));
    int id = int.Parse(Console.ReadLine()!);
    logger.Info($"Product ID {id} selected");
    
    Product product = db.Products.FirstOrDefault(p => p.ProductId == id)!;
    Console.WriteLine($"Product ID: {product.ProductId}\nProduct Name: {product.ProductName}\nSupplier ID: {product.SupplierId}\nCategory ID: {product.CategoryId}\nQuantity per unit: {product.QuantityPerUnit}\nUnit Price: {product.UnitPrice}\nUnits in stock: {product.UnitsInStock}\nUnits on order: {product.UnitsOnOrder}\nReorder level: {product.ReorderLevel}\nDiscontinued: {product.Discontinued}");
    
  }
  else if (choice == "9")
  {
    // Edit a Category
     var db = new DataContext();
    var query = db.Categories.OrderBy(c => c.CategoryId);

    Console.WriteLine("Select the category you want to edit:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    Console.ForegroundColor = ConsoleColor.White;
    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"Category {id} selected");
    Category category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
    Console.WriteLine("Enter new Category Name:");
    category.CategoryName = Console.ReadLine()!;
    Console.WriteLine("Enter the new Category Description:");
    category.Description = Console.ReadLine();
    db.SaveChanges();
    logger.Info("Category updated successfully");
  }
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
