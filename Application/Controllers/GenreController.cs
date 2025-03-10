﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Data;
using Application.Models;
using Microsoft.AspNetCore.Authorization;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly ModelContext _context;

        public GenreController(ModelContext context)
        {
            _context = context;
        }

        // GET: api/Genre
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGenres([FromQuery] string? search)
        {
            var genres = await _context.Genres
                .Where(g => string.IsNullOrEmpty(search) || g.Name.ToLower().StartsWith(search.ToLower()))
                .Select(g => new
                {
                    g.Name,
                    Media = g.Idmedia.Select(m => m.Id).ToList()
                })
                .ToListAsync();

            return Ok(genres);
        }

        // GET: api/Genre/shonen
        [HttpGet("{name}")]
        public async Task<ActionResult<object>> GetGenre(string name)
        {
            var genre = await _context.Genres
                .Select(g => new
                {
                    g.Name,
                    Media = g.Idmedia.Select(m => m.Id).ToList()
                })
                .FirstOrDefaultAsync(g => g.Name == name);

            if (genre == null)
            {
                return NotFound();
            }

            return genre;
        }

        // PUT: api/Genre/shonen
        [HttpPut("{name}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> PutGenre(string name, Genre genre)
        {
            if (name != genre.Name)
            {
                return BadRequest();
            }

            _context.Entry(genre).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenreExists(name))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Genre
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<ActionResult<Genre>> PostGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GenreExists(genre.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetGenre", new { name = genre.Name }, genre);
        }

        // DELETE: api/Genre/shonen
        [HttpDelete("{name}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> DeleteGenre(string name)
        {
            var genre = await _context.Genres.FindAsync(name);
            if (genre == null)
            {
                return NotFound();
            }

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool GenreExists(string name)
        {
            return _context.Genres.Any(e => e.Name == name);
        }
    }
}
