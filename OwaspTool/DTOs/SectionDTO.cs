using OwaspTool.Models.Database;

namespace OwaspTool.DTOs
{
    public class SectionDTO
    {
        public SectionDTO() { }
        public SectionDTO(Section S)
        {
            SectionID = S.SectionID;
            Number = S.Number;
            Title = S.Title;
        }

        public override bool Equals(object? obj)
        {
            return obj is SectionDTO dto &&
                   Number == dto.Number &&
                   Title == dto.Title;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Number, Title);
        }
        public int SectionID { get; set; }
        public string? Number { get; set; }
        public string? Title { get; set; }
    }
}
