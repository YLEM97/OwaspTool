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
                // Use the ChapterDTO constructor to populate ChapterID, Number and Title
                Chapter = new ChapterDTO(R.Chapter);
            }

            if (R.Section != null)
            {
                // Use the SectionDTO constructor to populate SectionID, Number and Title
                Section = new SectionDTO(R.Section);
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
