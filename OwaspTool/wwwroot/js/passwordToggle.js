const EYE_OPEN = `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
    fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
    <circle cx="12" cy="12" r="3"/>
</svg>`;

const EYE_CLOSED = `<svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24"
    fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
    <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"/>
    <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"/>
    <line x1="1" y1="1" x2="23" y2="23"/>
</svg>`;

export function initPasswordToggles() {
    document.querySelectorAll('.authlib-reset input[type="password"]').forEach(input => {
        // Evita di wrappare due volte
        if (input.parentNode.classList.contains('cc-input-wrapper')) return;

        // Crea un wrapper stretto attorno al solo input
        const inputWrapper = document.createElement('div');
        inputWrapper.className = 'cc-input-wrapper';
        inputWrapper.style.cssText = 'position:relative; display:block;';

        // Inserisci il wrapper al posto dell'input, poi sposta l'input dentro
        input.parentNode.insertBefore(inputWrapper, input);
        inputWrapper.appendChild(input);

        // Aggiungi il bottone dentro il wrapper (non nel .form-floating)
        const btn = document.createElement('span');
        btn.className = 'cc-eye-toggle';
        btn.innerHTML = EYE_OPEN;
        Object.assign(btn.style, {
            position: 'absolute',
            right: '12px',
            top: '50%',
            transform: 'translateY(-50%)',
            zIndex: '9999',
            cursor: 'pointer',
            color: '#07193e',
            lineHeight: '0',
            userSelect: 'none'
        });

        input.style.paddingRight = '45px';
        inputWrapper.appendChild(btn);

        btn.addEventListener('pointerdown', (e) => {
            e.preventDefault();
            e.stopImmediatePropagation();
            const show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            btn.innerHTML = show ? EYE_CLOSED : EYE_OPEN;
            btn.style.lineHeight = '0';
        });
    });
}
