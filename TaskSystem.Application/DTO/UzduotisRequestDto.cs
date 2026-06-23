namespace TaskSystem.Application.DTO
{
    public class UzduotisRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int StatusId { get; set; }
    }
}
