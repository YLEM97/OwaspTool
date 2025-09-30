using OwaspTool.Models.Database;

namespace OwaspTool.DTOs
{
    public class ChapterDTO
    {
        public ChapterDTO() { }
        public ChapterDTO(Chapter C)
        {
            ChapterID = C.ChapterID;
            Number = C.Number;
            Title = C.Title;
        }

        public override bool Equals(object? obj)
        {
            return obj is ChapterDTO dto &&
                   Number == dto.Number &&
                   Title == dto.Title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Number, Title);
        }
        public int ChapterID { get; set; }
        public string? Number { get; set; }
        public string? Title { get; set; }
    }
}
