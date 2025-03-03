using AutoMapper;
using Kbcprojects.Entities;
using Kbcprojects.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kbcprojects.Controllers
{
    [Authorize]
    public class GameSessionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GameSessionController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var gameSessions = await _context.GameSessions
                .Include(gs => gs.User)
                .OrderByDescending(gs => gs.StartedAt)
                .ToListAsync();

            var gameResultViewModels = _mapper.Map<List<GameResultViewModel>>(gameSessions);
            return View(gameResultViewModels);
        }

        public async Task<IActionResult> MySessions()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var userSessions = await _context.GameSessions
                .Where(gs => gs.UserId == userId)
                .OrderByDescending(gs => gs.StartedAt)
                .ToListAsync();

            var gameResultViewModels = _mapper.Map<List<GameResultViewModel>>(userSessions);
            return View(gameResultViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGameSession(int score, DateTime startTime)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value);
            var endTime = DateTime.Now;
            var timeTaken = endTime - startTime;

            var gameSession = new GameSession
            {
                UserId = userId,
                Score = score,
                StartedAt = startTime,
                EndedAt = endTime,
                TimeTaken = timeTaken
            };

            _context.GameSessions.Add(gameSession);
            await _context.SaveChangesAsync();

            var gameResultViewModel = _mapper.Map<GameResultViewModel>(gameSession);
            return View("GameOver", gameResultViewModel);
        }
    }
}


