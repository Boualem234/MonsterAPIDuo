using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLittleRPG_ElGuendouz.Data.Context;
using MyLittleRPG_ElGuendouz.Models;

namespace MyLittleRPG_ElGuendouz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstanceMonstresController : ControllerBase
    {
        private readonly MonsterContext _context;

        public InstanceMonstresController(MonsterContext context)
        {
            _context = context;
        }

        private bool InstanceMonstreExists(int id)
        {
            return _context.InstanceMonstre.Any(e => e.PositionX == id);
        }
    }
}
