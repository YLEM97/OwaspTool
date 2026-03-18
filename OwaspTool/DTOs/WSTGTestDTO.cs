using OwaspTool.Models.Database;

namespace OwaspTool.DTOs
{
    public class WSTGTestDTO
    {
        public WSTGTestDTO() { }

        public WSTGTestDTO(WSTGTest t)
        {
            WSTGTestID = t.WSTGTestID;
            WSTGChapterID = t.WSTGChapterID;
            Number = t.Number;
            NumberWSTG = t.NumberWSTG;
            Title = t.Title;
            Text = t.Text;
            Link = t.Link;
            Active = t.Active ?? false;

            if (t.WSTGChapter != null)
                Chapter = new WSTGChapterDTO(t.WSTGChapter);
        }

        public int WSTGTestID { get; set; }
        public int WSTGChapterID { get; set; }
        public string? Number { get; set; }
        public string? NumberWSTG { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? TextHtml { get; set; } // popolato dal componente
        public string? Link { get; set; }
        public bool Active { get; set; }
        public WSTGChapterDTO? Chapter { get; set; }

        /// <summary>
        /// Status values:
        /// 0 = Not started
        /// 1 = Pass
        /// 2 = Issues
        /// 3 = Not Applicable
        /// Nullable: null = not set (radio initially unselected)
        /// </summary>
        public int? TestStatus { get; set; }

        // Note salvata dall'utente (nullable)
        public string? Notes { get; set; }

        // AiNotes se presenti (nullable)
        public string? AiNotes { get; set; }
    }
}