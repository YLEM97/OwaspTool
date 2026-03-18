using System.Collections.Generic;
using OwaspTool.DTOs;

namespace OwaspTool.Services
{
    public interface ITestsPdfGeneratorService
    {
        /// <summary>
        /// Crea un PDF a partire dai test raggruppati per capitolo.
        /// La chiave del dizionario è il capitolo (WSTGChapterDTO), il valore è la lista dei test (WSTGTestDTO).
        /// </summary>
        byte[] CreatePdf(Dictionary<DTOs.WSTGChapterDTO, List<DTOs.WSTGTestDTO>> groupedTests, string applicationName);
    }
}
