using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieTrivia.Data;
using MovieTrivia.Models;

namespace MovieTrivia.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int TotalQuestions = 10; // number of questions per game

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Start page to enter player name
        public IActionResult Start()
        {
            return View();
        }

        // POST: Save player name and start quiz
        [HttpPost]
        public IActionResult StartGame(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                TempData["Error"] = "Please enter your name!";
                return RedirectToAction("Start");
            }

            // Store player name in session
            HttpContext.Session.SetString("playerName", playerName);

            // Reset score and question count
            HttpContext.Session.SetInt32("score", 0);
            HttpContext.Session.SetInt32("questionsAnswered", 0);

            return RedirectToAction("Index");
        }

        // GET: Show a quiz question
        public async Task<IActionResult> Index()
        {
            var count = await _context.MovieQuotes.CountAsync();
            if (count == 0)
            {
                return Content("No questions found in the database!");
            }

            var random = new Random();
            var quote = await _context.MovieQuotes
                                      .Skip(random.Next(count))
                                      .FirstOrDefaultAsync();

            var options = new List<string> { quote.CorrectMovie, quote.Option1, quote.Option2, quote.Option3 }
                .Distinct()
                .ToList();
            ViewBag.ShuffledOptions = options.OrderBy(x => random.Next()).ToList();


            var score = HttpContext.Session.GetInt32("score") ?? 0;
            ViewBag.Score = score;

            var playerName = HttpContext.Session.GetString("playerName") ?? "Player1";
            ViewBag.PlayerName = playerName;

            return View(quote);
        }

        // POST: Handle player's guess
        [HttpPost]
        public IActionResult Guess(int quoteId, string selectedMovie)
        {
            var quote = _context.MovieQuotes.Find(quoteId);
            int score = HttpContext.Session.GetInt32("score") ?? 0;
            int questionsAnswered = HttpContext.Session.GetInt32("questionsAnswered") ?? 0;

            if (quote.CorrectMovie == selectedMovie)
            {
                score++;
                TempData["Feedback"] = "Correct! 🎉";
            }
            else
            {
                TempData["Feedback"] = $"Sorry, the correct answer was '{quote.CorrectMovie}'.";
            }

            questionsAnswered++;

            // Update session values
            HttpContext.Session.SetInt32("score", score);
            HttpContext.Session.SetInt32("questionsAnswered", questionsAnswered);

            // Check if game is finished
            if (questionsAnswered >= TotalQuestions)
            {
                string playerName = HttpContext.Session.GetString("playerName") ?? "Player1";

                // Save final score to SQL Server
                _context.Leaderboard.Add(new Leaderboard
                {
                    PlayerName = playerName,
                    Score = score,
                    DatePosted = DateTime.Now
                });
                _context.SaveChanges();

                // Clear session for next game
                HttpContext.Session.Remove("score");
                HttpContext.Session.Remove("questionsAnswered");

                return RedirectToAction("Leaderboard");
            }

            return RedirectToAction("Index");
        }

        // GET: Show top scores
        public async Task<IActionResult> Leaderboard()
        {
            var highScores = await _context.Leaderboard
                                           .OrderByDescending(s => s.Score)
                                           .Take(10)
                                           .ToListAsync();
            return View(highScores);
        }
    }
}
