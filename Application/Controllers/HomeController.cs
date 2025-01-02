using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Application.Models;
using Application.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

public class HomeController : Controller
{

    private readonly ILogger<HomeController> _logger;
    private readonly ModelContext _context;

    private readonly SignInManager<Account> _signInManager;
    private readonly UserManager<Account> _userManager;

    public HomeController(ILogger<HomeController> logger, ModelContext context, SignInManager<Account> signInManager, UserManager<Account> userManager) // 
    {
        _logger = logger;
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
    }

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		if (!_signInManager.IsSignedIn(User))
		{
			return RedirectToAction("Login", "Account");
		}

		var user = _userManager.GetUserAsync(User).Result;
		int accountId = user.Id;

		ViewData["UserName"] = user.UserName;

        var listElements = await _context.Listelements
            .Where(le => le.Accountid == accountId)
            .Include(le => le.Medium)
            .Select(le => new
            {
                le.Accountid,
                le.Mediumid,
                le.Finishednumber,
                le.Status,
                le.Rating,
                le.Mediumcomment,
                le.Startdate,
                le.Finishdate,
                le.PostDate,
                Mediumname = le.Medium.Name,
                Mediumposter = le.Medium.Poster
            })
            .OrderByDescending(le => le.PostDate)
            .ToListAsync();

		return View(listElements);
	}

    [HttpGet]
    public IActionResult AddToList()
    {
        return View();
    }
 

    [HttpPost]
    public async Task<IActionResult> AddToList(ListElementViewModel newElement)
    {
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Login", "Account");
        }

        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            int accountId = user.Id;

            var medium = await _context.Media.Where(m => m.Id == newElement.Mediumid).FirstOrDefaultAsync();

            if (medium == null)
            {
                return View(newElement);
            }

            var listElement = new Listelement
            {
                Mediumid = newElement.Mediumid,
                Accountid = accountId,
                Status = newElement.Status,
                Finishednumber = newElement.Finishednumber,
                Startdate = newElement.Startdate,
                Finishdate = newElement.Finishdate,
                Rating = newElement.Rating,
                PostDate = DateTime.UtcNow,
                Account = user,
                Medium = medium
            };

            _context.Listelements.Add(listElement);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        foreach (var state in ModelState)
        {
            if (state.Value.Errors.Count > 0)
            {
                Console.WriteLine($"Key: {state.Key}");
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                    if (error.Exception != null)
                    {
                        Console.WriteLine($"Exception: {error.Exception.Message}");
                    }
                }
            }
        }
        return View(newElement);
    }

	[HttpGet]
	public async Task<IActionResult> EditElement(int mediumId)
	{
        if (!_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _userManager.GetUserAsync(User);
        int accountId = user.Id;

        Console.WriteLine($"Medium ID: {mediumId}, {accountId}");

        var element = await _context.Listelements.Where(e =>
            e.Mediumid ==  mediumId &&
            e.Accountid ==  accountId)
            .FirstOrDefaultAsync();

        if (element == null) {
            return RedirectToAction("Index");
        }

        var elementModel = new ListElementViewModel
        {
            Mediumid = element.Mediumid,
            Finishednumber = element.Finishednumber,
            Status = element.Status,
            Rating = element.Rating,
            Mediumcomment = element.Mediumcomment,
            Startdate = element.Startdate,
            Finishdate = element.Finishdate
        };

        return View(elementModel);
	}

    [HttpPost]
    public async Task<IActionResult> EditElement(ListElementViewModel model)
    {
		if (!ModelState.IsValid)
			return View(model);

        var medium = await _context.Media.Where(m => m.Id == model.Mediumid).FirstOrDefaultAsync();

		if (medium == null)
		{
            ModelState.AddModelError("Mediumid", "Could not find medium");
            return View(model);
		}

		return RedirectToAction("Index");
	}

	public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
