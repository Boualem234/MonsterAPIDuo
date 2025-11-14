using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Controllers
{
    [Route("api/[controller]")]
    public class QuestController : Controller
    {
        private readonly MonsterContext _context;

        public QuestController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Quest>>> Get()
        {
            return Ok(_context.Quest.ToList());
        }

        [HttpGet("Get/{id}")]
        public async Task<ActionResult<List<Quest>>> GetByCharacterId(int id)
        {
            if (!_context.Character.Any(c => c.idPersonnage == id)) return NotFound();

            else
            {
                return Ok(_context.Quest.Where(q => q.idPersonnage == id).ToList());
            }
        }
    }
}
