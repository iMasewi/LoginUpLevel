﻿namespace LoginUpLevel.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
