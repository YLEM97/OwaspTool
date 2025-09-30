using OwaspTool.Models.Database;
using OwaspTool.DAL;

namespace OwaspTool.DTOs
{
    public class RequirementDTO
    {
        public RequirementDTO() { }
        public RequirementDTO(ASVSRequirement R)
        {
            ASVSRequirementID = R.ASVSRequirementID;
            Number = R.Number;
            Text = R.Text;
            ChapterID = R.ChapterID;
            SectionID = R.SectionID;

            if (R.Chapter != null)
            {
                Chapter = new ChapterDTO
                {
                    Number = R.Chapter.Number,
                    Title = R.Chapter.Title
                };
            }

            if (R.Section != null)
            {
                Section = new SectionDTO
                {
                    Number = R.Section.Number,
                    Title = R.Section.Title
                };
            }
        }
        public int ASVSRequirementID { get; set; }
        public string? Number { get; set; }
        public string? Text { get; set; }
        public int ChapterID { get; set; }
        public int SectionID { get; set; }
        public ChapterDTO? Chapter { get; set; }
        public SectionDTO? Section { get; set; }
    }
}
