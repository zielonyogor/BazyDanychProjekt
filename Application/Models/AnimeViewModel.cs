﻿using System.ComponentModel.DataAnnotations;

namespace Application.Models
{
    public class AnimeViewModel
    {
        // People are not supposed to write ID into POST request, but it is useful for GET
        public int Id { get; set; }

		[Required(ErrorMessage = "Name is required.")]
		[StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
		public string Name { get; set; } = null!;

        public string? Status { get; set; }

        public int Count { get; set; }

        public string? Poster { get; set; }

        public DateTime? Publishdate { get; set; }

        public string? Description { get; set; }

        public string? Type { get; set; }

		[Required(ErrorMessage = "Studio is required")]
		public string Studioname { get; set; } = null!;

        public virtual List<string> Genrenames { get; set; } = new List<string>();

        public virtual List<int> Connections { get; set; } = new List<int>();
		public List<ReviewViewModel> Reviews { get; set; } = new(); // only for Mangas details let's hope
	}
}
