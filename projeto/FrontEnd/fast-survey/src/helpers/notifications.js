// Funções para notificações e feedback do usuário

export function showNotification(title, options = {}) {
    if (!('Notification' in window)) {
        console.warn('Este navegador não suporta notificações desktop');
        return;
    }

    if (Notification.permission === 'granted') {
        return new Notification(title, {
            icon: '/icons/icon-512.png',
            badge: '/icons/icon-512.png',
            ...options
        });
    } else if (Notification.permission !== 'denied') {
        Notification.requestPermission().then(permission => {
            if (permission === 'granted') {
                return new Notification(title, {
                    icon: '/icons/icon-512.png',
                    badge: '/icons/icon-512.png',
                    ...options
                });
            }
        });
    }
}

export function requestNotificationPermission() {
    if (!('Notification' in window)) {
        return Promise.resolve(false);
    }

    if (Notification.permission === 'granted') {
        return Promise.resolve(true);
    }

    if (Notification.permission === 'denied') {
        return Promise.resolve(false);
    }

    return Notification.requestPermission().then(permission => {
        return permission === 'granted';
    });
}

export function vibrate(pattern = 200) {
    if ('vibrate' in navigator) {
        navigator.vibrate(pattern);
    }
}

export function playSound(audioUrl) {
    const audio = new Audio(audioUrl);
    audio.play().catch(error => {
        console.warn('Erro ao reproduzir áudio:', error);
    });
}

export function showToast(message, type = 'info', duration = 3000) {
    // Esta função pode ser integrada com react-toastify ou outra biblioteca
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 4px;
        color: white;
        font-weight: 500;
        z-index: 10000;
        animation: slideIn 0.3s ease;
        max-width: 300px;
        word-wrap: break-word;
    `;

    // Estilos baseados no tipo
    const styles = {
        success: 'background-color: #4caf50;',
        error: 'background-color: #f44336;',
        warning: 'background-color: #ff9800;',
        info: 'background-color: #2196f3;'
    };

    toast.style.cssText += styles[type] || styles.info;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => {
            if (toast.parentNode) {
                document.body.removeChild(toast);
            }
        }, 300);
    }, duration);
}

export function showLoading(message = 'Carregando...') {
    const loading = document.createElement('div');
    loading.id = 'global-loading';
    loading.innerHTML = `
        <div class="loading-overlay">
            <div class="loading-spinner"></div>
            <p>${message}</p>
        </div>
    `;
    loading.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
    `;

    const overlay = loading.querySelector('.loading-overlay');
    overlay.style.cssText = `
        background: white;
        padding: 30px;
        border-radius: 8px;
        text-align: center;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    `;

    const spinner = loading.querySelector('.loading-spinner');
    spinner.style.cssText = `
        width: 40px;
        height: 40px;
        border: 4px solid #f3f3f3;
        border-top: 4px solid #3498db;
        border-radius: 50%;
        animation: spin 1s linear infinite;
        margin: 0 auto 15px;
    `;

    document.body.appendChild(loading);
    return loading;
}

export function hideLoading() {
    const loading = document.getElementById('global-loading');
    if (loading) {
        loading.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => {
            if (loading.parentNode) {
                document.body.removeChild(loading);
            }
        }, 300);
    }
}

export function showConfirmDialog(message, onConfirm, onCancel) {
    const dialog = document.createElement('div');
    dialog.innerHTML = `
        <div class="confirm-dialog">
            <div class="confirm-content">
                <p>${message}</p>
                <div class="confirm-buttons">
                    <button class="btn-cancel">Cancelar</button>
                    <button class="btn-confirm">Confirmar</button>
                </div>
            </div>
        </div>
    `;

    dialog.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 10000;
    `;

    const content = dialog.querySelector('.confirm-content');
    content.style.cssText = `
        background: white;
        padding: 30px;
        border-radius: 8px;
        max-width: 400px;
        text-align: center;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
    `;

    const buttons = dialog.querySelector('.confirm-buttons');
    buttons.style.cssText = `
        display: flex;
        gap: 10px;
        justify-content: center;
        margin-top: 20px;
    `;

    const cancelBtn = dialog.querySelector('.btn-cancel');
    const confirmBtn = dialog.querySelector('.btn-confirm');

    cancelBtn.style.cssText = `
        padding: 8px 16px;
        border: 1px solid #ddd;
        background: white;
        border-radius: 4px;
        cursor: pointer;
    `;

    confirmBtn.style.cssText = `
        padding: 8px 16px;
        border: none;
        background: #f44336;
        color: white;
        border-radius: 4px;
        cursor: pointer;
    `;

    cancelBtn.onclick = () => {
        document.body.removeChild(dialog);
        if (onCancel) onCancel();
    };

    confirmBtn.onclick = () => {
        document.body.removeChild(dialog);
        if (onConfirm) onConfirm();
    };

    document.body.appendChild(dialog);
    return dialog;
}

export function showAlert(message, type = 'info') {
    return showConfirmDialog(message, () => {}, () => {});
}
