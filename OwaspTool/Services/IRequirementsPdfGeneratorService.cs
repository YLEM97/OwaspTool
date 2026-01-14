using OwaspTool.DTOs;

namespace OwaspTool.Services
{
    public interface IRequirementsPdfGeneratorService
    {
        byte[] CreatePdf(List<RequirementDTO> requirements, string applicationName);
        byte[] CreatePdfV2(Dictionary<ChapterDTO, Dictionary<SectionDTO, List<RequirementDTO>>> groupedRequirements, string applicationName);
    }
}
