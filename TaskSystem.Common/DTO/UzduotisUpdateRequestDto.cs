namespace TaskSystem.Common.DTO
{
    public class UzduotisUpdateRequestDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int StatusId { get; set; }
    }
}
