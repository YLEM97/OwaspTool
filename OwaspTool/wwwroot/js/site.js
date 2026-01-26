window.owaspAiChat = {
    /**
     * Avvia una chat con Grok passando il requisito + domanda dell'utente.
     * @param {string} requirementNumber
     * @param {string} requirementText
     * @param {string} userQuestion
     */
    askRequirementHelp: async function (requirementNumber, requirementText, userQuestion) {
        if (!window.puter || !window.puter.ai || !userQuestion?.trim()) {
            return "AI not available or empty question.";
        }

        const prompt = `
You are an application security expert helping a developer implement OWASP ASVS requirements.

Requirement:
- ID: ${requirementNumber}
- Text: ${requirementText}

User question about this requirement:
${userQuestion}

Provide:
1) A short explanation (max 4-5 lines).
2) Concrete implementation suggestions (C#/.NET and web app context if possible).
3) If relevant, common pitfalls and checks to verify compliance.
Use concise, technical language.
`;

        const response = await window.puter.ai.chat(
            prompt,
            {
                model: 'x-ai/grok-4.1-fast', // modello indicato nella guida Grok+Puter.js
                temperature: 0.3,
                max_tokens: 400
            }
        ); // [page:0]

        return response?.message?.content ?? "No answer from AI.";
    }
};
