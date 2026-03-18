using OwaspTool.Models.Database;

namespace OwaspTool.DTOs
{
    public class WSTGChapterDTO
    {
        public WSTGChapterDTO() { }

        public WSTGChapterDTO(WSTGChapter c)
        {
            WSTGChapterID = c.WSTGChapterID;
            Number = c.Number;
            Title = c.Title;
        }

        public override bool Equals(object? obj)
        {
            return obj is WSTGChapterDTO dto &&
                   Number == dto.Number &&
                   Title == dto.Title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Number, Title);
        }

        public int WSTGChapterID { get; set; }
        public string? Number { get; set; }
        public string? Title { get; set; }
    }
}